<script setup lang="ts">
import { onMounted, ref, watch, computed } from 'vue'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useEnergyEntriesState } from '@/composables/vehicles/useEnergyEntriesState'
import { useNotificationsStore } from '@/stores/notifications'

import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto, EnergyStatsDto, EnergyType } from '@/api/generated/apiV1.schemas'

import EnergyEntriesList from '@/components/vehicles/energy/EnergyEntriesList.vue'
import EnergyEntriesTable from '@/components/vehicles/energy/EnergyEntriesTable.vue'
import EnergyStatCard from '@/components/vehicles/energy/EnergyStatCard.vue'

import EnergyEntryDialog from '@/components/vehicles/energy/EnergyEntryDialog.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

interface Props {
  vehicleId: string
  allowedEnergyTypes: EnergyType[] | undefined
  energystats: EnergyStatsDto | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'entry-changed': []
}>()

const { isMobile } = useResponsiveLayout()
const { getApiVehiclesVehicleIdEnergyEntries, deleteApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()
const { showEntryDialog, selectedEntry, openEditDialog, closeDialog } = useEnergyEntriesState()
const notifications = useNotificationsStore()

const energyEntries = ref<EnergyEntryDto[]>([])
const energyEntriesLoading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const itemsPerPage = ref(10)
const hasMoreRecords = ref(true)
const error = ref<string | null>(null)
const infiniteScrollKey = ref(0)

const selectedEnergyTypeFilters = ref<EnergyType[]>([])
const selectedEntryIds = ref<string[]>([])

const hasSelection = computed(() => selectedEntryIds.value.length > 0)
const selectedCount = computed(() => selectedEntryIds.value.length)

function clearSelection() {
  selectedEntryIds.value = []
}

function resetList() {
  page.value = 1
  energyEntries.value = []
  selectedEntryIds.value = []
  infiniteScrollKey.value++
}

async function loadEnergyEntries() {
  if (!props.vehicleId) return

  energyEntriesLoading.value = true
  error.value = null

  try {
    const response = await getApiVehiclesVehicleIdEnergyEntries(props.vehicleId, {
      page: page.value,
      pageSize: itemsPerPage.value,
      energyTypes: selectedEnergyTypeFilters.value.length > 0 ? selectedEnergyTypeFilters.value : undefined,
    })

    const fetchedItems = response.items ?? []
    const total = response.totalCount ?? 0

    if (isMobile.value && page.value > 1) {
      energyEntries.value = [...energyEntries.value, ...fetchedItems]
    } else {
      energyEntries.value = fetchedItems
    }

    totalCount.value = total
    hasMoreRecords.value = energyEntries.value.length < totalCount.value
  } catch (err) {
    console.error('Failed to load energy entries:', err)
    error.value = 'Failed to load data'
    if (page.value === 1) energyEntries.value = []
    hasMoreRecords.value = false
  } finally {
    energyEntriesLoading.value = false
  }
}

async function loadMore({ done }: { done: (status: 'ok' | 'empty' | 'error') => void }) {
  if (energyEntriesLoading.value) {
    done('ok')
    return
  }

  page.value = energyEntries.value.length > 0 ? page.value + 1 : 1

  await loadEnergyEntries()

  if (error.value) {
    done('error')
  } else if (!hasMoreRecords.value) {
    done('empty')
  } else {
    done('ok')
  }
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  loadEnergyEntries()
}

const handlePageSizeChange = (newSize: number) => {
  itemsPerPage.value = newSize
  page.value = 1
  loadEnergyEntries()
}

function onEntrySaved() {
  resetList()
  if (!isMobile.value) loadEnergyEntries()
  emit('entry-changed')
}

const entryToDeleteId = ref<string | null>(null)
const showSingleDeleteDialog = ref(false)
const showBulkDeleteDialog = ref(false)

function openSingleDeleteDialog(id: string | undefined) {
  if (!id) return
  entryToDeleteId.value = id
  showSingleDeleteDialog.value = true
}

async function confirmSingleDelete() {
  if (!entryToDeleteId.value) return

  try {
    await deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, entryToDeleteId.value)
    notifications.show('Fuel entry deleted successfully.')
    resetList()
    if (!isMobile.value) loadEnergyEntries()
    emit('entry-changed')
  } catch (err) {
    console.error('Delete failed', err)
  } finally {
    showSingleDeleteDialog.value = false
    entryToDeleteId.value = null
  }
}

async function confirmBulkDelete() {
  if (selectedEntryIds.value.length === 0) return

  const idsToDelete = [...selectedEntryIds.value]
  showBulkDeleteDialog.value = false
  resetList()

  try {
    await Promise.all(idsToDelete.map((id) => deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, id)))
    notifications.show('Fuel entries deleted successfully.')
    if (!isMobile.value) loadEnergyEntries()
    emit('entry-changed')
  } catch (err) {
    console.error('Delete failed', err)
  }
}

onMounted(() => {
  if (!isMobile.value) {
    loadEnergyEntries()
  }
})

watch(
  () => props.vehicleId,
  () => {
    resetList()
    loadEnergyEntries()
  },
)

watch(selectedEnergyTypeFilters, () => {
  resetList()
  loadEnergyEntries()
})
</script>

<template>
  <template v-if="isMobile">
    <div class="topbar-container">
      <v-fade-transition mode="out-in" duration="200">
        <div v-if="hasSelection" class="context-bar d-flex align-center w-100 rounded-pill px-2 py-1" key="context-bar">
          <v-tooltip text="Clear Selection" location="bottom" open-delay="200" close-delay="500">
            <template #activator="{ props }">
              <v-btn v-bind="props" icon="mdi-close" variant="text" @click="clearSelection"></v-btn>
            </template>
          </v-tooltip>

          <div class="text-subtitle-1 font-weight-medium ml-2">{{ selectedCount }} items selected</div>

          <v-spacer></v-spacer>
          <v-tooltip text="Delete selected fuel entries" location="bottom" open-delay="200" close-delay="500">
            <template #activator="{ props }">
              <v-btn v-bind="props" icon="mdi-delete" variant="text" color="error" @click="showBulkDeleteDialog = true"></v-btn>
            </template>
          </v-tooltip>
        </div>

        <div v-else class="d-flex justify-space-between align-center w-100 py-1" key="standard-bar">
          <v-spacer />
          <v-btn icon="mdi-filter-variant" variant="text" disabled></v-btn>
        </div>
      </v-fade-transition>
    </div>

    <v-infinite-scroll :key="infiniteScrollKey" :onLoad="loadMore" :items="energyEntries">
      <EnergyEntriesList v-model="selectedEntryIds" :items="energyEntries" @delete="openSingleDeleteDialog" />
      <template v-slot:empty>
        <div class="pa-4 text-center text-medium-emphasis text-caption">No more records</div>
      </template>
    </v-infinite-scroll>
  </template>

  <template v-else>
    <v-row class="equal-height-row">
      <v-col cols="12">
        <v-card class="fuel-card" variant="flat" rounded="md-16px">
          <v-row class="fuel-container-row no-gutters mx-4 my-4" dense>
            <div class="table-container-flex">
              <div class="topbar-container mb-3">
                <v-fade-transition mode="out-in" duration="200">
                  <div v-if="hasSelection" class="context-bar d-flex align-center w-100 rounded-pill px-2" key="context-bar">
                    <v-tooltip text="Clear Selection" location="bottom" open-delay="200" close-delay="500">
                      <template #activator="{ props }">
                        <v-btn v-bind="props" icon="mdi-close" variant="text" density="comfortable" @click="clearSelection" />
                      </template>
                    </v-tooltip>

                    <span class="text-subtitle-2 font-weight-medium ml-2">{{ selectedCount }} selected</span>

                    <v-spacer />

                    <v-tooltip text="Delete selected" location="bottom" open-delay="200" close-delay="500">
                      <template #activator="{ props }">
                        <v-btn
                          v-bind="props"
                          icon="mdi-delete"
                          variant="text"
                          color="error"
                          density="comfortable"
                          @click="showBulkDeleteDialog = true"
                        />
                      </template>
                    </v-tooltip>
                  </div>

                  <div v-else class="d-flex justify-space-between align-center w-100" key="standard-bar">
                    <v-spacer />
                    <v-chip-group v-model="selectedEnergyTypeFilters" multiple filter class="pr-1" selected-class="filter-chip-selected">
                      <v-chip
                        v-for="energyType in allowedEnergyTypes"
                        :key="energyType"
                        :value="energyType"
                        filter
                        variant="outlined"
                        class="filter-chip"
                      >
                        {{ energyType }}
                      </v-chip>
                    </v-chip-group>
                  </div>
                </v-fade-transition>
              </div>

              <EnergyEntriesTable
                :items="energyEntries"
                :total-count="totalCount"
                :loading="energyEntriesLoading"
                :page="page"
                :items-per-page="itemsPerPage"
                v-model:selectedIds="selectedEntryIds"
                @update:page="handlePageChange"
                @update:items-per-page="handlePageSizeChange"
                @edit="openEditDialog"
                @delete="(item) => openSingleDeleteDialog(item.id)"
              />
            </div>

            <div class="statistics-container-flex">
              <v-row>
                <v-col cols="6">
                  <EnergyStatCard
                    title="Average Consumption"
                    title-prepend-icon="mdi-gauge"
                    subtitle="This year"
                    value="8.2"
                    value-append="L/100km"
                    trend-icon="mdi-arrow-bottom-right"
                    trend="-0.5%"
                  />
                </v-col>
                <v-col cols="6">
                  <EnergyStatCard
                    title="Average Consumption"
                    title-prepend-icon="mdi-gauge"
                    subtitle="This year"
                    value="8"
                    value-append="L/100km"
                    trend-icon="mdi-arrow-bottom-right"
                    trend="+10%"
                  />
                </v-col>
              </v-row>
            </div>
          </v-row>
        </v-card>
      </v-col>
    </v-row>
  </template>

  <EnergyEntryDialog
    :model-value="showEntryDialog"
    :vehicleId="vehicleId"
    :entry="selectedEntry"
    :allowedEnergyTypes="allowedEnergyTypes"
    @update:model-value="(val) => !val && closeDialog()"
    @saved="onEntrySaved"
  />

  <DeleteDialog
    item-to-delete="fuel entry"
    :is-open="showSingleDeleteDialog"
    :on-confirm="confirmSingleDelete"
    :on-cancel="() => (showSingleDeleteDialog = false)"
  />
  <DeleteDialog
    :item-to-delete="selectedCount > 1 ? `${selectedCount} fuel entries` : `${selectedCount} fuel entry`"
    :is-open="showBulkDeleteDialog"
    :on-confirm="confirmBulkDelete"
    :on-cancel="() => (showBulkDeleteDialog = false)"
  />
</template>

<style scoped lang="scss">
.fuel-card {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.fuel-container-row {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  align-items: flex-start;
}

.table-container-flex {
  flex: 1 1 65%;
  min-width: 600px;
  max-width: 1400px;

  @media (max-width: 960px) {
    flex: 1 1 100%;
    min-width: 100%;
  }
}

.statistics-container-flex {
  flex: 1 1 300px;
  max-width: 600px;

  @media (max-width: 960px) {
    flex: 1 1 100%;
    max-width: 100%;
  }
}

.topbar-container {
  display: flex;
  align-items: center;
  min-height: 64px;
  padding: 0;
}

.context-bar {
  background-color: rgb(var(--v-theme-secondary-container));
  color: rgb(var(--v-theme-on-secondary-container));
}

.filter-chip {
  background-color: transparent !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-color: rgb(var(--v-theme-outline)) !important;
  border-radius: 8px !important;
  margin-top: 0px;
  margin-left: 8px;
  margin-right: 0px;
}

.filter-chip-selected {
  background-color: rgb(var(--v-theme-secondary-container)) !important;
  border-width: 0 !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-radius: 8px !important;
}

.filter-chip :deep(.v-icon) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.filter-chip-selected :deep(.v-icon) {
  color: rgb(var(--v-theme-on-secondary-container)) !important;
}

.equal-height-row {
  align-items: stretch;
}

.opacity-20 {
  opacity: 0.2;
}

.summary-card {
  transition: background-color 0.2s ease-in-out;
}

.summary-card:hover {
  background-color: rgb(var(--v-theme-surface-container));
}
</style>

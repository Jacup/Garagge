<script setup lang="ts">
import { onMounted, ref, watch, computed } from 'vue'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useEnergyEntriesState } from '@/composables/vehicles/useEnergyEntriesState'

import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto, EnergyStatsDto, EnergyType } from '@/api/generated/apiV1.schemas'

import EnergyEntriesList from '@/components/vehicles/energy/EnergyEntriesList.vue'
import EnergyEntriesTable from '@/components/vehicles/energy/EnergyEntriesTable.vue'
import EnergyStatisticsCard from '@/components/vehicles/energy/EnergyStatisticsCard.vue'

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

const energyEntries = ref<EnergyEntryDto[]>([])
const energyEntriesLoading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const itemsPerPage = ref(10)
const hasMoreRecords = ref(true)
const error = ref<string | null>(null)

const selectedEnergyTypeFilters = ref<EnergyType[]>([])

const selectedEntryIds = ref<string[]>([])

const hasSelection = computed(() => selectedEntryIds.value.length > 0)
const selectedCount = computed(() => selectedEntryIds.value.length)

function clearSelection() {
  selectedEntryIds.value = []
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
  if (energyEntriesLoading.value) return

  if (energyEntries.value.length > 0) {
    page.value++
  } else {
    page.value = 1
  }

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
  loadEnergyEntries()
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

    page.value = 1
    selectedEntryIds.value = []
    await loadEnergyEntries()
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

  try {
    const idsToDelete = [...selectedEntryIds.value]

    clearSelection()
    showBulkDeleteDialog.value = false

    await Promise.all(idsToDelete.map((id) => deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, id)))
    await loadEnergyEntries()
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
    page.value = 1
    energyEntries.value = []
    selectedEntryIds.value = []
    loadEnergyEntries()
  },
)

watch(selectedEnergyTypeFilters, () => {
  page.value = 1
  energyEntries.value = []
  selectedEntryIds.value = []
  loadEnergyEntries()
})
</script>

<template>
  <template v-if="!isMobile">
    <v-row class="mb-4">
      <v-col cols="6" sm="6" md="3" v-for="i in 4" :key="i">
        <v-card class="summary-card border-thin" height="110" color="surface-container-low" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary opacity-20" style="top: 12px; right: 16px" />
            <div class="text-caption text-medium-emphasis text-uppercase font-weight-bold mb-1">Total Cost</div>
            <div class="text-h5 font-weight-bold text-high-emphasis">2 450 PLN</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="equal-height-row">
      <v-col cols="12" md="8" xl="6">
        <v-card class="table-container pa-3" variant="flat" rounded="md-16px">
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
        </v-card>
      </v-col>

      <v-col cols="12" md="4" xl="6">
        <!-- <EnergyStatisticsCard
          :vehicle-id="vehicleId"
          :allowed-energy-types="energyTypes"
          :energystats="energystats"
          :stats-loading="statsLoading"
        /> -->
      </v-col>
    </v-row>
  </template>

  <template v-else>
    <v-infinite-scroll :onLoad="loadMore" :items="energyEntries">
      <EnergyEntriesList :items="energyEntries" @delete="openSingleDeleteDialog" />
      <template v-slot:empty>
        <div class="pa-4 text-center text-medium-emphasis text-caption">No more records</div>
      </template>
    </v-infinite-scroll>
  </template>

  <EnergyEntryDialog
    :model-value="showEntryDialog"
    :vehicleId="vehicleId"
    :entry="selectedEntry"
    :allowedEnergyTypes="allowedEnergyTypes"
    @update:model-value="val => !val && closeDialog()"
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

<style scoped>
.table-container {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 8px;
}

.topbar-container {
  display: flex;
  align-items: center;
  height: 48px;
  padding: 0;
}

.context-bar {
  background-color: rgb(var(--v-theme-secondary-container));
  color: rgb(var(--v-theme-on-secondary-container));
  height: 48px;
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

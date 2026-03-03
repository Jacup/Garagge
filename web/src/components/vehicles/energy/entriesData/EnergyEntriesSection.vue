<script lang="ts" setup>
import { ref, computed, watch, onMounted } from 'vue'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useEnergyEntriesState } from '@/composables/vehicles/useEnergyEntriesState'
import { useNotificationsStore } from '@/stores/notifications'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto, EnergyType } from '@/api/generated/apiV1.schemas'

import EnergyEntriesFilters from '@/components/vehicles/energy/entriesData/EnergyEntriesFilters.vue'
import EnergyEntriesList from '@/components/vehicles/energy/entriesData/EnergyEntriesList.vue'
import EnergyEntriesTable from '@/components/vehicles/energy/entriesData/EnergyEntriesTable.vue'
import EnergyEntryDialog from '@/components/vehicles/energy/EnergyEntryDialog.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

interface Props {
  vehicleId: string
  allowedEnergyTypes: EnergyType[] | undefined
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

function openEditDialogById(id: string | undefined) {
  const entry = energyEntries.value.find((e) => e.id === id)
  if (entry) openEditDialog(entry)

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
    notifications.show('Fuel entry deleted successfully.')
    resetList()
    loadEnergyEntries()
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
    loadEnergyEntries()
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
      <v-spacer />
      <v-fade-transition>
        <v-btn
          v-if="hasSelection"
          key="delete-btn"
          color="error"
          prepend-icon="mdi-delete"
          class="mr-2"
          height="32"
          @click="showBulkDeleteDialog = true"
        >
          Delete ({{ selectedCount }})
        </v-btn>
      </v-fade-transition>
      <div v-if="allowedEnergyTypes && allowedEnergyTypes.length > 1" class="d-flex align-center">
        <EnergyEntriesFilters
          :allowed-energy-types="allowedEnergyTypes"
          :model-value="selectedEnergyTypeFilters"
          @update:model-value="selectedEnergyTypeFilters = $event"
        />
      </div>
    </div>
    <v-infinite-scroll :key="infiniteScrollKey" @load="loadMore">
      <EnergyEntriesList
        :items="energyEntries"
        :model-value="selectedEntryIds"
        @update:model-value="selectedEntryIds = $event"
        @select="openEditDialogById"
        @delete="openSingleDeleteDialog"
      />
      <template #empty>
        <div class="py-4 text-center text-medium-emphasis text-caption">No more entries</div>
      </template>
      <template #error="{ props: retryProps }">
        <div class="pa-4 text-center">
          <span class="text-error text-caption mr-2">Failed to load entries.</span>
          <v-btn v-bind="retryProps" size="small" variant="tonal">Retry</v-btn>
        </div>
      </template>
    </v-infinite-scroll>
  </template>

  <template v-else>
    <v-card class="fuel-card" rounded="md-16px" variant="flat">
      <v-card-title class="d-flex flex-row align-center py-3">
        <span>Fuel entries</span>
        <v-spacer />
        <v-fade-transition>
          <v-btn
            v-if="hasSelection"
            key="delete-btn"
            color="error"
            prepend-icon="mdi-delete"
            class="mr-2"
            height="32"
            @click="showBulkDeleteDialog = true"
          >
            Delete ({{ selectedCount }})
          </v-btn>
        </v-fade-transition>
        <div v-if="allowedEnergyTypes && allowedEnergyTypes.length > 1" class="d-flex align-center">
          <EnergyEntriesFilters
            :allowed-energy-types="allowedEnergyTypes"
            :model-value="selectedEnergyTypeFilters"
            @update:model-value="selectedEnergyTypeFilters = $event"
          />
        </div>
      </v-card-title>

      <v-card-text>
        <EnergyEntriesTable
          :items="energyEntries"
          :total-count="totalCount"
          :loading="energyEntriesLoading"
          :page="page"
          :items-per-page="itemsPerPage"
          :selected-ids="selectedEntryIds"
          @update:selected-ids="selectedEntryIds = $event"
          @edit="(item) => openEditDialog(item)"
          @delete="(item) => openSingleDeleteDialog(item.id)"
          @update:page="handlePageChange"
          @update:items-per-page="handlePageSizeChange"
        />
      </v-card-text>
    </v-card>
  </template>

  <!-- ── Dialogs ── -->
  <EnergyEntryDialog
    :model-value="showEntryDialog"
    :vehicle-id="vehicleId"
    :entry="selectedEntry"
    :allowed-energy-types="allowedEnergyTypes"
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

<style lang="scss" scoped>
.fuel-card {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
.topbar-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  min-height: 40px;
  margin-bottom: 8px;
}

.context-bar {
  background-color: rgb(var(--v-theme-secondary-container));
  color: rgb(var(--v-theme-on-secondary-container));
}
</style>

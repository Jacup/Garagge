<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import ModifyEnergyEntryDialog from '@/components/vehicles/energyEntries/ModifyEnergyEntryDialog.vue'

interface Props {
  vehicleId: string
  allowedEnergyTypes?: string[] // Energy types allowed for this vehicle
  selected?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  allowedEnergyTypes: () => [],
  selected: () => []
})

// Define emits
const emit = defineEmits<{
  'update:selected': [value: string[]]
}>()

const { getApiVehiclesVehicleIdEnergyEntries, deleteApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()

// Energy entries data
const energyEntries = ref<EnergyEntryDto[]>([])
const energyEntriesLoading = ref(false)
const energyEntriesPage = ref(1)
const energyEntriesPageSize = ref(10)
const energyEntriesTotal = ref(0)
const error = ref<string | null>(null)

// Selection state - computed to sync with parent
const selectedItems = computed({
  get: () => props.selected,
  set: (value: string[]) => emit('update:selected', value)
})

// Dialog state
const editDialog = ref(false)
const deleteDialog = ref(false)
const selectedEntry = ref<EnergyEntryDto | null>(null)

// Table configuration
const energyHeaders = [
  { title: 'Date', key: 'date', value: 'date', sortable: false },
  { title: 'Mileage', key: 'mileage', value: 'mileage', sortable: false },
  { title: 'Type', key: 'type', value: 'type', sortable: false },
  { title: 'Volume', key: 'volume', value: 'volume', sortable: false },
  { title: 'Cost', key: 'cost', value: 'cost', sortable: false },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const },
]

// Utility functions
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

// Load energy entries from API
async function loadEnergyEntries() {
  if (!props.vehicleId) return

  energyEntriesLoading.value = true
  error.value = null
  try {
    const response = await getApiVehiclesVehicleIdEnergyEntries(props.vehicleId, {
      page: energyEntriesPage.value,
      pageSize: energyEntriesPageSize.value,
    })
    energyEntries.value = response.data.items ?? []
    energyEntriesTotal.value = response.data.totalCount ?? 0
  } catch (err) {
    console.error('Failed to load energy entries:', err)
    error.value = 'Nie udało się załadować danych tankowania'
    energyEntries.value = []
    energyEntriesTotal.value = 0
  } finally {
    energyEntriesLoading.value = false
  }
}

async function remove(id: string) {
  try {
    await deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, id)
    loadEnergyEntries()
  } catch (err) {
    console.error('Failed to delete energy entry:', err)
  }
}

// Bulk delete function
async function removeMultiple(ids: string[]) {
  try {
    await Promise.all(ids.map(id => deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, id)))
    // Clear selection after successful delete
    selectedItems.value = []
    loadEnergyEntries()
  } catch (err) {
    console.error('Failed to delete energy entries:', err)
  }
}

function openDeleteDialog(id: string) {
  selectedEntry.value = energyEntries.value.find((entry) => entry.id === id) || null
  deleteDialog.value = true
}

function closeDeleteDialog() {
  deleteDialog.value = false
  selectedEntry.value = null
}

async function confirmDelete() {
  if (selectedEntry.value?.id) {
    await remove(selectedEntry.value.id)
    deleteDialog.value = false
    selectedEntry.value = null
  }
}

function openEditDialog(entry: EnergyEntryDto) {
  selectedEntry.value = entry
  editDialog.value = true
}

function closeEditDialog() {
  editDialog.value = false
  selectedEntry.value = null
}

function handleEntrySaved() {
  closeEditDialog()
  loadEnergyEntries()
}

function handlePageUpdate(page: number) {
  energyEntriesPage.value = page
  loadEnergyEntries()
}

function handlePageSizeUpdate(pageSize: number) {
  energyEntriesPageSize.value = pageSize
  loadEnergyEntries()
}

onMounted(() => {
  loadEnergyEntries()
})

// Expose loadEnergyEntries function to parent components
defineExpose({
  loadEnergyEntries,
  removeMultiple
})

</script>

<template>
  <div>
        <v-alert
      v-if="error"
      type="error"
      density="compact"
      class="mb-4"
      closable
      @click:close="error = null"
    >
      {{ error }}
    </v-alert>

    <v-data-table-server
      v-model:items-per-page="energyEntriesPageSize"
      v-model="selectedItems"
      :headers="energyHeaders"
      :items="energyEntries"
      :items-length="energyEntriesTotal"
      :loading="energyEntriesLoading"
      :page="energyEntriesPage"
      @update:page="handlePageUpdate"
      @update:items-per-page="handlePageSizeUpdate"
      show-select
      density="compact"
      fixed-header
      disable-sort
      :height="407"
    >
      <template v-slot:[`item.date`]="{ item }">
        {{ formatDate(item.date) }}
      </template>
      <template v-slot:[`item.volume`]="{ item }"> {{ item.volume }} {{ item.energyUnit }} </template>
      <template v-slot:[`item.cost`]="{ item }">
        {{ item.cost ? item.cost.toFixed(2) + ' PLN' : 'N/A' }}
      </template>
      <template v-slot:[`item.actions`]="{ item }">
          <v-btn icon="mdi-pencil" variant="text" size="x-small" @click="openEditDialog(item)" />
          <v-btn icon="mdi-delete" variant="text" size="x-small" color="error" @click="openDeleteDialog(item.id)" />
      </template>
    </v-data-table-server>

    <DeleteDialog :is-open="deleteDialog" item-to-delete="energy entry" :on-confirm="confirmDelete" :on-cancel="closeDeleteDialog" />

    <ModifyEnergyEntryDialog
      :is-open="editDialog"
      :vehicle-id="vehicleId"
      :allowed-energy-types="allowedEnergyTypes"
      :entry="selectedEntry"
      :on-save="handleEntrySaved"
      :on-cancel="closeEditDialog"
    />
  </div>
</template>

<style scoped>
/* Make table background transparent */
:deep(.v-table) {
  background: transparent !important;
}

/* Table header styling */
:deep(.v-data-table__th) {
  background-color: rgba(var(--v-theme-primary), 0.12) !important;
  position: sticky !important;
  top: 0 !important;
  z-index: 2 !important;
}

/* Make header opaque for proper sticky behavior and rounded corners */
:deep(.v-data-table__thead) {
  background-color: rgba(var(--v-theme-surface), 1) !important;
}

/* Ensure only first and last header cells have proper radius */
:deep(.v-data-table__th:first-child) {
  border-top-left-radius: 8px !important;
}

:deep(.v-data-table__th:last-child) {
  border-top-right-radius: 8px !important;
}

</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import ModifyEnergyEntryDialog from '@/components/vehicles/energyEntries/ModifyEnergyEntryDialog.vue'

interface Props {
  vehicleId: string
}

const props = defineProps<Props>()

const { getApiVehiclesVehicleIdEnergyEntries, deleteApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()

// Energy entries data
const energyEntries = ref<EnergyEntryDto[]>([])
const energyEntriesLoading = ref(false)
const energyEntriesPage = ref(1)
const energyEntriesPageSize = ref(10)
const energyEntriesTotal = ref(0)

// Dialog state
const editDialog = ref(false)
const deleteDialog = ref(false)
const selectedEntry = ref<EnergyEntryDto | null>(null)

// Table configuration
const energyHeaders = [
  { title: 'Date', key: 'date', value: 'date' },
  { title: 'Mileage', key: 'mileage', value: 'mileage' },
  { title: 'Type', key: 'type', value: 'type' },
  { title: 'Volume', key: 'volume', value: 'volume' },
  { title: 'Cost', key: 'cost', value: 'cost' },
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
  try {
    const response = await getApiVehiclesVehicleIdEnergyEntries(props.vehicleId, {
      page: energyEntriesPage.value,
      pageSize: energyEntriesPageSize.value,
    })
    energyEntries.value = response.data.items ?? []
    energyEntriesTotal.value = response.data.totalCount ?? 0
  } catch (err) {
    console.error('Failed to load energy entries:', err)
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

function openDeleteDialog(id: string) {
  selectedEntry.value = energyEntries.value.find(entry => entry.id === id) || null
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

function openAddDialog() {
  selectedEntry.value = null
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

onMounted(() => {
  loadEnergyEntries()
})
</script>

<template>
  <v-card class="fuel-history-card card-background" height="400" variant="flat">
    <template #title>Fuel History</template>
    <template #append>
      <v-btn
        class="text-none"
        prepend-icon="mdi-plus"
        variant="flat"
        color="primary"
        @click="openAddDialog"
      >
        Add
      </v-btn>
    </template>
    <v-card-text class="pa-0 d-flex flex-column" style="height: calc(400px - 56px); flex: 1">
      <v-data-table-server
        v-model:items-per-page="energyEntriesPageSize"
        :headers="energyHeaders"
        :items="energyEntries"
        :items-length="energyEntriesTotal"
        :loading="energyEntriesLoading"
        :page="energyEntriesPage"
        @update:page="energyEntriesPage = $event; loadEnergyEntries()"
        @update:items-per-page="energyEntriesPageSize = $event; loadEnergyEntries()"
        density="compact"
        height="100%"
        style="flex: 1"
      >
        <template v-slot:[`item.date`]="{ item }">
          {{ formatDate(item.date) }}
        </template>
        <template v-slot:[`item.volume`]="{ item }">
          {{ item.volume }} {{ item.energyUnit }}
        </template>
        <template v-slot:[`item.cost`]="{ item }">
          {{ item.cost ? item.cost.toFixed(2) + ' PLN' : 'N/A' }}
        </template>
        <template v-slot:[`item.actions`]="{ item }">
          <div class="d-flex ga-2 justify-end">
            <v-btn
              icon="mdi-pencil"
              variant="text"
              size="x-small"
              @click="openEditDialog(item)"
            />
            <v-btn
              icon="mdi-delete"
              variant="text"
              size="x-small"
              @click="openDeleteDialog(item.id)"
            />
          </div>
        </template>
      </v-data-table-server>
    </v-card-text>
  </v-card>

  <DeleteDialog
    :is-open="deleteDialog"
    item-to-delete="energy entry"
    :on-confirm="confirmDelete"
    :on-cancel="closeDeleteDialog"
  />

  <ModifyEnergyEntryDialog
    :is-open="editDialog"
    :vehicle-id="vehicleId"
    :entry="selectedEntry"
    :on-save="handleEntrySaved"
    :on-cancel="closeEditDialog"
  />
</template>

<style scoped>
/* Table styling - full container height */
.fuel-history-card {
  height: 100% !important;
  background-color: transparent !important;
}

.fuel-history-card :deep(.v-data-table__th) {
  background-color: rgba(var(--v-theme-primary), 0.12) !important;
}

.fuel-history-card .v-data-table__wrapper {
  height: calc(400px - 56px) !important;
}
</style>

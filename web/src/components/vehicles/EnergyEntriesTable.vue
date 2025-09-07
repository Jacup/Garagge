<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto, EnergyUnit } from '@/api/generated/apiV1.schemas'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

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

function closeEditDialog() {
  editDialog.value = false
  selectedEntry.value = null
}

// Computed properties for form binding
const editForm = computed({
  get: () => ({
    date: selectedEntry.value?.date || '',
    mileage: selectedEntry.value?.mileage || 0,
    type: selectedEntry.value?.type || '',
    volume: selectedEntry.value?.volume || 0,
    energyUnit: selectedEntry.value?.energyUnit || '',
    cost: selectedEntry.value?.cost || 0,
  }),
  set: (value) => {
    if (selectedEntry.value) {
      selectedEntry.value.date = value.date
      selectedEntry.value.mileage = value.mileage
      selectedEntry.value.type = value.type
      selectedEntry.value.volume = value.volume
      selectedEntry.value.energyUnit = value.energyUnit as EnergyUnit
      selectedEntry.value.cost = value.cost
    }
  }
})

onMounted(() => {
  loadEnergyEntries()
})
</script>

<template>
  <v-card class="fuel-history-card card-background" height="400" variant="flat">
    <template #title>Fuel History</template>
    <template #append>
      <v-btn class="text-none" prepend-icon="mdi-plus" variant="flat" color="primary" disabled>Add</v-btn>
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
        <template v-slot:[`item.volume`]="{ item }"> {{ item.volume }} {{ item.energyUnit }} </template>
        <template v-slot:[`item.cost`]="{ item }">
          {{ item.cost ? item.cost.toFixed(2) + ' PLN' : 'N/A' }}
        </template>
        <template v-slot:[`item.actions`]="{ item }">
          <div class="d-flex ga-2 justify-end">
            <v-btn icon="mdi-pencil" variant="text" size="x-small" @click="openEditDialog(item)"></v-btn>
            <v-btn icon="mdi-delete" variant="text" size="x-small" @click="openDeleteDialog(item.id)"></v-btn>
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


  <!-- Edit Dialog -->
  <v-dialog v-model="editDialog" max-width="600px">
    <v-card>
      <v-card-title>
        <span class="text-h6">Edit Energy Entry</span>
      </v-card-title>
      <v-card-text>
        <v-container>
          <v-row>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="editForm.date"
                label="Date"
                type="date"
                variant="outlined"
              ></v-text-field>
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model.number="editForm.mileage"
                label="Mileage"
                type="number"
                variant="outlined"
              ></v-text-field>
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="editForm.type"
                label="Type"
                variant="outlined"
              ></v-text-field>
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model.number="editForm.volume"
                label="Volume"
                type="number"
                variant="outlined"
              ></v-text-field>
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="editForm.energyUnit"
                label="Unit"
                variant="outlined"
              ></v-text-field>
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model.number="editForm.cost"
                label="Cost"
                type="number"
                variant="outlined"
              ></v-text-field>
            </v-col>
          </v-row>
        </v-container>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="blue-darken-1" variant="text" @click="closeEditDialog">
          Cancel
        </v-btn>
        <v-btn color="blue-darken-1" variant="text" @click="closeEditDialog">
          Save
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
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

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { EnergyEntryDto, EnergyUnit, CreateEnergyEntryRequest, UpdateEnergyEntryRequest } from '@/api/generated/apiV1.schemas'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'

interface Props {
  isOpen: boolean
  vehicleId: string
  entry?: EnergyEntryDto | null
  onSave: () => void
  onCancel: () => void
}

const props = defineProps<Props>()

const { postApiVehiclesVehicleIdEnergyEntries, putApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()

const isLoading = ref(false)
const form = ref({
  date: '',
  mileage: 0,
  type: '',
  volume: 0,
  energyUnit: 'Liter' as EnergyUnit,
  cost: 0,
})

const energyUnitOptions: EnergyUnit[] = ['Liter', 'Gallon', 'CubicMeter', 'kWh']

const isEditMode = computed(() => !!props.entry?.id)
const dialogTitle = computed(() => (isEditMode.value ? 'Edit Energy Entry' : 'Add Energy Entry'))

// Calculate price per unit automatically
const calculatePricePerUnit = computed(() => {
  if (form.value.cost && form.value.volume && form.value.volume > 0) {
    return (form.value.cost / form.value.volume).toFixed(2)
  }
  return '0.00'
})

// Reset form when dialog opens/closes or entry changes
watch(
  [() => props.isOpen, () => props.entry],
  () => {
    if (props.isOpen) {
      if (props.entry) {
        // Edit mode - populate form with existing data
        form.value = {
          date: props.entry.date || '',
          mileage: props.entry.mileage || 0,
          type: props.entry.type || '',
          volume: props.entry.volume || 0,
          energyUnit: props.entry.energyUnit || 'Liter',
          cost: props.entry.cost || 0,
        }
      } else {
        // Add mode - reset form
        form.value = {
          date: new Date().toISOString().split('T')[0], // Today's date
          mileage: 0,
          type: '',
          volume: 0,
          energyUnit: 'Liter',
          cost: 0,
        }
      }
    }
  },
  { immediate: true },
)

async function handleSave() {
  if (!props.vehicleId) return

  isLoading.value = true
  try {
    if (isEditMode.value && props.entry?.id) {
      // Update existing entry
      const updateCommand: UpdateEnergyEntryRequest = {
        date: form.value.date,
        mileage: form.value.mileage,
        type: form.value.type,
        volume: form.value.volume,
        energyUnit: form.value.energyUnit,
        cost: form.value.cost || null,
        pricePerUnit: null, // Optional field
      }
      await putApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, props.entry.id, updateCommand)
    } else {
      // Create new entry
      const createCommand: CreateEnergyEntryRequest = {
        date: form.value.date,
        mileage: form.value.mileage,
        type: form.value.type,
        volume: form.value.volume,
        energyUnit: form.value.energyUnit,
        cost: form.value.cost || null,
        pricePerUnit: null, // Optional field
      }
      await postApiVehiclesVehicleIdEnergyEntries(props.vehicleId, createCommand)
    }
    props.onSave()
  } catch (error) {
    console.error('Failed to save energy entry:', error)
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <v-dialog :model-value="isOpen" class="dialog-container" @update:model-value="props.onCancel">
    <v-card variant="flat" class="dialog-card" rounded="xl" elevation="6">
      <template v-slot:title>
        {{ dialogTitle }}
      </template>

      <template v-slot:text>
        <v-container class="pa-0">
          <div class="section-container">
            <v-row class="mt-2">
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="form.date"
                  label="Date"
                  type="date"
                  variant="outlined"
                  density="comfortable"
                  prepend-inner-icon="mdi-calendar"
                  required
                />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model.number="form.mileage"
                  label="Mileage (km)"
                  type="number"
                  variant="outlined"
                  density="comfortable"
                  prepend-inner-icon="mdi-speedometer"
                  suffix="km"
                  required
                />
              </v-col>
            </v-row>
          </div>

          <v-divider />

          <!-- What & How Much Section -->
          <div class="section-container">
            <v-row class="mt-2">
              <v-col cols="12">
                <v-text-field
                  v-model="form.type"
                  label="Entry Type"
                  variant="outlined"
                  density="comfortable"
                  prepend-inner-icon="mdi-tag"
                  placeholder="e.g., Refuel, Charge, Top-up"
                  hint="Describe what kind of energy entry this is"
                  persistent-hint
                />
              </v-col>
              <v-col cols="12" md="8">
                <v-text-field
                  v-model.number="form.volume"
                  label="Amount"
                  type="number"
                  step="0.01"
                  min="0"
                  variant="outlined"
                  density="comfortable"
                  prepend-inner-icon="mdi-fuel"
                  required
                />
              </v-col>
              <v-col cols="12" md="4">
                <v-select
                  v-model="form.energyUnit"
                  label="Unit"
                  :items="energyUnitOptions"
                  variant="outlined"
                  density="comfortable"
                  required
                />
              </v-col>
            </v-row>
          </div>

          <v-divider class="my-4" />

          <!-- Prices Section -->
          <div class="section-container">
            <div class="section-header">
              <v-icon color="primary" class="mr-2">mdi-currency-usd</v-icon>
              <span class="section-title">Prices</span>
              <span class="section-subtitle ml-2">(optional)</span>
            </div>
            <v-row class="mt-2">
              <v-col cols="12" md="6">
                <v-text-field
                  v-model.number="form.cost"
                  label="Total Cost"
                  type="number"
                  step="0.01"
                  min="0"
                  variant="outlined"
                  density="comfortable"
                  prepend-inner-icon="mdi-cash"
                  suffix="PLN"
                  hint="Total amount paid"
                  persistent-hint
                />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  :model-value="calculatePricePerUnit"
                  label="Price per Unit"
                  type="number"
                  variant="outlined"
                  density="comfortable"
                  prepend-inner-icon="mdi-calculator"
                  :suffix="`PLN/${form.energyUnit}`"
                  readonly
                  hint="Automatically calculated"
                  persistent-hint
                />
              </v-col>
            </v-row>
          </div>
        </v-container>
      </template>

      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" class="text-none" @click="onCancel" :disabled="isLoading"> Cancel </v-btn>
        <v-btn variant="text" class="text-none" color="primary" @click="handleSave" :loading="isLoading">
          {{ isEditMode ? 'Update' : 'Save' }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.dialog-container {
  min-width: 280px;
  max-width: 600px;
}

.dialog-card {
  background-color: rgb(var(--v-theme-surface-container-high)) !important;
}

.dialog-card :deep(.v-card-title) {
  color: rgb(var(--v-theme-on-surface)) !important;
  font-size: 1.25rem;
  font-weight: 500;
}

.dialog-card :deep(.v-card-text) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  padding: 24px !important;
}

.dialog-card :deep(.v-card-actions) {
  padding: 0 24px 24px 24px !important;
}

.section-container {
  margin-bottom: 8px;
}

.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 4px;
}

.section-title {
  font-size: 1rem;
  font-weight: 500;
  color: rgb(var(--v-theme-on-surface));
}

.section-subtitle {
  font-size: 0.875rem;
  color: rgb(var(--v-theme-on-surface-variant));
  font-style: italic;
}

/* Better spacing for form fields */
:deep(.v-field) {
  margin-bottom: 4px;
}

:deep(.v-divider) {
  opacity: 0.6;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .dialog-container {
    margin: 16px;
    max-width: calc(100vw - 32px);
  }

  .dialog-card :deep(.v-card-text) {
    padding: 16px !important;
  }
}
</style>

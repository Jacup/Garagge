<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { EnergyEntryDto, CreateEnergyEntryRequest, UpdateEnergyEntryRequest } from '@/api/generated/apiV1.schemas'
import { EnergyType, EnergyUnit } from '@/api/generated/apiV1.schemas'

import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'

const energyTypeOptions = Object.values(EnergyType)
const energyUnitOptions = Object.values(EnergyUnit)

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
const formRef = ref()
const isFormValid = ref(true)
const form = ref({
  date: '',
  mileage: 0,
  type: 'Gasoline' as EnergyType,
  volume: 0,
  energyUnit: 'Liter' as EnergyUnit,
  cost: 0,
})

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
          type: '' as EnergyType,
          volume: 0,
          energyUnit: '' as EnergyUnit,
          cost: 0,
        }
      }
    }
  },
  { immediate: true },
)

async function handleSave() {
  if (!props.vehicleId) return
  // Validate form before submit
  const valid = await formRef.value?.validate()
  isFormValid.value = valid?.valid ?? true
  if (!isFormValid.value) return

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
        <v-form ref="formRef" v-model="isFormValid" lazy-validation @submit.prevent="handleSave">
          <v-container class="form-container pa-4">
            <v-row class="mb-2" align="center" justify="center">
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="form.date"
                  label="Date"
                  type="date"
                  variant="outlined"
                  density="comfortable"
                  required
                  class="form-field"
                />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model.number="form.mileage"
                  label="Mileage (km)"
                  type="number"
                  variant="outlined"
                  density="comfortable"
                  suffix="km"
                  required
                  class="form-field"
                />
              </v-col>
            </v-row>
            <v-row class="mb-2" align="center" justify="center">
              <v-col cols="12" md="6">
                <v-select
                  v-model="form.type"
                  label="Fuel entry type"
                  :items="energyTypeOptions"
                  variant="outlined"
                  density="comfortable"
                  required
                  class="form-field"
                />
              </v-col>
              <v-col cols="12" md="3">
                <v-text-field
                  v-model.number="form.volume"
                  label="Amount"
                  type="number"
                  step="0.1"
                  min="0"
                  variant="outlined"
                  density="comfortable"
                  required
                  class="form-field"
                />
              </v-col>
              <v-col cols="12" md="3">
                <v-select
                  v-model="form.energyUnit"
                  label="Unit"
                  :items="energyUnitOptions"
                  variant="outlined"
                  density="comfortable"
                  required
                  class="form-field"
                />
              </v-col>
            </v-row>
            <v-divider class="my-4" />
            <v-row class="mb-2" align="center" justify="center">
              <v-col cols="12" md="6">
                <v-text-field
                  v-model.number="form.cost"
                  label="Total Cost"
                  type="number"
                  step="0.01"
                  min="0"
                  variant="outlined"
                  density="comfortable"
                  suffix="PLN"
                  class="form-field"
                />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  :model-value="calculatePricePerUnit"
                  label="Price per Unit"
                  type="number"
                  variant="outlined"
                  density="comfortable"
                  :suffix="`PLN/${form.energyUnit}`"
                  readonly
                  hint="Automatically calculated"
                  persistent-hint
                  class="form-field"
                />
              </v-col>
            </v-row>
          </v-container>
        </v-form>
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
.form-container {
  padding: 16px 8px;
}
.form-field {
  margin-bottom: 12px;
}
.dialog-container {
  min-width: 320px;
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
.v-divider {
  opacity: 0.6;
}
@media (max-width: 768px) {
  .dialog-container {
    margin: 16px;
    max-width: calc(100vw - 32px);
  }
  .form-container {
    padding: 8px 2px;
  }
  .dialog-card :deep(.v-card-text) {
    padding: 12px !important;
  }
}
</style>

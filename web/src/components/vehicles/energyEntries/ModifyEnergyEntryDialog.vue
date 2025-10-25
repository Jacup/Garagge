<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { EnergyEntryDto, EnergyEntryCreateRequest, EnergyEntryUpdateRequest } from '@/api/generated/apiV1.schemas'
import { EnergyType, EnergyUnit } from '@/api/generated/apiV1.schemas'

import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'

// API Error Response interfaces
interface ApiError {
  code: string
  description: string
  type: string
}

interface ApiErrorResponse {
  type: string
  title: string
  status: number
  detail: string
  errors?: ApiError[]
  traceId?: string
}

// Energy type labels mapping
const ENERGY_TYPE_LABELS: Record<EnergyType, string> = {
  [EnergyType.Gasoline]: 'Gasoline',
  [EnergyType.Diesel]: 'Diesel',
  [EnergyType.Electric]: 'Electric',
  [EnergyType.LPG]: 'LPG',
  [EnergyType.CNG]: 'CNG',
  [EnergyType.Ethanol]: 'Ethanol',
  [EnergyType.Biofuel]: 'Biofuel',
  [EnergyType.Hydrogen]: 'Hydrogen',
}

// Mapa domyślnych jednostek dla typów energii
const defaultUnitMap: Record<string, EnergyUnit> = {
  [EnergyType.Gasoline]: EnergyUnit.Liter,
  [EnergyType.Diesel]: EnergyUnit.Liter,
  [EnergyType.LPG]: EnergyUnit.Liter,
  [EnergyType.CNG]: EnergyUnit.CubicMeter,
  [EnergyType.Ethanol]: EnergyUnit.Liter,
  [EnergyType.Biofuel]: EnergyUnit.Liter,
  [EnergyType.Hydrogen]: EnergyUnit.CubicMeter,
  [EnergyType.Electric]: EnergyUnit.kWh,
}

const energyUnitOptions = Object.values(EnergyUnit)

interface Props {
  isOpen: boolean
  vehicleId: string
  entry?: EnergyEntryDto | null
  allowedEnergyTypes?: string[] // Energy types allowed for this vehicle
  onSave: () => void
  onCancel: () => void
}

const props = defineProps<Props>()

// Create filtered energy type options based on vehicle's allowed types
const availableEnergyTypeOptions = computed(() => {
  if (!props.allowedEnergyTypes || props.allowedEnergyTypes.length === 0) {
    // Fallback to all energy types if no allowed types specified
    return Object.values(EnergyType).map(type => ({
      title: ENERGY_TYPE_LABELS[type],
      value: type
    }))
  }

  // Filter to only show allowed energy types
  return props.allowedEnergyTypes
    .filter(type => Object.values(EnergyType).includes(type as EnergyType))
    .map(type => ({
      title: ENERGY_TYPE_LABELS[type as EnergyType],
      value: type as EnergyType
    }))
})

const { postApiVehiclesVehicleIdEnergyEntries, putApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()

const isLoading = ref(false)
const formRef = ref()
const isFormValid = ref(true)

// Error handling
const apiErrors = ref<ApiError[]>([])
const generalError = ref<string | null>(null)

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
        // Add mode - reset form with first available energy type
        const firstAvailableType = availableEnergyTypeOptions.value[0]?.value || ('' as EnergyType)
        const defaultUnit = firstAvailableType ? defaultUnitMap[firstAvailableType] || ('' as EnergyUnit) : ('' as EnergyUnit)

        form.value = {
          date: new Date().toISOString().split('T')[0], // Today's date
          mileage: 0,
          type: firstAvailableType,
          volume: 0,
          energyUnit: defaultUnit,
          cost: 0,
        }
      }
      // Clear any previous errors when opening dialog
      clearErrors()
    }
  },
  { immediate: true },
)

// Automatyczna zmiana jednostki po zmianie typu energii
watch(() => form.value.type, (newType) => {
  if (!newType) return
  const defaultUnit = defaultUnitMap[newType]
  if (defaultUnit) {
    form.value.energyUnit = defaultUnit
  }
})

// Watch for changes in allowed energy types - reset form type if no longer allowed
watch(() => props.allowedEnergyTypes, (newAllowedTypes) => {
  if (!newAllowedTypes || !form.value.type) return

  // Check if current selected type is still allowed
  if (!newAllowedTypes.includes(form.value.type)) {
    // Reset to first available type or empty if none available
    const firstAvailable = availableEnergyTypeOptions.value[0]?.value
    form.value.type = firstAvailable || ('' as EnergyType)
  }
}, { deep: true })

const clearErrors = () => {
  apiErrors.value = []
  generalError.value = null
}

const setApiError = (errorResponse: ApiErrorResponse): void => {
  apiErrors.value = errorResponse.errors || []
  generalError.value = 'An unexpected error occurred'
}

async function handleSave() {
  if (!props.vehicleId) return
  // Validate form before submit
  const valid = await formRef.value?.validate()
  isFormValid.value = valid?.valid ?? true
  if (!isFormValid.value) return

  isLoading.value = true
  clearErrors()

  try {
    if (isEditMode.value && props.entry?.id) {
      // Update existing entry
      const updateCommand: EnergyEntryUpdateRequest = {
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
      const createCommand: EnergyEntryCreateRequest = {
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
  } catch (error: unknown) {
    console.error('Failed to save energy entry:', error)
    if (error && typeof error === 'object' && 'response' in error) {
      const axiosError = error as { response?: { data?: ApiErrorResponse } }
      if (axiosError.response?.data) {
        setApiError(axiosError.response.data)
      } else {
        generalError.value = 'Failed to save energy entry. Please try again.'
      }
    } else {
      generalError.value = 'Failed to save energy entry. Please try again.'
    }
  } finally {
    isLoading.value = false
  }
}

defineExpose({
  setApiError,
  clearErrors
})
</script>

<template>
  <v-dialog :model-value="isOpen" class="dialog-container" @update:model-value="props.onCancel">
    <v-card variant="flat" class="dialog-card" rounded="xl" elevation="6">
      <template v-slot:title>
        {{ dialogTitle }}
      </template>

      <template v-slot:text>
        <!-- Error alerts -->
        <div v-if="generalError || apiErrors.length > 0" class="mb-4">
          <v-alert v-if="generalError" type="error" class="mb-2">
            {{ generalError }}
          </v-alert>
          <v-alert v-for="error in apiErrors" :key="error.code" type="error" class="mb-2">
            {{ error.description }}
          </v-alert>
        </div>

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
                  :items="availableEnergyTypeOptions"
                  item-title="title"
                  item-value="value"
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
                  :items="form.type === 'Electric' ? ['kWh'] : energyUnitOptions.filter(u => u !== 'kWh')"
                  :disabled="form.type === 'Electric'"
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

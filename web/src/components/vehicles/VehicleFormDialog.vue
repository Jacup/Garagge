<script setup lang="ts">
import { ref, reactive, watch, nextTick } from 'vue'
import type {
  CreateVehicleCommand,
  UpdateVehicleRequest,
  VehicleDto,
  EngineType,
  EnergyType,
  NullableOfVehicleType,
} from '@/api/generated/apiV1.schemas'
import {
  EngineType as EngineTypeEnum,
  EnergyType as EnergyTypeEnum,
  NullableOfVehicleType as VehicleTypeEnum,
} from '@/api/generated/apiV1.schemas'

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

interface Props {
  isOpen: boolean
  vehicle?: VehicleDto | null
  loading?: boolean
}

interface Emits {
  (e: 'update:isOpen', value: boolean): void
  (e: 'save', vehicle: CreateVehicleCommand | UpdateVehicleRequest): void
  (e: 'error', error: ApiErrorResponse): void
}

const props = withDefaults(defineProps<Props>(), {
  vehicle: null,
  loading: false,
})

const emit = defineEmits<Emits>()

const form = ref()
const isValid = ref(false)
const isEditMode = ref(false)

// Error handling
const apiErrors = ref<ApiError[]>([])
const generalError = ref<string | null>(null)

const formData = reactive<CreateVehicleCommand & UpdateVehicleRequest>({
  brand: '',
  model: '',
  engineType: EngineTypeEnum.Fuel,
  manufacturedYear: null,
  type: undefined,
  vin: null,
  energyTypes: [],
})

// Form validation rules
const rules = {
  required: (value: string | number | null) => !!value || 'This field is required.',
  counter: (value: string) => value.length <= 64 || 'Max 64 characters',
  yearMin: (value: number | null) => !value || value >= 1886 || 'Year must be >= 1886',
  yearMax: (value: number | null) => !value || value <= new Date().getFullYear() || `Year cannot be in the future`,
  vinLength: (value: string | null) => !value || value.length === 17 || 'VIN must be exactly 17 characters',
}

// Options for select fields
const engineTypeOptions: { label: string; value: EngineType }[] = [
  { label: 'Fuel', value: EngineTypeEnum.Fuel },
  { label: 'Hybrid', value: EngineTypeEnum.Hybrid },
  { label: 'Plug-in Hybrid', value: EngineTypeEnum.PlugInHybrid },
  { label: 'Electric', value: EngineTypeEnum.Electric },
  { label: 'Hydrogen', value: EngineTypeEnum.Hydrogen },
]

const vehicleTypeOptions: { label: string; value: NullableOfVehicleType }[] = [
  { label: 'Bus', value: VehicleTypeEnum.Bus },
  { label: 'Car', value: VehicleTypeEnum.Car },
  { label: 'Motorbike', value: VehicleTypeEnum.Motorbike },
  { label: 'Truck', value: VehicleTypeEnum.Truck },
]

const energyTypeOptions: { label: string; value: EnergyType }[] = [
  { label: 'Gasoline', value: EnergyTypeEnum.Gasoline },
  { label: 'Diesel', value: EnergyTypeEnum.Diesel },
  { label: 'Electric', value: EnergyTypeEnum.Electric },
  { label: 'LPG', value: EnergyTypeEnum.LPG },
  { label: 'CNG', value: EnergyTypeEnum.CNG },
  { label: 'Ethanol', value: EnergyTypeEnum.Ethanol },
  { label: 'Biofuel', value: EnergyTypeEnum.Biofuel },
  { label: 'Hydrogen', value: EnergyTypeEnum.Hydrogen },
]

// Watch for vehicle prop changes to populate form
watch(
  () => props.vehicle,
  (newVehicle) => {
    if (newVehicle) {
      isEditMode.value = true
      Object.assign(formData, {
        brand: newVehicle.brand,
        model: newVehicle.model,
        engineType: newVehicle.engineType,
        manufacturedYear: newVehicle.manufacturedYear,
        type: newVehicle.type,
        vin: newVehicle.vin,
        energyTypes: [...(newVehicle.allowedEnergyTypes || [])],
      })
    } else {
      isEditMode.value = false
      resetForm()
    }
  },
  { immediate: true },
)

// Watch for dialog open/close
watch(
  () => props.isOpen,
  async (isOpen) => {
    if (isOpen) {
      await nextTick()
      form.value?.resetValidation()
      clearErrors()
    }
  },
)

function clearErrors() {
  apiErrors.value = []
  generalError.value = null
}

function resetForm() {
  Object.assign(formData, {
    brand: '',
    model: '',
    engineType: EngineTypeEnum.Fuel,
    manufacturedYear: null,
    type: undefined,
    vin: null,
    energyTypes: [],
  })
  form.value?.resetValidation()
  clearErrors()
}

function closeDialog() {
  emit('update:isOpen', false)
}

async function handleSave() {
  const { valid } = await form.value.validate()
  if (valid) {
    clearErrors()
    emit('save', { ...formData })
  }
}

function handleCancel() {
  resetForm()
  closeDialog()
}

// Function to be called from parent when API error occurs
function setApiError(errorResponse: ApiErrorResponse) {
  if (errorResponse.errors && errorResponse.errors.length > 0) {
    apiErrors.value = errorResponse.errors
  } else {
    generalError.value = errorResponse.detail || errorResponse.title || 'An error occurred'
  }
}

// Expose the setApiError function to parent
defineExpose({
  setApiError
})
</script>

<template>
  <v-dialog :model-value="isOpen" @update:model-value="emit('update:isOpen', $event)" max-width="600px">
    <v-card variant="flat" class="dialog-card" rounded="xl" elevation="6">
      <template v-slot:title>
        {{ isEditMode ? 'Edit Vehicle' : 'Add New Vehicle' }}
      </template>

      <template v-slot:text>
        <v-form ref="form" v-model="isValid" @submit.prevent="handleSave">
          <v-row>
            <!-- Brand -->
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.brand"
                label="Brand"
                :rules="[rules.required, rules.counter]"
                maxlength="64"
                counter
                variant="outlined"
                density="comfortable"
                :disabled="loading"
              />
            </v-col>

            <!-- Model -->
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.model"
                label="Model"
                :rules="[rules.required, rules.counter]"
                maxlength="64"
                counter
                variant="outlined"
                density="comfortable"
                :disabled="loading"
              />
            </v-col>

            <!-- Engine Type -->
            <v-col cols="12" sm="6">
              <v-select
                v-model="formData.engineType"
                label="Engine Type"
                :items="engineTypeOptions"
                item-title="label"
                item-value="value"
                :rules="[rules.required]"
                variant="outlined"
                density="comfortable"
                :disabled="loading"
              />
            </v-col>

            <!-- Vehicle Type -->
            <v-col cols="12" sm="6">
              <v-select
                v-model="formData.type"
                label="Vehicle Type"
                :items="vehicleTypeOptions"
                item-title="label"
                item-value="value"
                variant="outlined"
                density="comfortable"
                clearable
                :disabled="loading"
              />
            </v-col>

            <!-- Manufactured Year -->
            <v-col cols="12" sm="6">
              <v-number-input
                v-model="formData.manufacturedYear"
                label="Manufactured Year"
                :rules="[rules.yearMin, rules.yearMax]"
                variant="outlined"
                density="comfortable"
                :disabled="loading"
              />
            </v-col>

            <!-- VIN -->
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.vin"
                label="VIN (Vehicle Identification Number)"
                :rules="[rules.vinLength]"
                maxlength="17"
                counter
                variant="outlined"
                density="comfortable"
                :disabled="loading"
              />
            </v-col>

            <!-- Energy Types -->
            <v-col cols="12">
              <v-select
                v-model="formData.energyTypes"
                label="Supported Energy Types"
                :items="energyTypeOptions"
                item-title="label"
                item-value="value"
                variant="outlined"
                density="comfortable"
                multiple
                chips
                clearable
                :disabled="loading"
                hint="Select the energy types this vehicle can use (gasoline, diesel, electric, etc.)"
                persistent-hint
              />
            </v-col>
          </v-row>
        </v-form>

        <!-- Error Display -->
        <div v-if="generalError || apiErrors.length > 0" class="mt-4">
          <!-- General Error -->
          <v-alert
            v-if="generalError"
            type="error"
            variant="tonal"
            density="compact"
            class="mb-3"
          >
            {{ generalError }}
          </v-alert>

          <!-- API Validation Errors -->
          <v-alert
            v-if="apiErrors.length > 0"
            type="error"
            variant="tonal"
            density="compact"
            class="mb-3"
          >
            <div class="font-weight-medium mb-2">Validation Errors:</div>
            <ul class="ml-4">
              <li v-for="error in apiErrors" :key="error.code" class="mb-1">
                {{ error.description }}
              </li>
            </ul>
          </v-alert>
        </div>
      </template>

      <v-card-actions class="pa-6 pt-2">
        <v-spacer />
        <v-btn variant="text" @click="handleCancel" :disabled="loading"> Cancel </v-btn>
        <v-btn color="primary" variant="flat" @click="handleSave" :loading="loading" :disabled="!isValid">
          {{ isEditMode ? 'Update' : 'Save' }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped lang="scss">
.dialog-card {
  overflow: hidden;
}
</style>

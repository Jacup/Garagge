<script setup lang="ts">
import { ref, reactive, watch, nextTick } from 'vue'
import type {
  VehicleCreateRequest,
  VehicleUpdateRequest,
  VehicleDto,
  EngineType,
  EnergyType,
  NullableOfVehicleType,
} from '@/api/generated/apiV1.schemas'
import {
  EngineType as EngineTypeEnum,
  EnergyType as EnergyTypeEnum,
} from '@/api/generated/apiV1.schemas'
import { getVehicleEnergyTypes } from '@/api/generated/vehicle-energy-types/vehicle-energy-types'

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
  (e: 'save', vehicle: VehicleCreateRequest | VehicleUpdateRequest): void
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

const apiErrors = ref<ApiError[]>([])
const generalError = ref<string | null>(null)

const energyTypesLoading = ref(false)
const availableEnergyTypes = ref<{ label: string; value: EnergyType }[]>([])
const isLoadingVehicle = ref(false)

interface VehicleFormData {
  brand: string
  model: string
  engineType: EngineType | null
  manufacturedYear: number | null
  type: NullableOfVehicleType | null
  vin: string | null
  energyTypes: EnergyType[]
}

const formData = reactive<VehicleFormData>({
  brand: '',
  model: '',
  engineType: null,
  manufacturedYear: null,
  type: null,
  vin: null,
  energyTypes: [],
})

const MAX_FIELD_LENGTH = 64
const VIN_LENGTH = 17
const MIN_YEAR = 1886
const CURRENT_YEAR = new Date().getFullYear()

const rules = {
  required: (value: string | number | null) => !!value || 'This field is required.',
  counter: (value: string) => value.length <= MAX_FIELD_LENGTH || `Max ${MAX_FIELD_LENGTH} characters`,
  yearMin: (value: number | null) => !value || value >= MIN_YEAR || `Year must be >= ${MIN_YEAR}`,
  yearMax: (value: number | null) => !value || value <= CURRENT_YEAR || `Year cannot be in the future`,
  vinLength: (value: string | null) => !value || value.length === VIN_LENGTH || `VIN must be exactly ${VIN_LENGTH} characters`,
}

const ENERGY_TYPE_LABELS: Record<EnergyType, string> = {
  [EnergyTypeEnum.Gasoline]: 'Gasoline',
  [EnergyTypeEnum.Diesel]: 'Diesel',
  [EnergyTypeEnum.Electric]: 'Electric',
  [EnergyTypeEnum.LPG]: 'LPG',
  [EnergyTypeEnum.CNG]: 'CNG',
  [EnergyTypeEnum.Ethanol]: 'Ethanol',
  [EnergyTypeEnum.Biofuel]: 'Biofuel',
  [EnergyTypeEnum.Hydrogen]: 'Hydrogen',
}

const ENGINE_TYPE_LABELS: Record<EngineType, string> = {
  [EngineTypeEnum.Fuel]: 'Fuel',
  [EngineTypeEnum.Hybrid]: 'Hybrid',
  [EngineTypeEnum.PlugInHybrid]: 'Plug-in Hybrid',
  [EngineTypeEnum.Electric]: 'Electric',
  [EngineTypeEnum.Hydrogen]: 'Hydrogen',
}

const VEHICLE_TYPE_LABELS: Record<string, string> = {
  'Bus': 'Bus',
  'Car': 'Car',
  'Motorbike': 'Motorbike',
  'Truck': 'Truck',
}

const createEnergyTypeOptions = (energyTypes: EnergyType[]) => energyTypes.map((type) => ({ label: ENERGY_TYPE_LABELS[type], value: type }))

const engineTypeOptions: { label: string; value: EngineType }[] = Object.entries(ENGINE_TYPE_LABELS).map(([value, label]) => ({
  label,
  value: value as EngineType,
}))

const vehicleTypeOptions: { label: string; value: NullableOfVehicleType }[] = Object.entries(VEHICLE_TYPE_LABELS)
  .map(([value, label]) => ({ label, value: value as NullableOfVehicleType }))

async function fetchSupportedEnergyTypes(engineType: EngineType) {
  try {
    energyTypesLoading.value = true
    const api = getVehicleEnergyTypes()
    const result = await api.getApiEnergyTypesSupported({ engineType })

    availableEnergyTypes.value = createEnergyTypeOptions(result)
  } catch (error) {
    console.error('Failed to fetch supported energy types:', error)
    availableEnergyTypes.value = []
    generalError.value = 'Failed to load supported energy types for the selected engine type. Please try again.'
  } finally {
    energyTypesLoading.value = false
  }
}

function populateFormWithVehicle(vehicle: VehicleDto) {
  Object.assign(formData, {
    brand: vehicle.brand,
    model: vehicle.model,
    engineType: vehicle.engineType,
    manufacturedYear: vehicle.manufacturedYear,
    type: vehicle.type,
    vin: vehicle.vin,
    energyTypes: [...(vehicle.allowedEnergyTypes || [])],
  })
}

watch(
  () => props.vehicle,
  async (newVehicle) => {
    if (newVehicle) {
      isLoadingVehicle.value = true
      isEditMode.value = true
      populateFormWithVehicle(newVehicle)

      if (newVehicle.engineType) {
        await fetchSupportedEnergyTypes(newVehicle.engineType)
      }
      isLoadingVehicle.value = false
    } else {
      isEditMode.value = false
      resetForm()
    }
  },
  { immediate: true },
)

watch(
  () => formData.engineType,
  async (newEngineType, oldEngineType) => {
    if (oldEngineType === undefined || isLoadingVehicle.value) return

    if (newEngineType && newEngineType !== oldEngineType) {
      formData.energyTypes = []
      await fetchSupportedEnergyTypes(newEngineType)
    } else if (!newEngineType) {
      formData.energyTypes = []
      availableEnergyTypes.value = []
    }
  },
)

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
    engineType: null,
    manufacturedYear: null,
    type: undefined,
    vin: null,
    energyTypes: [],
  })
  // Reset available energy types
  availableEnergyTypes.value = []
  form.value?.resetValidation()
  clearErrors()
}

function closeDialog() {
  emit('update:isOpen', false)
}

function transformFormDataToApiFormat(formData: VehicleFormData) {
  if (!formData.engineType) {
    throw new Error('Engine type is required')
  }

  return {
    brand: formData.brand,
    model: formData.model,
    engineType: formData.engineType,
    manufacturedYear: formData.manufacturedYear,
    type: formData.type ?? null,
    vin: formData.vin,
    energyTypes: formData.energyTypes.length > 0 ? formData.energyTypes : null,
  }
}

async function handleSave() {
  const { valid } = await form.value.validate()
  if (valid && formData.engineType) {
    clearErrors()
    try {
      const vehicleData = transformFormDataToApiFormat(formData)
      emit('save', vehicleData)
    } catch (error) {
      generalError.value = error instanceof Error ? error.message : 'Invalid form data'
    }
  }
}

function handleCancel() {
  resetForm()
  closeDialog()
}

function setApiError(errorResponse: ApiErrorResponse) {
  if (errorResponse.errors && errorResponse.errors.length > 0) {
    apiErrors.value = errorResponse.errors
  } else {
    generalError.value = errorResponse.detail || errorResponse.title || 'An error occurred'
  }
}

defineExpose({
  setApiError,
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
                :items="availableEnergyTypes"
                item-title="label"
                item-value="value"
                variant="outlined"
                density="comfortable"
                multiple
                chips
                clearable
                :disabled="loading || energyTypesLoading || !formData.engineType"
                :loading="energyTypesLoading"
                hint="Select the energy types this vehicle can use (gasoline, diesel, electric, etc.)"
                persistent-hint
              />
            </v-col>
          </v-row>
        </v-form>

        <!-- Error Display -->
        <div v-if="generalError || apiErrors.length > 0" class="mt-4">
          <!-- General Error -->
          <v-alert v-if="generalError" type="error" variant="tonal" density="compact" class="mb-3">
            {{ generalError }}
          </v-alert>

          <!-- API Validation Errors -->
          <v-alert v-if="apiErrors.length > 0" type="error" variant="tonal" density="compact" class="mb-3">
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

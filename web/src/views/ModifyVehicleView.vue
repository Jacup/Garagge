<script lang="ts" setup>
import { ref, reactive, inject, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import VehicleInformationCard from '@/components/vehicles/VehicleInformationCard.vue'
import FileUploadCard from '@/components/vehicles/FileUploadCard.vue'

import { getVehicles } from '@/api/generated/vehicles/vehicles'
import {
  type VehicleCreateRequest,
  type VehicleUpdateRequest,
  type VehicleDto,
  EngineType,
} from '@/api/generated/apiV1.schemas'

const route = useRoute()
const router = useRouter()
const { getApiVehiclesId, postApiVehicles, putApiVehiclesId } = getVehicles()

const vehicleId = route.params.id as string
const isEditMode = !!vehicleId

const createVehicle = reactive<VehicleCreateRequest>({
  brand: '',
  model: '',
  engineType: EngineType.Fuel, // Default to first option
  manufacturedYear: null,
  type: null,
  vin: null,
  energyTypes: null,
})

const editVehicle = reactive<VehicleUpdateRequest>({
  brand: '',
  model: '',
  engineType: EngineType.Fuel,
  manufacturedYear: null,
  type: null,
  vin: null,
  energyTypes: null,
})

// Use the appropriate vehicle object based on mode
const vehicle = isEditMode ? editVehicle : createVehicle

const uploadedFiles = ref<File[]>([])
const isLoading = ref(isEditMode)

onMounted(async () => {
  if (isEditMode) {
    try {
      const res = await getApiVehiclesId(vehicleId)
      const vehicleData = res.data as VehicleDto

      Object.assign(editVehicle, {
        brand: vehicleData.brand || '',
        model: vehicleData.model || '',
        engineType: vehicleData.engineType,
        manufacturedYear: vehicleData.manufacturedYear,
        type: vehicleData.type || null,
        vin: vehicleData.vin || null,
      })
    } catch (error) {
      console.error('Failed to load vehicle data:', error)
      router.push('/vehicles')
    } finally {
      isLoading.value = false
    }
  }
})

async function save() {
  try {
    if (isEditMode) {
      console.log('Update vehicle data:', editVehicle)
      console.log('Vehicle ID:', vehicleId)
      await putApiVehiclesId(vehicleId, editVehicle)
      router.push('/vehicles')
    } else {
      console.log('Vehicle data:', createVehicle)
      console.log('Uploaded files:', uploadedFiles.value)
      await postApiVehicles(createVehicle)
      router.push('/vehicles')
    }
  } catch (error) {
    console.error('Failed to save vehicle:', error)
  }
}

// Inject header actions
interface HeaderAction {
  label: string
  action: () => void
  color?: string
  variant?: 'flat' | 'text' | 'elevated' | 'tonal' | 'outlined' | 'plain'
}

const headerActions = inject('headerActions') as { value: HeaderAction[] } | undefined

onMounted(() => {
  if (headerActions) {
    const actionLabel = isEditMode ? 'Update Vehicle' : 'Save Vehicle'
    headerActions.value = [{ label: actionLabel, action: save, color: 'primary' }]
  }
})

onUnmounted(() => {
  if (headerActions) {
    headerActions.value = []
  }
})
</script>

<template>
  <div class="form-container" v-if="!isLoading">
    <VehicleInformationCard :vehicle="vehicle" @update:vehicle="Object.assign(vehicle, $event)" @save="save" />
    <FileUploadCard :files="uploadedFiles" @update:files="uploadedFiles = $event" />
    <v-btn @click="save" color="primary" class="mt-4">Zapisz zmiany</v-btn>
  </div>
  <div v-else class="loading-container">
    <v-progress-circular indeterminate color="primary"></v-progress-circular>
    <p>Loading vehicle data...</p>
  </div>
</template>

<style scoped>
.form-container {
  display: flex;
  flex-direction: row;
  justify-content: center;
  gap: 2rem;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 50vh;
  gap: 1rem;
}
</style>

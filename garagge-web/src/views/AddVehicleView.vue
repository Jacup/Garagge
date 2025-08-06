<script lang="ts" setup>
import { ref, reactive, inject, onMounted, onUnmounted } from 'vue'
import VehicleInformationCard from '@/components/vehicles/VehicleInformationCard.vue'
import FileUploadCard from '@/components/vehicles/FileUploadCard.vue'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import type { CreateMyVehicleCommand, PowerType, NullableOfVehicleType2 } from '@/api/generated/apiV1.schemas'

const vehicle = reactive<CreateMyVehicleCommand>({
  brand: '',
  model: '',
  powerType: 'Gasoline' as PowerType,
  manufacturedYear: new Date().getFullYear(),
  type: null as NullableOfVehicleType2,
  vin: null,
})

const uploadedFiles = ref<File[]>([])

async function save() {
  console.log('Vehicle data:', vehicle)
  console.log('Uploaded files:', uploadedFiles.value)
  const vehiclesApi = getVehicles()
  await vehiclesApi.postVehiclesMy(vehicle)
}

// Inject header actions
interface HeaderAction {
  label: string
  action: () => void
  color?: string
  variant?: 'flat' | 'text' | 'elevated' | 'tonal' | 'outlined' | 'plain'
}

const headerActions = inject('headerActions') as { value: HeaderAction[] }

onMounted(() => {
  headerActions.value = [{ label: 'Save Vehicle', action: save, color: 'primary' }]
})

onUnmounted(() => {
  headerActions.value = []
})
</script>

<template>
  <div class="form-container">
    <VehicleInformationCard :vehicle="vehicle" @update:vehicle="Object.assign(vehicle, $event)" @save="save" />
    <FileUploadCard :files="uploadedFiles" @update:files="uploadedFiles = $event" />
  </div>
</template>

<style scoped>
.form-container {
  display: flex;
  flex-direction: row;
  justify-content: center;
  gap: 2rem;
}
</style>

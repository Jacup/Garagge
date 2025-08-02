<script lang="ts" setup>
import { ref, reactive, inject, onMounted, onUnmounted } from 'vue'
import VehicleInformationCard from '@/components/vehicles/VehicleInformationCard.vue'
import FileUploadCard from '@/components/vehicles/FileUploadCard.vue'
import { addNewVehicle } from '@/api/vehiclesApi'

import type { Vehicle } from '@/types/vehicle'

const vehicle = reactive<Vehicle>({
  brand: '',
  model: '',
  manufacturedYear: new Date().getFullYear(),
})

const uploadedFiles = ref<File[]>([])

async function save() {
  console.log('Vehicle data:', vehicle)
  console.log('Uploaded files:', uploadedFiles.value)
  await addNewVehicle(vehicle)
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

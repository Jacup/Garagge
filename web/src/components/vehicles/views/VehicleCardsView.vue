<script lang="ts" setup>
import { ref } from 'vue'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

interface Props {
  items: VehicleDto[]
  loading?: boolean
}

interface Emits {
  (e: 'edit', id: string): void
  (e: 'delete', id: string): void
  (e: 'view', id: string): void
}

defineProps<Props>()
const emit = defineEmits<Emits>()

const deleteDialog = ref(false)
const selectedVehicle = ref<VehicleDto | null>(null)

function openDeleteDialog(vehicle: VehicleDto) {
  selectedVehicle.value = vehicle
  deleteDialog.value = true
}

function closeDeleteDialog() {
  deleteDialog.value = false
  selectedVehicle.value = null
}

async function confirmDelete() {
  if (selectedVehicle.value?.id) {
    emit('delete', selectedVehicle.value.id)
    closeDeleteDialog()
  }
}
</script>

<template>
  <v-row class="ma-0" :class="{ 'mx-n1': true }">
    <template v-if="loading">
      <v-col v-for="n in 6" :key="n" cols="12" sm="6" md="4" lg="3">
        <v-card>
          <v-card-title>
            <v-skeleton-loader type="text" />
          </v-card-title>
          <v-card-text>
            <v-skeleton-loader type="paragraph" />
          </v-card-text>
          <v-card-actions>
            <v-skeleton-loader type="button" />
            <v-skeleton-loader type="button" />
          </v-card-actions>
        </v-card>
      </v-col>
    </template>

    <v-col v-else v-for="vehicle in items" :key="vehicle.id" cols="12" sm="6" md="4" lg="3" class="pa-1">
      <v-card class="vehicle-card" variant="tonal" rounded="md-medium" @click="$emit('view', vehicle.id!)">
        <v-card-title> {{ vehicle.brand }} {{ vehicle.model }} </v-card-title>
        <v-card-subtitle>{{ vehicle.engineType || 'N/A' }}</v-card-subtitle>
        <v-card-actions>
          <v-spacer />

          <v-btn icon="mdi-pencil" variant="text" size="x-small" @click.stop="$emit('edit', vehicle.id!)" />
          <v-btn icon="mdi-delete" variant="text" size="x-small" color="error" @click.stop="openDeleteDialog(vehicle)" />
        </v-card-actions>
      </v-card>
    </v-col>
  </v-row>

  <DeleteDialog
    :item-to-delete="`${selectedVehicle?.brand} ${selectedVehicle?.model}`"
    :is-open="deleteDialog"
    :on-confirm="confirmDelete"
    :on-cancel="closeDeleteDialog"
  />
</template>

<style scoped>
.vehicle-card {
  cursor: pointer;
}
</style>

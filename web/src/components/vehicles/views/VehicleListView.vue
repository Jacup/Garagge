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
  <v-list
    class="transparent-list"
    >
    <template v-if="loading">
      <v-list-item v-for="n in 5" :key="n">
        <v-list-item-title>
          <v-skeleton-loader type="text" />
        </v-list-item-title>
        <v-list-item-subtitle>
          <v-skeleton-loader type="text" />
        </v-list-item-subtitle>
      </v-list-item>
    </template>

    <v-list-item
      v-else
      v-for="vehicle in items"
      :key="vehicle.id"
      @click="$emit('view', vehicle.id!)"
      class="vehicle-list-item"
      rounded="xl"
    >
      <v-list-item-title>
        {{ vehicle.brand }} {{ vehicle.model }}
      </v-list-item-title>

      <v-list-item-subtitle>
        {{ vehicle.engineType }} • {{ vehicle.manufacturedYear || 'N/A' }}
      </v-list-item-subtitle>

      <template #append>
        <div class="d-flex gap-1">
          <v-btn
            icon="mdi-pencil"
            size="small"
            variant="text"
            @click.stop="$emit('edit', vehicle.id!)"
          />
          <v-btn
            icon="mdi-delete"
            size="small"
            variant="text"
            color="error"
            @click.stop="openDeleteDialog(vehicle)"
          />
        </div>
      </template>
    </v-list-item>
  </v-list>

  <!-- Delete Confirmation Dialog -->
  <DeleteDialog
    v-if="selectedVehicle"
    :item-to-delete="`${selectedVehicle.brand} ${selectedVehicle.model}`"
    :is-open="deleteDialog"
    :on-confirm="confirmDelete"
    :on-cancel="closeDeleteDialog"
  />
</template>

<style scoped lang="scss">
.transparent-list {
  background: transparent !important;
}

.vehicle-list-item {
  cursor: pointer;

  &:hover {
    background-color: rgba(var(--v-theme-on-surface), 0.08);
  }
}
</style>

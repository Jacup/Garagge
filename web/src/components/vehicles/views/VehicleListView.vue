<script lang="ts" setup>
import type { VehicleDto } from '@/api/generated/apiV1.schemas'

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
defineEmits<Emits>()
</script>

<template>
  <v-list>
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
            @click.stop="$emit('delete', vehicle.id!)"
          />
        </div>
      </template>
    </v-list-item>
  </v-list>
</template>

<style scoped>
.vehicle-list-item {
  cursor: pointer;
}

.vehicle-list-item:hover {
  background-color: rgba(var(--v-theme-on-surface), 0.08);
}
</style>

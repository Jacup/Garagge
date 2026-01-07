<script lang="ts" setup>
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { getVehicleIcon } from '@/utils/vehicleUtils'

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
  <v-row class="ma-n2">
    <v-col v-for="vehicle in items" :key="vehicle.id" cols="12" sm="6" md="4" lg="3" class="pa-2">
      <v-card
        class="vehicle-card d-flex flex-column transition-swing"
        variant="flat"
        height="100%"
        :title="`${vehicle.brand} ${vehicle.model}`"
        :subtitle="vehicle.manufacturedYear || '-'"
        rounded="md-16px"
        @click="$emit('view', vehicle.id!)"
      >
        <template v-slot:prepend>
          <v-avatar color="primary" variant="tonal" class="mr-3">
            <v-icon :icon="getVehicleIcon(vehicle.type)" color="primary" />
          </v-avatar>
        </template>

        <v-card-text>
          <div class="d-flex flex-wrap">
            <v-chip v-if="vehicle.engineType" class="suggestion-chip" variant="flat" size="small" density="comfortable">
              <v-icon start icon="mdi-engine" size="x-small"></v-icon>
              {{ vehicle.engineType }}
            </v-chip>
          </div>

          <div v-if="vehicle.vin" class="mt-2 text-caption text-disabled font-monospace d-flex align-center">
            <v-icon icon="mdi-barcode" size="small" class="mr-1"></v-icon>
            {{ vehicle.vin }}
          </div>
        </v-card-text>

        <v-card-actions>
          <v-spacer />

          <v-tooltip text="Edit" location="bottom" open-delay="200" close-delay="500">
            <template #activator="{ props }">
              <v-btn
                v-bind="props"
                icon="mdi-pencil"
                variant="text"
                density="comfortable"
                color="secondary"
                @click.stop="$emit('edit', vehicle.id!)"
              />
            </template>
          </v-tooltip>

          <v-tooltip text="Delete" location="bottom" open-delay="200" close-delay="500">
            <template #activator="{ props }">
              <v-btn
                v-bind="props"
                icon="mdi-delete"
                variant="text"
                density="comfortable"
                color="error"
                @click.stop="$emit('delete', vehicle.id!)"
              />
            </template>
          </v-tooltip>
        </v-card-actions>
      </v-card>
    </v-col>
  </v-row>
</template>

<style scoped lang="scss">
.vehicle-card {
  cursor: pointer;
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.lh-tight {
  line-height: 1.2;
}

.gap-2 {
  gap: 8px;
}

.font-monospace {
  font-family: monospace;
}

.suggestion-chip {
  border-radius: 8px !important;
  border: 1px solid rgb(var(--v-theme-outline-variant)) !important;
  font-weight: 500 !important;
  line-height: 20px !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}
</style>

<script setup lang="ts">
import type { VehicleDto, EnergyStatsDto } from '@/api/generated/apiV1.schemas'
import VehicleDetailItem from '@/components/vehicles/VehicleDetailItem.vue'

interface Props {
  vehicle: VehicleDto
  lastEnteredMileage: number | null
  globalStats: EnergyStatsDto | null
  summaryStats: {
    totalEntries: number
    totalCost: number
    totalVolume: number
    volumeUnit: string
    consumptions: Array<{ value: number | undefined; unit: string }>
  } | null
}

defineProps<Props>()

const emit = defineEmits<{
  'edit-vehicle': []
}>()
</script>

<template>
  <!-- Enhanced Summary Cards with Material Design Colors -->
  <section class="summary-section mb-6">
    <v-row>
      <v-col cols="12" sm="6" md="3">
        <v-card class="summary-card" height="120" color="primary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon
              icon="mdi-gas-station"
              size="32"
              class="position-absolute text-primary"
              style="top: 12px; right: 16px; opacity: 0.6"
            />
            <div class="text-caption text-on-primary-container">Fuel Type</div>
            <div class="text-h6 font-weight-bold text-on-primary-container">{{ vehicle?.engineType || 'N/A' }}</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="summary-card" height="120" color="secondary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon
              icon="mdi-speedometer"
              size="32"
              class="position-absolute text-secondary"
              style="top: 12px; right: 16px; opacity: 0.6"
            />
            <div class="text-caption text-on-secondary-container">Last entered mileage</div>
            <div class="text-h6 font-weight-bold text-on-secondary-container">
              {{ lastEnteredMileage !== null ? lastEnteredMileage.toLocaleString() : 'N/A' }}
              <span v-if="lastEnteredMileage !== null" class="text-body-2">km</span>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="summary-card" height="120" color="tertiary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon
              icon="mdi-currency-usd"
              size="32"
              class="position-absolute text-tertiary"
              style="top: 12px; right: 16px; opacity: 0.6"
            />
            <div class="text-caption text-on-tertiary-container">Total fuel cost</div>
            <div class="text-h6 font-weight-bold text-on-tertiary-container">
              ${{ globalStats ? globalStats.totalCost.toFixed(2) : 'N/A' }}
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="summary-card" height="120" color="surface-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-car-info" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
            <div class="text-caption text-on-surface">Avg. Consumption</div>
            <div v-if="summaryStats && summaryStats.consumptions.length > 0" class="consumption-values">
              <div v-for="(consumption, index) in summaryStats.consumptions" :key="index" class="consumption-item">
                <span class="text-h6 font-weight-bold text-on-surface">{{ consumption.value?.toFixed(2) }}</span>
                <span class="text-body-2 text-on-surface ml-1">{{ consumption.unit }}</span>
              </div>
            </div>
            <div v-else class="text-h6 font-weight-bold text-on-surface">N/A</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </section>

  <v-row class="equal-height-row">
    <v-col cols="12" md="4">
      <v-card class="card-background" variant="flat" rounded="md-16px" height="260px">
        <template #title>{{ vehicle?.brand }} {{ vehicle?.model }}</template>
        <template #append>
          <v-btn prepend-icon="mdi-pencil" variant="flat" color="primary" @click="emit('edit-vehicle')">Edit</v-btn>
        </template>
        <template #subtitle>
          <v-chip variant="tonal" size="small" density="comfortable" rounded="lg">
            {{ vehicle?.engineType }}
          </v-chip>
        </template>
        <v-card-text>
          <div class="details-items-container">
            <VehicleDetailItem
              icon="mdi-calendar"
              label="Year"
              :value="vehicle?.manufacturedYear ? vehicle.manufacturedYear : 'N/A'"
            />
            <v-spacer />
            <VehicleDetailItem icon="mdi-car" label="Type" :value="vehicle?.type ? vehicle.type : 'N/A'" />
            <v-spacer />
          </div>
          <v-divider class="my-3" />
          <div class="details-items-container">
            <VehicleDetailItem icon="mdi-pound" label="VIN" :value="vehicle?.vin ? vehicle.vin : 'N/A'" />
          </div>
        </v-card-text>
      </v-card>
    </v-col>

    <v-col cols="12" md="8">
      <v-card class="card-background vehicle-image-card" variant="flat" rounded="md-16px" height="260px">
        <v-card-text class="image-placeholder">
          <v-icon size="64">mdi-image-off-outline</v-icon>
          <span>No image available</span>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

<style scoped>
/* Summary cards */
.summary-card {
  border-radius: 16px;
}

/* Consumption display for multiple values */
.consumption-values {
  display: flex;
  flex-direction: column;
}

.consumption-item {
  display: flex;
  align-items: baseline;
}

/* Equal height rows */
.equal-height-row {
  align-items: stretch;
}

.equal-height-row .v-col {
  display: flex;
}

.equal-height-row .v-card {
  flex: 1;
}

/* Card background */
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.details-items-container {
  margin-top: 8px;
  margin-bottom: 8px;
  display: flex;
  flex-direction: row;
  gap: 24px;
  align-items: center;
}

.vehicle-image-card {
  text-align: center;
}

.image-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  height: 100%;
  justify-content: center;
}
</style>

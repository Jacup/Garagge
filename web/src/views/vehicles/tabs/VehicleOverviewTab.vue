<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import type { VehicleDto, EnergyStatsDto } from '@/api/generated/apiV1.schemas'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import RecordInfo from '@/components/common/RecordInfo.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import StackedButton from '@/components/common/StackedButton.vue'

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

const props = defineProps<Props>()

const { deleteApiVehiclesId } = getVehicles()
const { isMobile } = useResponsiveLayout()
const router = useRouter()

const showDeleteDialog = ref(false)

async function confirmDelete() {
  if (!props.vehicle.id) return

  showDeleteDialog.value = false

  await deleteApiVehiclesId(props.vehicle.id)
  router.push({ name: 'Vehicles' })
}
</script>

<template>
  <template v-if="isMobile">
    <v-row>
      <v-card variant="flat">
        <v-card-title>
          <div class="text-h6 font-weight-bold">{{ vehicle.brand }} {{ vehicle.model }}</div>
        </v-card-title>
      </v-card>

      <!-- Actions -->
      <v-col cols="12">
        <div class="d-flex flex-wrap ga-2">
          <StackedButton icon="mdi-shield" label="Insurance" disabled/>
          <StackedButton icon="mdi-bell" label="Reminders" disabled />
        </div>
      </v-col>

      <!-- Vehicle details -->
      <v-col cols="12" class="pt-0">
        <v-list lines="two">
          <v-list-item
            v-if="vehicle.manufacturedYear"
            class="list-item"
            prepend-icon="mdi-calendar"
            :title="vehicle.manufacturedYear"
            subtitle="Year"
          />
          <v-list-item v-if="vehicle.type" class="list-item" prepend-icon="mdi-car-outline" :title="vehicle.type" subtitle="Type" />
          <v-list-item v-if="vehicle.vin" class="list-item" prepend-icon="mdi-pound" :title="vehicle.vin" subtitle="VIN" />
        </v-list>
      </v-col>

      <!-- Engine details -->
      <v-col cols="12" class="pt-0">
        <v-list lines="two">
          <v-list-item class="list-item" prepend-icon="mdi-engine-outline" :title="vehicle.engineType" subtitle="Engine Type" />
          <v-list-item
            v-if="vehicle.allowedEnergyTypes && vehicle.allowedEnergyTypes.length > 0"
            lines="one"
            class="list-item"
            :prepend-icon="vehicle.engineType === 'Electric' ? 'mdi-ev-station' : 'mdi-gas-station-outline'"
          >
            <template #title>
              <v-chip
                v-for="energyType in vehicle.allowedEnergyTypes"
                :key="energyType"
                class="suggestion-chip mr-2"
                size="small"
                variant="flat"
                >{{ energyType }}</v-chip
              >
            </template>
          </v-list-item>
        </v-list>
      </v-col>

      <!-- Settings -->
      <v-col cols="12">
        <v-list lines="one" selectable>
          <v-list-item class="list-item" prepend-icon="mdi-view-grid-plus-outline" title="Add to homepage" disabled />
          <v-list-item class="list-item" prepend-icon="mdi-swap-horizontal" title="Transfer vehicle ownership" disabled />
          <v-list-item
            class="list-item"
            prepend-icon="mdi-delete-outline"
            title="Delete vehicle"
            base-color="error"
            @click="() => (showDeleteDialog = true)"
          />
        </v-list>
      </v-col>

      <v-col cols="12">
        <RecordInfo :created-date="vehicle.createdDate!" :updated-date="vehicle.updatedDate!" :id="vehicle.id!" />
      </v-col>
    </v-row>
  </template>

  <template v-else> </template>

  <!-- Enhanced Summary Cards with Material Design Colors -->
  <!-- <section class="summary-section mb-6">
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
  </section> -->

  <v-row class="equal-height-row">
    <!-- <v-col cols="12" md="4">
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
            <VehicleDetailItem icon="mdi-calendar" label="Year" :value="vehicle?.manufacturedYear ? vehicle.manufacturedYear : 'N/A'" />
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
    </v-col> -->

    <!-- <v-col cols="12" md="8">
      <v-card class="card-background vehicle-image-card" variant="flat" rounded="md-16px" height="260px">
        <v-card-text class="image-placeholder">
          <v-icon size="64">mdi-image-off-outline</v-icon>
          <span>No image available</span>
        </v-card-text>
      </v-card>
    </v-col> -->
  </v-row>

  <DeleteDialog
    :item-to-delete="vehicle.brand + ' ' + vehicle.model"
    :is-open="showDeleteDialog"
    :on-confirm="confirmDelete"
    :on-cancel="() => (showDeleteDialog = false)"
  />
</template>

<style scoped>
/* Summary cards */
.summary-card {
  border-radius: 16px;
}
.suggestion-chip {
  border-radius: 8px !important;
  border: 1px solid rgb(var(--v-theme-outline-variant)) !important;
  font-weight: 500 !important;
  line-height: 20px !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
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

.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
  border-radius: 2px !important;
}

.list-item:first-child {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}

.list-item:last-child {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
  margin-bottom: 0 !important;
}
</style>

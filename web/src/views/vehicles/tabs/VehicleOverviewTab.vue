<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import type { VehicleDto, EnergyStatsDto } from '@/api/generated/apiV1.schemas'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import RecordInfo from '@/components/common/RecordInfo.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import StackedButton from '@/components/common/StackedButton.vue'
import StatCard from '@/components/dashboard/StatCard.vue'
import VehicleDetailItem from '@/components/vehicles/VehicleDetailItem.vue'
import { v } from 'vue-router/dist/router-CWoNjPRp.mjs'

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
  <v-row>
    <v-col cols="12" class="hero-container">
      <div class="image-container">
        <v-icon size="64">mdi-image-off-outline</v-icon>
        <span>No image available</span>
      </div>

      <div class="title-container">
        <span class="title">{{ vehicle.brand }} {{ vehicle.model }}</span>
      </div>
      <div class="hero-actions">
        <v-btn color="on-surface-variant" icon variant="text">
          <v-icon size="24">mdi-star-outline</v-icon>
        </v-btn>
        <v-btn color="primary" variant="flat">Edit</v-btn>
        <v-btn color="on-surface-variant" icon variant="text">
          <v-icon size="24">mdi-dots-vertical</v-icon>
          <v-menu activator="parent">
            <v-list>
              <v-list-item prepend-icon="mdi-view-grid-plus-outline" title="Add to homepage" disabled />
              <v-list-item prepend-icon="mdi-swap-horizontal" title="Transfer vehicle ownership" disabled />
              <v-list-item prepend-icon="mdi-account-multiple-outline" title="Manage accessibility" disabled />
              <v-list-item
                prepend-icon="mdi-delete-outline"
                title="Delete vehicle"
                base-color="error"
                @click="() => (showDeleteDialog = true)"
              />
            </v-list>
          </v-menu>
        </v-btn>
      </div>
    </v-col>

    <v-col cols="12">
      <div class="d-flex align-start ga-2">
        <StackedButton icon="mdi-shield" label="Insurance" disabled />
        <StackedButton icon="mdi-bell" label="Reminders" disabled />
        <StackedButton icon="mdi-calendar" label="Plan" disabled />
        <v-divider v-if="!isMobile" class="flex-grow-1" style="margin-top: 28px"></v-divider>
      </div>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="5" lg="4">
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

    <v-col>
      <v-row>
        <v-col cols="6">
          <StatCard title="Fuel Expenses" metric="fuelExpenses" icon="mdi-gas-station" accent-color="primary" />
        </v-col>
        <v-col cols="6">
          <StatCard title="Last Mileage" metric="fuelExpenses" icon="mdi-gas-station" accent-color="primary" />
        </v-col>
        <v-col cols="6">
          <StatCard title="Avg. Consumption" metric="fuelExpenses" icon="mdi-gas-station" accent-color="primary" />
        </v-col>
        <v-col cols="6">
          <StatCard title="Total Entries" metric="fuelExpenses" icon="mdi-gas-station" accent-color="primary" />
        </v-col>

        <v-col cols="12">
          <v-card variant="flat" color="red" height="250px">recent activity</v-card>
        </v-col>
      </v-row>
    </v-col>
  </v-row>

  <v-row>
    <v-col v-if="isMobile">
      <v-list lines="one" selectable>
        <v-list-item class="list-item" prepend-icon="mdi-view-grid-plus-outline" title="Add to homepage" disabled />
        <v-list-item class="list-item" prepend-icon="mdi-swap-horizontal" title="Transfer vehicle ownership" disabled />
        <v-list-item class="list-item" prepend-icon="mdi-account-multiple-outline" title="Manage accessibility" disabled />
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

  <DeleteDialog
    :item-to-delete="vehicle.brand + ' ' + vehicle.model"
    :is-open="showDeleteDialog"
    :on-confirm="confirmDelete"
    :on-cancel="() => (showDeleteDialog = false)"
  />
</template>

<style scoped lang="scss">
.hero-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: flex-start;
  gap: 24px;
  position: relative;

  @media (max-width: 959px) {
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    gap: 16px;
  }
}

.image-container {
  width: 350px;
  height: 200px;
  display: flex;
  flex-shrink: 0;
  justify-content: center;
  align-items: center;
  border: 2px dashed rgba(var(--v-theme-on-surface-variant), 0.38);
  border-radius: 28px;
}

.title {
  font-size: 32px;
  font-weight: 600;
  line-height: 40px;
  color: rgba(var(--v-theme-on-surface-variant));
}

.hero-actions {
  position: absolute;
  top: 0;
  right: 0;

  /* Opcjonalnie: mały odstęp, żeby nie dotykało krawędzi */
  margin-top: 12px;
  margin-right: 12px;
}

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

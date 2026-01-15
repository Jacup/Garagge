<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { VehicleDto, VehicleUpdateRequest, EnergyStatsDto } from '@/api/generated/apiV1.schemas'

import VehicleOverviewTab from '@/views/vehicles/tabs/VehicleOverviewTab.vue'
import VehicleFuelTab from '@/views/vehicles/tabs/VehicleFuelTab.vue'
import VehicleServiceTab from '@/views/vehicles/tabs/VehicleServiceTab.vue'

import VehicleFormDialog from '@/components/vehicles/VehicleFormDialog.vue'
import { useLayoutFab } from '@/composables/useLayoutFab'
import { useServiceDetailsState } from '@/composables/vehicles/useServiceDetailsState'
import { useEnergyEntriesState } from '@/composables/vehicles/useEnergyEntriesState'

const route = useRoute()

const { getApiVehiclesId, putApiVehiclesId } = getVehicles()
const { getApiVehiclesVehicleIdEnergyEntriesStats } = getEnergyEntries()
const { registerFab, registerFabMenu, unregisterFab } = useLayoutFab()
const { close: closeServiceDetailsSheet } = useServiceDetailsState()

const energyEntriesState = useEnergyEntriesState()
const detailsState = useServiceDetailsState()

const vehicleId = ref(route.params.id as string)
const vehicle = ref<VehicleDto | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const energystats = ref<EnergyStatsDto | null>(null)
const statsLoading = ref(false)
const globalStats = ref<EnergyStatsDto | null>(null)

const getUnitLabel = (unit: string | undefined): string => {
  if (!unit) return ''
  switch (unit) {
    case 'Liter':
      return 'L'
    case 'Gallon':
      return 'gal'
    case 'CubicMeter':
      return 'm³'
    case 'kWh':
      return 'kWh'
    default:
      return unit
  }
}

const summaryStats = computed(() => {
  if (!globalStats.value) return null

  const firstUnit = globalStats.value.energyUnitStats[0]

  const consumptions = globalStats.value.energyUnitStats
    .filter((stat) => stat.averageConsumption && stat.averageConsumption > 0)
    .map((stat) => ({
      value: stat.averageConsumption,
      unit: stat.unit === 'kWh' ? 'kWh/100km' : 'L/100km',
    }))

  return {
    totalEntries: globalStats.value.totalEntries,
    totalCost: globalStats.value.totalCost,
    totalVolume: firstUnit?.totalVolume ?? 0,
    volumeUnit: getUnitLabel(firstUnit?.unit),
    consumptions: consumptions,
  }
})

const activeTab = ref('overview')
const editVehicleDialog = ref(false)

const selectedServiceRecords = ref<string[]>([])

async function loadVehicle() {
  if (!vehicleId.value) {
    error.value = 'No vehicle ID provided'
    loading.value = false
    return
  }

  try {
    loading.value = true
    error.value = null
    const response = await getApiVehiclesId(vehicleId.value)
    vehicle.value = response
    await loadGlobalStats()
  } catch (err) {
    console.error('Failed to load vehicle:', err)
    error.value = 'Failed to load vehicle data'
  } finally {
    loading.value = false
  }
}

async function loadEnergyStats() {
  if (!vehicleId.value) return

  try {
    statsLoading.value = true
    const response = await getApiVehiclesVehicleIdEnergyEntriesStats(vehicleId.value, {
      energyTypes: undefined,
    })
    energystats.value = response
  } catch (err) {
    console.error('Failed to load energy stats:', err)
    energystats.value = null
  } finally {
    statsLoading.value = false
  }
}

async function loadGlobalStats() {
  if (!vehicleId.value) return

  try {
    const response = await getApiVehiclesVehicleIdEnergyEntriesStats(vehicleId.value, {
      energyTypes: undefined,
    })
    globalStats.value = response
    energystats.value = response
  } catch (err) {
    console.error('Failed to load global stats:', err)
    globalStats.value = null
  }
}

const mockStats = {
  totalDistance: 12450,
  totalFuelCost: 2340,
  avgConsumption: 7.2,
  lastFuelCost: 320.5,
  avgFuelPrice: 6.45,
  lastFuelDate: '2024-12-20',
  monthlyFuelCost: 650.0,
  totalServiceCost: 2350.0,
}

function handleEnergyEntryChanged() {
  loadEnergyStats()
  loadGlobalStats()
}

function openEditVehicleDialog() {
  editVehicleDialog.value = true
}

function closeEditVehicleDialog() {
  editVehicleDialog.value = false
}

interface ApiErrorResponse {
  type: string
  title: string
  status: number
  detail: string
  errors?: Array<{ code: string; description: string; type: string }>
  traceId?: string
}

async function handleVehicleUpdated(vehicleData: VehicleUpdateRequest) {
  if (!vehicleId.value) return

  try {
    await putApiVehiclesId(vehicleId.value, vehicleData)
    closeEditVehicleDialog()
    await loadVehicle()
  } catch (error) {
    console.error('Failed to update vehicle:', error)
  }
}

const updateFabForTab = () => {
  if (activeTab.value === 'overview') {
    registerFabMenu({
      icon: 'mdi-plus',
      text: 'Add',
      menuItems: [
        {
          key: 'fuel',
          icon: 'mdi-gas-station',
          text: 'Add Fuel',
          action: () => {
            activeTab.value = 'fuel'
            nextTick(() => {
              energyEntriesState.openCreateDialog()
            })
          },
        },
        {
          key: 'service',
          icon: 'mdi-wrench',
          text: 'Add Service',
          color: 'secondary',
          action: () => {
            activeTab.value = 'service'
            nextTick(() => {
              detailsState.create()
            })
          },
        },
      ],
    })
  } else if (activeTab.value === 'fuel') {
    registerFab({
      icon: 'mdi-gas-station',
      text: 'Add Fuel',
      action: () => energyEntriesState.openCreateDialog(),
    })
  } else if (activeTab.value === 'service') {
    registerFab({
      icon: 'mdi-plus',
      text: 'Add Service',
      action: () => {
        detailsState.create()
      },
    })
  }
}

onMounted(async () => {
  await loadVehicle()
  updateFabForTab()
})

onUnmounted(() => {
  unregisterFab()
})

watch(activeTab, () => {
  updateFabForTab()
  closeServiceDetailsSheet()
})
</script>

<template>
  <div v-if="loading" class="page-content">
    <div class="d-flex align-center mb-6">
      <v-skeleton-loader type="button" width="40" height="40" class="mr-4"></v-skeleton-loader>
      <div class="flex-grow-1">
        <v-skeleton-loader type="heading" width="200" class="mb-2"></v-skeleton-loader>
        <v-skeleton-loader type="text" width="300"></v-skeleton-loader>
      </div>
    </div>
    <section class="summary-section mb-6">
      <v-row>
        <v-col v-for="n in 4" :key="n" cols="12" sm="6" md="3">
          <v-card class="card-background summary-card" height="120">
            <v-card-text>
              <v-skeleton-loader type="text" width="60%" class="mb-2"></v-skeleton-loader>
              <v-skeleton-loader type="heading" width="80%"></v-skeleton-loader>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>
  </div>

  <div v-else-if="error" class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="error" class="mb-4">mdi-alert-circle</v-icon>
    <h3 class="text-h5 mb-2 text-error">{{ error }}</h3>
    <v-btn @click="loadVehicle" color="primary" variant="outlined">
      <v-icon start>mdi-refresh</v-icon>
      Retry
    </v-btn>
  </div>

  <div v-else-if="vehicle" class="page-content">
    <v-tabs
      v-model="activeTab"
      align-tabs="center"
      hide-slider
      mandatory
      grow
      selected-class="selected-tab"
      height="64"
      class="mb-4 tabs-container"
    >
      <v-tab value="overview" rounded="pill">
        <v-icon :icon="activeTab === 'overview' ? 'mdi-information' : 'mdi-information-outline'" start size="24" />
        <span class="tab-text">Overview</span>
      </v-tab>
      <v-tab value="fuel" rounded="pill">
        <v-icon :icon="activeTab === 'fuel' ? 'mdi-gas-station' : 'mdi-gas-station-outline'" start size="24" />
        <span class="tab-text">Fuel</span>
      </v-tab>
      <v-tab value="service" rounded="pill">
        <v-icon :icon="activeTab === 'service' ? 'mdi-wrench' : 'mdi-wrench-outline'" start size="24" />
        <span class="tab-text">Service</span>
      </v-tab>
    </v-tabs>

    <v-window v-model="activeTab" :continuous="false" :touch="false">
      <v-window-item value="overview">
        <VehicleOverviewTab
          :vehicle="vehicle"
          :last-entered-mileage="1234"
          :global-stats="globalStats"
          :summary-stats="summaryStats"
          @edit-vehicle="openEditVehicleDialog"
        />
      </v-window-item>

      <v-window-item value="fuel">
        <VehicleFuelTab
          :vehicle-id="vehicle.id!"
          :allowedEnergyTypes="vehicle?.allowedEnergyTypes"
          :energystats="energystats"
          @entry-changed="handleEnergyEntryChanged"
        />
      </v-window-item>

      <v-window-item value="service">
        <VehicleServiceTab
          :vehicle-id="vehicle.id!"
          :selected-service-records="selectedServiceRecords"
          :mock-stats="mockStats"
          @update:selected-service-records="selectedServiceRecords = $event"
        />
      </v-window-item>
    </v-window>
  </div>

  <div v-else class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="warning" class="mb-4">mdi-car-off</v-icon>
    <h3 class="text-h5 mb-2">No vehicle found</h3>
    <p class="text-body-1 text-medium-emphasis">The requested vehicle could not be found.</p>
  </div>

  <VehicleFormDialog
    ref="vehicleFormDialogRef"
    :is-open="editVehicleDialog"
    :vehicle="vehicle"
    @update:is-open="editVehicleDialog = $event"
    @save="handleVehicleUpdated"
  />
</template>

<style scoped>
/* Layout */
.details-container {
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

.details-item {
  display: flex;
  flex-direction: row;
  gap: 8px;
  align-items: center;
}

.details-item-data {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.details-item-label {
  font-weight: 400;
  color: rgba(var(--v-theme-on-surface-variant), 0.8);
}

.tabs-container {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 9999px;
}

.selected-tab {
  background-color: rgb(var(--v-theme-secondary-container));
}

.equal-height-row {
  align-items: stretch;
}

.equal-height-row .v-col {
  display: flex;
}

.equal-height-row .v-card {
  flex: 1;
}

.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.summary-card {
  border-radius: 16px;
}

.summary-section,
.details-section,
.fuel-section,
.service-section {
  scroll-margin-top: 80px;
  margin-bottom: 24px;
}

.tab-text {
  font-family: 'Roboto', sans-serif;
  font-size: 16px;
  font-weight: 500;
  line-height: 24px;
  height: 24px;
  letter-spacing: normal;
}

@media (max-width: 599px) {
  .summary-card {
    margin-bottom: 8px;
  }
}

@media (min-width: 600px) and (max-width: 959px) {
  .summary-card {
    margin-bottom: 12px;
  }
}
</style>

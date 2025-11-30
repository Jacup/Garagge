<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { VehicleDto, EnergyEntryDto, VehicleUpdateRequest, EnergyStatsDto } from '@/api/generated/apiV1.schemas'
import VehicleOverviewTab from '@/views/vehicles/tabs/VehicleOverviewTab.vue'
import VehicleFuelTab from '@/views/vehicles/tabs/VehicleFuelTab.vue'
import VehicleServiceTab from '@/views/vehicles/tabs/VehicleServiceTab.vue'
import ModifyEnergyEntryDialog from '@/components/vehicles/energyEntries/ModifyEnergyEntryDialog.vue'
import VehicleFormDialog from '@/components/vehicles/VehicleFormDialog.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import EnergyEntriesTable from '@/components/vehicles/EnergyEntriesTable.vue'
import { useLayoutFab } from '@/composables/useLayoutFab'
import { useServiceDetailsState } from '@/composables/vehicle/useServiceDetailsState';

const route = useRoute()
const { getApiVehiclesId, putApiVehiclesId } = getVehicles()
const { getApiVehiclesVehicleIdEnergyEntries, getApiVehiclesVehicleIdEnergyEntriesStats } = getEnergyEntries()
const { registerFab, registerFabMenu, unregisterFab } = useLayoutFab()
const { close: closeServiceDetailsSheet } = useServiceDetailsState();
const detailsState = useServiceDetailsState();

const vehicleId = ref(route.params.id as string)
const selectedVehicle = ref<VehicleDto | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const energyEntries = ref<EnergyEntryDto[]>([])
const timelineLoading = ref(false)

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

const lastEnteredMileage = computed(() => {
  if (!energyEntries.value || energyEntries.value.length === 0) return null

  const sortedEntries = [...energyEntries.value].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())

  return sortedEntries[0]?.mileage ?? null
})

const activeTab = ref('overview')

const addDialog = ref(false)
const bulkDeleteDialog = ref(false)
const editVehicleDialog = ref(false)

const selectedEnergyEntries = ref<string[]>([])

const selectedServiceRecords = ref<string[]>([])

const energyEntriesTableRef = ref<InstanceType<typeof EnergyEntriesTable> | null>(null)
const vehicleFormDialogRef = ref<InstanceType<typeof VehicleFormDialog> | null>(null)

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
    selectedVehicle.value = response.data
    await loadEnergyEntries()
    await loadGlobalStats()
  } catch (err) {
    console.error('Failed to load vehicle:', err)
    error.value = 'Failed to load vehicle data'
  } finally {
    loading.value = false
  }
}

async function loadEnergyEntries() {
  if (!vehicleId.value) return

  try {
    timelineLoading.value = true
    const response = await getApiVehiclesVehicleIdEnergyEntries(vehicleId.value, {
      page: 1,
      pageSize: 100,
    })
    energyEntries.value = response.data.items ?? []
  } catch (err) {
    console.error('Failed to load energy entries for timeline:', err)
    energyEntries.value = []
  } finally {
    timelineLoading.value = false
  }
}

async function loadEnergyStats() {
  if (!vehicleId.value) return

  try {
    statsLoading.value = true
    const response = await getApiVehiclesVehicleIdEnergyEntriesStats(vehicleId.value, {
      energyTypes: undefined,
    })
    energystats.value = response.data
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
    globalStats.value = response.data
    energystats.value = response.data
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

function openAddDialog() {
  addDialog.value = true
}

function closeAddDialog() {
  addDialog.value = false
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
  try {
    await putApiVehiclesId(vehicleId.value, vehicleData)
    closeEditVehicleDialog()
    await loadVehicle()
  } catch (error) {
    console.error('Failed to update vehicle:', error)
    if (vehicleFormDialogRef.value && error && typeof error === 'object' && 'response' in error) {
      const axiosError = error as { response?: { data?: ApiErrorResponse } }
      if (axiosError.response?.data) {
        vehicleFormDialogRef.value.setApiError(axiosError.response.data)
      }
    }
  }
}

function handleEntrySaved() {
  closeAddDialog()
  loadEnergyEntries()
  loadEnergyStats()
  loadGlobalStats()
  energyEntriesTableRef.value?.loadEnergyEntries()
}

function openBulkDeleteDialog() {
  bulkDeleteDialog.value = true
}

function closeBulkDeleteDialog() {
  bulkDeleteDialog.value = false
}

async function confirmBulkDelete() {
  if (selectedEnergyEntries.value.length > 0) {
    await energyEntriesTableRef.value?.removeMultiple(selectedEnergyEntries.value)
    selectedEnergyEntries.value = []
    await loadEnergyStats()
    await loadGlobalStats()
  }
  bulkDeleteDialog.value = false
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
              addDialog.value = true
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
      action: () => (addDialog.value = true),
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
  <!-- Loading State with Skeleton -->
  <div v-if="loading" class="page-content">
    <!-- Skeleton for Navigation Header -->
    <div class="d-flex align-center mb-6">
      <v-skeleton-loader type="button" width="40" height="40" class="mr-4"></v-skeleton-loader>
      <div class="flex-grow-1">
        <v-skeleton-loader type="heading" width="200" class="mb-2"></v-skeleton-loader>
        <v-skeleton-loader type="text" width="300"></v-skeleton-loader>
      </div>
    </div>

    <!-- Skeleton for Summary Cards -->
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

    <!-- Skeleton for Details Section -->
    <section class="details-section mb-6">
      <v-row class="equal-height-row">
        <v-col cols="12" md="4">
          <v-card class="card-background" height="320">
            <v-card-title>
              <v-skeleton-loader type="text" width="120"></v-skeleton-loader>
            </v-card-title>
            <v-card-text class="d-flex flex-column align-center justify-center h-100">
              <v-skeleton-loader type="avatar" size="120" class="mb-4"></v-skeleton-loader>
              <v-skeleton-loader type="button" width="100"></v-skeleton-loader>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" md="8">
          <v-card class="card-background" height="320">
            <v-card-title>
              <v-skeleton-loader type="text" width="140"></v-skeleton-loader>
            </v-card-title>
            <v-card-text>
              <v-row>
                <v-col v-for="n in 5" :key="n" cols="12" :sm="n === 5 ? 12 : 6">
                  <v-skeleton-loader type="text" width="80" class="mb-2"></v-skeleton-loader>
                  <v-skeleton-loader type="text" width="100%"></v-skeleton-loader>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>

    <!-- Skeleton for remaining sections -->
    <section class="mb-6">
      <v-row class="equal-height-row">
        <v-col cols="12" md="4">
          <v-card class="card-background" height="400">
            <v-card-title>
              <v-skeleton-loader type="text" width="120"></v-skeleton-loader>
            </v-card-title>
            <v-card-text>
              <div v-for="n in 5" :key="n" class="mb-4">
                <v-skeleton-loader type="text" width="60%" class="mb-1"></v-skeleton-loader>
                <v-skeleton-loader type="text" width="40%"></v-skeleton-loader>
              </div>
            </v-card-text>
          </v-card>
        </v-col>
        <v-col cols="12" md="8">
          <v-card class="card-background" height="400">
            <v-card-title>
              <v-skeleton-loader type="text" width="120"></v-skeleton-loader>
            </v-card-title>
            <v-card-text>
              <v-skeleton-loader type="table" class="mt-4"></v-skeleton-loader>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>
  </div>
  <!-- Error State -->
  <div v-else-if="error" class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="error" class="mb-4">mdi-alert-circle</v-icon>
    <h3 class="text-h5 mb-2 text-error">{{ error }}</h3>
    <v-btn @click="loadVehicle" color="primary" variant="outlined">
      <v-icon start>mdi-refresh</v-icon>
      Retry
    </v-btn>
  </div>

  <!-- Main Content -->
  <div v-else-if="selectedVehicle" class="page-content">
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

    <!-- Tab Content -->
    <v-window v-model="activeTab">
      <v-window-item value="overview">
        <VehicleOverviewTab
          :vehicle="selectedVehicle"
          :last-entered-mileage="lastEnteredMileage"
          :global-stats="globalStats"
          :summary-stats="summaryStats"
          @edit-vehicle="openEditVehicleDialog"
        />
      </v-window-item>

      <v-window-item value="fuel">
        <VehicleFuelTab
          :vehicle-id="vehicleId"
          :allowed-energy-types="selectedVehicle?.allowedEnergyTypes"
          :energystats="energystats"
          :stats-loading="statsLoading"
          :selected-energy-entries="selectedEnergyEntries"
          @update:selected-energy-entries="selectedEnergyEntries = $event"
          @entry-changed="handleEnergyEntryChanged"
          @add-entry="openAddDialog"
          @bulk-delete="openBulkDeleteDialog"
        />
      </v-window-item>

      <v-window-item value="service">
        <VehicleServiceTab
          :vehicle-id="vehicleId"
          :selected-service-records="selectedServiceRecords"
          :mock-stats="mockStats"
          @update:selected-service-records="selectedServiceRecords = $event"
        />
      </v-window-item>
    </v-window>
  </div>

  <!-- No Vehicle State -->
  <div v-else class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="warning" class="mb-4">mdi-car-off</v-icon>
    <h3 class="text-h5 mb-2">No vehicle found</h3>
    <p class="text-body-1 text-medium-emphasis">The requested vehicle could not be found.</p>
  </div>

  <ModifyEnergyEntryDialog
    :is-open="addDialog"
    :vehicle-id="vehicleId"
    :allowed-energy-types="selectedVehicle?.allowedEnergyTypes"
    :on-save="handleEntrySaved"
    :on-cancel="closeAddDialog"
  />

  <VehicleFormDialog
    ref="vehicleFormDialogRef"
    :is-open="editVehicleDialog"
    :vehicle="selectedVehicle"
    @update:is-open="editVehicleDialog = $event"
    @save="handleVehicleUpdated"
  />

  <DeleteDialog
    :is-open="bulkDeleteDialog"
    :item-to-delete="`${selectedEnergyEntries.length} energy entries`"
    :on-confirm="confirmBulkDelete"
    :on-cancel="closeBulkDeleteDialog"
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

.details-item-label {
  font-weight: 400;
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

.stat-value {
  margin-top: 8px;
}

.consumption-values {
  display: flex;
  flex-direction: column;
}

.consumption-item {
  display: flex;
  align-items: baseline;
}

.action-btn {
  text-transform: none;
  font-weight: 500;
}

.action-btn-small {
  text-transform: none;
  font-weight: 500;
  height: 32px;
  min-width: 80px;
  font-size: 0.875rem;
}

.summary-section,
.details-section,
.fuel-section,
.service-section {
  scroll-margin-top: 80px; /* Account for app bar */
  margin-bottom: 24px; /* Consistent spacing between sections */
}

.vehicle-image-card,
.vehicle-details-card,
.fuel-stats-card,
.fuel-history-card,
.service-stats-card,
.service-timeline-card {
  min-height: fit-content;
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

.v-card-title {
  padding-left: 16px;
  padding-right: 16px;
}

.v-card-title.d-flex {
  padding-right: 8px;
}

.stats-grid .stat-item {
  padding: 12px 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(var(--v-theme-outline), 0.12);
}

.stats-grid .stat-item:last-child {
  border-bottom: none;
}

.timeline-card {
  border-left: 2px solid rgba(var(--v-theme-primary), 0.12);
  margin-left: 8px;
}

.fuel-table {
  height: 100% !important;
  background-color: transparent !important;
}

.fuel-table :deep(.v-data-table__th) {
  background-color: rgba(var(--v-theme-primary), 0.12) !important;
}

.fuel-table .v-data-table__wrapper {
  height: calc(400px - 56px) !important;
}

.service-timeline-card .v-card-text {
  padding: 0 !important;
}

.service-timeline-card .v-timeline {
  padding: 16px;
}

@media (max-width: 959px) {
  .stats-grid .stat-item {
    text-align: center;
    padding: 12px;
  }

  .timeline-card {
    margin-left: 0;
    border-left: none;
    padding-left: 0 !important;
  }
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

  .image-card {
    min-height: 150px;
    margin-bottom: 16px;
  }
}

@media (min-width: 600px) and (max-width: 959px) {
  .summary-card {
    margin-bottom: 12px;
  }
}

.energy-entries-card .v-card-text {
  overflow: hidden;
}
</style>

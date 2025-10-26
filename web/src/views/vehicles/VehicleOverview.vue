<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { VehicleDto, EnergyEntryDto, VehicleUpdateRequest, EnergyStatsDto } from '@/api/generated/apiV1.schemas'
import EnergyEntriesTable from '@/components/vehicles/EnergyEntriesTable.vue'
import VehicleDetailItem from '@/components/vehicles/VehicleDetailItem.vue'
import ModifyEnergyEntryDialog from '@/components/vehicles/energyEntries/ModifyEnergyEntryDialog.vue'
import VehicleFormDialog from '@/components/vehicles/VehicleFormDialog.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import EnergyStatisticsCard from '@/components/vehicles/EnergyStatisticsCard.vue'

const route = useRoute()
const { getApiVehiclesId, putApiVehiclesId } = getVehicles()
const { getApiVehiclesVehicleIdEnergyEntries, getApiVehiclesVehicleIdEnergyEntriesStats } = getEnergyEntries()

// Vehicle data
const vehicleId = computed(() => route.params.id as string)
const selectedVehicle = ref<VehicleDto | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

// Energy entries for timeline
const energyEntries = ref<EnergyEntryDto[]>([])
const timelineLoading = ref(false)

const energystats = ref<EnergyStatsDto | null>(null)
const statsLoading = ref(false)

// Global stats for summary cards (always unfiltered)
const globalStats = ref<EnergyStatsDto | null>(null)

// Computed: Get unit label with proper formatting (used in summary cards)
const getUnitLabel = (unit: string | undefined): string => {
  if (!unit) return ''
  switch (unit) {
    case 'Liter': return 'L'
    case 'Gallon': return 'gal'
    case 'CubicMeter': return 'm³'
    case 'kWh': return 'kWh'
    default: return unit
  }
}

// Computed: Summary card stats (aggregated from first unit or totals)
const summaryStats = computed(() => {
  if (!globalStats.value) return null

  const firstUnit = globalStats.value.energyUnitStats[0]

  // Get all consumption data for display
  const consumptions = globalStats.value.energyUnitStats
    .filter(stat => stat.averageConsumption && stat.averageConsumption > 0)
    .map(stat => ({
      value: stat.averageConsumption,
      unit: stat.unit === 'kWh' ? 'kWh/100km' : 'L/100km'
    }))

  return {
    totalEntries: globalStats.value.totalEntries,
    totalCost: globalStats.value.totalCost,
    // Use first unit's volume and consumption, or 0 if none
    totalVolume: firstUnit?.totalVolume ?? 0,
    volumeUnit: getUnitLabel(firstUnit?.unit),
    consumptions: consumptions
  }
})

// Computed: Last entered mileage from energy entries
const lastEnteredMileage = computed(() => {
  if (!energyEntries.value || energyEntries.value.length === 0) return null

  // Sort by date descending and get the first entry (most recent)
  const sortedEntries = [...energyEntries.value].sort((a, b) =>
    new Date(b.date).getTime() - new Date(a.date).getTime()
  )

  return sortedEntries[0]?.mileage ?? null
})

// Dialog state
const addDialog = ref(false)
const bulkDeleteDialog = ref(false)
const editVehicleDialog = ref(false)

// Selection state for energy entries
const selectedEnergyEntries = ref<string[]>([])

// Component refs
const energyEntriesTableRef = ref<InstanceType<typeof EnergyEntriesTable> | null>(null)
const vehicleFormDialogRef = ref<InstanceType<typeof VehicleFormDialog> | null>(null)

// Load vehicle data from API
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
    // Load energy entries for timeline
    await loadEnergyEntries()
    // Load global stats (also sets initial energystats since no filters at start)
    await loadGlobalStats()
  } catch (err) {
    console.error('Failed to load vehicle:', err)
    error.value = 'Failed to load vehicle data'
  } finally {
    loading.value = false
  }
}

// Load energy entries for timeline
async function loadEnergyEntries() {
  if (!vehicleId.value) return

  try {
    timelineLoading.value = true
    // Get all energy entries (high page size to get full timeline)
    const response = await getApiVehiclesVehicleIdEnergyEntries(vehicleId.value, {
      page: 1,
      pageSize: 1000, // Get all entries for timeline
    })
    energyEntries.value = response.data.items ?? []
  } catch (err) {
    console.error('Failed to load energy entries for timeline:', err)
    energyEntries.value = []
  } finally {
    timelineLoading.value = false
  }
}

// Load energy statistics (called on initial load and when filters change in child component)
async function loadEnergyStats() {
  if (!vehicleId.value) return

  try {
    statsLoading.value = true
    // Always fetch all stats (filtering happens in EnergyStatisticsCard component)
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

// Load global statistics for summary cards (always unfiltered)
async function loadGlobalStats() {
  if (!vehicleId.value) return

  try {
    const response = await getApiVehiclesVehicleIdEnergyEntriesStats(vehicleId.value, {
      energyTypes: undefined, // Always get all stats for summary cards
    })
    globalStats.value = response.data
    // Also set energystats to the same value initially
    energystats.value = response.data
  } catch (err) {
    console.error('Failed to load global stats:', err)
    globalStats.value = null
  }
}

// Mock statistics data (to be replaced with API calls)
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

// Mock service history data
const mockServiceHistory = ref([
  { date: '2025-03-15', service: 'Przegląd okresowy', description: 'Zaplanowany przegląd roczny', type: 'upcoming', cost: null },
  { date: '2024-11-15', service: 'Wymiana oleju', description: 'Wymiana oleju silnikowego i filtra', type: 'completed', cost: 180.0 },
  { date: '2024-08-22', service: 'Wymiana klocków', description: 'Wymiana klocków hamulcowych przód', type: 'completed', cost: 450.0 },
  { date: '2024-06-10', service: 'Przegląd AC', description: 'Serwis klimatyzacji', type: 'completed', cost: 220.0 },
  { date: '2024-03-15', service: 'Przegląd okresowy', description: 'Przegląd roczny + wymiana świec', type: 'completed', cost: 520.0 },
])

// Utility functions
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

// Handler for energy entry changes (edit/delete from table)
function handleEnergyEntryChanged() {
  loadEnergyStats()
  loadGlobalStats()
}

// Dialog functions
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

// API Error Response interfaces (same as in VehicleFormDialog)
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
    // Update vehicle via API
    await putApiVehiclesId(vehicleId.value, vehicleData)
    closeEditVehicleDialog()
    // Reload vehicle data to refresh the view
    await loadVehicle()
  } catch (error) {
    console.error('Failed to update vehicle:', error)
    // Pass error to the dialog for display
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
  loadGlobalStats() // Refresh summary cards
  // Refresh the energy entries table
  energyEntriesTableRef.value?.loadEnergyEntries()
}

// Bulk delete functions
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
    // Refresh stats after deletion
    await loadEnergyStats()
    await loadGlobalStats() // Refresh summary cards
  }
  bulkDeleteDialog.value = false
}

onMounted(async () => {
  await loadVehicle()
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
    <!-- Navigation Header -->
    <v-breadcrumbs
      :items="[
        { title: 'Vehicles', to: '/vehicles' },
        { title: `${selectedVehicle.brand} ${selectedVehicle.model}`, disabled: true },
      ]"
      class="mt-4 mb-4"
    />
    <!-- Enhanced Summary Cards with Material Design Colors -->
    <section class="summary-section mb-6">
      <v-row>
        <v-col cols="12" sm="6" md="3" class="grid-column">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon
                icon="mdi-gas-station"
                size="32"
                class="position-absolute text-primary"
                style="top: 12px; right: 16px; opacity: 0.6"
              />
              <div class="text-caption text-on-primary-container">Fuel Type</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">{{ selectedVehicle?.engineType || 'N/A' }}</div>
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
          <template #title>{{ selectedVehicle?.brand }} {{ selectedVehicle?.model }}</template>
          <template #append>
            <v-btn prepend-icon="mdi-pencil" variant="flat" color="primary" @click="openEditVehicleDialog">Edit</v-btn>
          </template>
          <template #subtitle>
            <v-chip variant="tonal" size="small" density="comfortable" rounded="lg">
              {{ selectedVehicle?.engineType }}
            </v-chip>
          </template>
          <v-card-text>
            <div class="details-items-container">
              <VehicleDetailItem
                icon="mdi-calendar"
                label="Year"
                :value="selectedVehicle?.manufacturedYear ? selectedVehicle.manufacturedYear : 'N/A'"
              />
              <v-spacer />
              <VehicleDetailItem icon="mdi-car" label="Type" :value="selectedVehicle?.type ? selectedVehicle.type : 'N/A'" />
              <v-spacer />
            </div>
            <v-divider class="my-3" />
            <div class="details-items-container">
              <VehicleDetailItem icon="mdi-pound" label="VIN" :value="selectedVehicle?.vin ? selectedVehicle.vin : 'N/A'" />
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

    <!-- Fuel Section -->
    <v-row class="equal-height-row">
      <v-col cols="12" md="8">
        <v-card class="card-background" variant="flat" rounded="md-16px" height="520px">
          <template #title>Fuel History</template>
          <template #append>
            <v-btn
              v-if="selectedEnergyEntries.length > 0"
              class="text-none mr-2"
              prepend-icon="mdi-delete"
              variant="flat"
              color="error"
              size="small"
              @click="openBulkDeleteDialog"
            >
              Delete ({{ selectedEnergyEntries.length }})
            </v-btn>
            <v-btn class="text-none" prepend-icon="mdi-plus" variant="flat" color="primary" size="small" @click="openAddDialog">
              Add
            </v-btn>
          </template>
          <v-card-text>
            <EnergyEntriesTable
              ref="energyEntriesTableRef"
              :vehicle-id="vehicleId"
              :allowed-energy-types="selectedVehicle?.allowedEnergyTypes"
              v-model:selected="selectedEnergyEntries"
              @entry-changed="handleEnergyEntryChanged"
            />
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <EnergyStatisticsCard
          :vehicle-id="vehicleId"
          :allowed-energy-types="selectedVehicle?.allowedEnergyTypes"
          :energystats="energystats"
          :stats-loading="statsLoading"
        />
      </v-col>
    </v-row>

    <!-- Service History -->
    <section class="service-section mb-6">
      <v-row class="equal-height-row">
        <v-col cols="12" md="4">
          <v-card class="service-stats-card card-background" height="400" variant="flat">
            <v-card-title>Service Statistics</v-card-title>
            <v-card-text>
              <div class="stats-grid">
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Total Cost</div>
                  <div class="text-body-2 font-weight-bold text-on-surface">{{ mockStats.totalServiceCost.toFixed(2) }} PLN</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Last Service</div>
                  <div class="text-body-2 font-weight-bold text-on-surface">{{ formatDate('2024-11-15') }}</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Next Service</div>
                  <div class="text-body-2 font-weight-bold text-on-surface">{{ formatDate('2025-03-15') }}</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Average Cost</div>
                  <div class="text-body-2 font-weight-bold text-on-surface">{{ (mockStats.totalServiceCost / 8).toFixed(2) }} PLN</div>
                </div>
              </div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" md="8">
          <v-card class="service-timeline-card card-background" height="400" variant="flat">
            <template #title>Service History</template>
            <template #append>
              <v-btn class="text-none" prepend-icon="mdi-plus" variant="flat" color="primary" disabled>Add</v-btn>
            </template>
            <v-card-text class="pa-4" style="height: 320px; overflow-y: auto">
              <v-timeline density="compact">
                <v-timeline-item
                  v-for="entry in mockServiceHistory"
                  :key="entry.date"
                  :dot-color="entry.type === 'upcoming' ? 'warning' : 'success'"
                  size="small"
                >
                  <template #default>
                    <div class="timeline-card pa-3">
                      <div class="d-flex justify-space-between align-center mb-2">
                        <h4 class="text-subtitle-1 font-weight-medium">{{ entry.service }}</h4>
                        <span class="text-body-2 text-medium-emphasis">{{ formatDate(entry.date) }}</span>
                      </div>
                      <p class="text-body-2 mb-2">{{ entry.description }}</p>
                      <div class="d-flex justify-space-between">
                        <span class="text-body-2 font-weight-medium">
                          <v-chip size="x-small" :color="entry.type === 'upcoming' ? 'warning' : 'success'" variant="tonal">
                            {{ entry.type === 'upcoming' ? 'Upcoming' : 'Completed' }}
                          </v-chip>
                        </span>
                        <span v-if="entry.cost" class="text-subtitle-2 font-weight-bold"> {{ entry.cost.toFixed(2) }} PLN </span>
                      </div>
                    </div>
                  </template>
                </v-timeline-item>
              </v-timeline>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>
  </div>

  <!-- No Vehicle State -->
  <div v-else class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="warning" class="mb-4">mdi-car-off</v-icon>
    <h3 class="text-h5 mb-2">No vehicle found</h3>
    <p class="text-body-1 text-medium-emphasis">The requested vehicle could not be found.</p>
  </div>

  <!-- Add Energy Entry Dialog -->
  <ModifyEnergyEntryDialog
    :is-open="addDialog"
    :vehicle-id="vehicleId"
    :allowed-energy-types="selectedVehicle?.allowedEnergyTypes"
    :on-save="handleEntrySaved"
    :on-cancel="closeAddDialog"
  />

  <!-- Edit Vehicle Dialog -->
  <VehicleFormDialog
    ref="vehicleFormDialogRef"
    :is-open="editVehicleDialog"
    :vehicle="selectedVehicle"
    @update:is-open="editVehicleDialog = $event"
    @save="handleVehicleUpdated"
  />

  <!-- Bulk Delete Dialog -->
  <DeleteDialog
    :is-open="bulkDeleteDialog"
    :item-to-delete="`${selectedEnergyEntries.length} energy entries`"
    :on-confirm="confirmBulkDelete"
    :on-cancel="closeBulkDeleteDialog"
  />
</template>

<style scoped>
/* Layout */
.page-content {
  margin-left: -12px;
  margin-right: -12px;
}

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

/* Consistent card background matching navigation */
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

/* Enhanced summary cards */
.summary-card {
  border-radius: 16px;
}

.stat-value {
  margin-top: 8px;
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

/* Enhanced buttons */
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

/* Section spacing */
.summary-section,
.details-section,
.fuel-section,
.service-section {
  scroll-margin-top: 80px; /* Account for app bar */
  margin-bottom: 24px; /* Consistent spacing between sections */
}

/* Vehicle cards */
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

/* Card titles with action buttons */
.v-card-title {
  padding-left: 16px;
  padding-right: 16px;
}

.v-card-title.d-flex {
  padding-right: 8px;
}

/* Stats grid for info cards */
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

/* Timeline customization */
.timeline-card {
  border-left: 2px solid rgba(var(--v-theme-primary), 0.12);
  margin-left: 8px;
}

/* Table styling - full container height */
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

/* Scrollable timeline */
.service-timeline-card .v-card-text {
  padding: 0 !important;
}

.service-timeline-card .v-timeline {
  padding: 16px;
}

/* Responsive improvements */
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

/* Mobile optimizations */
@media (max-width: 599px) {
  .summary-card {
    margin-bottom: 8px;
  }

  .image-card {
    min-height: 150px;
    margin-bottom: 16px;
  }
}

/* Tablet optimizations */
@media (min-width: 600px) and (max-width: 959px) {
  .summary-card {
    margin-bottom: 12px;
  }
}

.energy-entries-card .v-card-text {
  overflow: hidden;
}

/* Desktop optimizations */
</style>

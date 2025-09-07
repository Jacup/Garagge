<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import EnergyEntriesTable from '@/components/vehicles/EnergyEntriesTable.vue'

const route = useRoute()
const { getApiVehiclesMyId } = getVehicles()

// Vehicle data
const vehicleId = computed(() => route.params.id as string)
const selectedVehicle = ref<VehicleDto | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

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
    const response = await getApiVehiclesMyId(vehicleId.value)
    selectedVehicle.value = response.data
  } catch (err) {
    console.error('Failed to load vehicle:', err)
    error.value = 'Failed to load vehicle data'
  } finally {
    loading.value = false
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
              <div class="text-caption text-on-secondary-container">Total Distance</div>
              <div class="text-h6 font-weight-bold text-on-secondary-container">{{ mockStats.totalDistance.toLocaleString() }} km</div>
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
              <div class="text-caption text-on-tertiary-container">Fuel Cost</div>
              <div class="text-h6 font-weight-bold text-on-tertiary-container">${{ mockStats.totalFuelCost }}</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" sm="6" md="3">
          <v-card class="summary-card" height="120" color="surface-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon icon="mdi-car-info" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
              <div class="text-caption text-on-surface">Avg. Consumption</div>
              <div class="text-h6 font-weight-bold text-on-surface">{{ mockStats.avgConsumption }} L/100km</div>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>

    <!-- Main Details Section -->
    <section class="details-section mb-6">
      <v-row class="equal-height-row">
        <!-- Vehicle Image Card -->
        <v-col cols="12" md="4">
          <v-card class="card-background" height="320" variant="flat">
            <template #title>
              <span>Vehicle Image</span>
            </template>
            <v-card-text class="d-flex flex-column align-center justify-center h-100">
              <v-avatar size="120" class="mb-4">
                <v-img
                  :src="`https://via.placeholder.com/150x150/104C83/white?text=${selectedVehicle?.brand.charAt(0)}${selectedVehicle?.model.charAt(0)}`"
                  alt="Vehicle"
                />
              </v-avatar>
            </v-card-text>
          </v-card>
        </v-col>

        <!-- Vehicle Details Card -->
        <v-col cols="12" md="8">
          <v-card class="card-background" height="320" variant="flat">
            <template #title>
              <span>Vehicle Details</span>
            </template>
            <template #append>
              <v-btn class="text-none" prepend-icon="mdi-pencil" variant="flat" color="primary" disabled>Edit</v-btn>
            </template>
            <v-card-text>
              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field label="Brand" :model-value="selectedVehicle?.brand" readonly variant="outlined" density="compact" />
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field label="Model" :model-value="selectedVehicle?.model" readonly variant="outlined" density="compact" />
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    label="Year"
                    :model-value="selectedVehicle?.manufacturedYear"
                    readonly
                    variant="outlined"
                    density="compact"
                  />
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    label="Power Type"
                    :model-value="selectedVehicle?.engineType"
                    readonly
                    variant="outlined"
                    density="compact"
                  />
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    label="Vehicle Type"
                    :model-value="selectedVehicle?.type || 'Not specified'"
                    readonly
                    variant="outlined"
                    density="compact"
                  />
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="VIN"
                    :model-value="selectedVehicle?.vin || 'Not specified'"
                    readonly
                    variant="outlined"
                    density="compact"
                  />
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>

    <!-- Fuel & Service Section -->
    <section class="fuel-section mb-6">
      <v-row class="equal-height-row">
        <v-col cols="12" md="4">
          <v-card class="fuel-stats-card card-background" height="400" variant="flat">
            <v-card-title>Fuel Statistics</v-card-title>
            <v-card-text>
              <div class="stats-grid">
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Last Fill-up</div>
                  <div class="text-h6 font-weight-bold text-on-surface">${{ mockStats.lastFuelCost.toFixed(2) }}</div>
                  <div class="text-caption text-medium-emphasis">{{ mockStats.lastFuelDate }}</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Average Price</div>
                  <div class="text-h6 font-weight-bold text-on-surface">${{ mockStats.avgFuelPrice.toFixed(2) }}/L</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Monthly Average</div>
                  <div class="text-h6 font-weight-bold text-on-surface">${{ mockStats.monthlyFuelCost.toFixed(2) }}</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Total Spent</div>
                  <div class="text-h6 font-weight-bold text-on-surface">${{ mockStats.totalFuelCost.toFixed(2) }}</div>
                </div>
              </div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" md="8">
          <EnergyEntriesTable :vehicle-id="vehicleId" />
        </v-col>
      </v-row>
    </section>

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
                  <div class="text-h6 font-weight-bold text-on-surface">{{ mockStats.totalServiceCost.toFixed(2) }} PLN</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Last Service</div>
                  <div class="text-h6">{{ formatDate('2024-11-15') }}</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Next Service</div>
                  <div class="text-h6">{{ formatDate('2025-03-15') }}</div>
                </div>
                <div class="stat-item">
                  <div class="text-body-2 text-medium-emphasis">Average Cost</div>
                  <div class="text-h6">{{ (mockStats.totalServiceCost / 8).toFixed(2) }} PLN</div>
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
</template>

<style scoped>
/* Layout */
.page-content {
  max-width: 1200px;
  margin: 0 auto;
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
  border-radius: 16px;
  min-height: fit-content;
}

.vehicle-image-card {
  text-align: center;
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
  padding: 8px 0;
  border-bottom: 1px solid rgba(var(--v-theme-outline), 0.12);
}

.stats-grid .stat-item:last-child {
  border-bottom: none;
}

/* Timeline customization */
.timeline-card {
  border-left: 2px solid rgba(var(--v-theme-primary), 0.12);
  margin-left: 8px;
  padding-left: 12px !important;
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

/* Desktop optimizations */
</style>

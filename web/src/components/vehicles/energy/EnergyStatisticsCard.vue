<script setup lang="ts">
import { ref, computed } from 'vue'
import type { EnergyStatsDto } from '@/api/generated/apiV1.schemas'
import { EnergyType } from '@/api/generated/apiV1.schemas'

interface Props {
  vehicleId: string
  allowedEnergyTypes?: EnergyType[]
  energystats: EnergyStatsDto | null
  statsLoading: boolean
}

const props = defineProps<Props>()

// Filter state for energy types
const selectedEnergyTypeFilters = ref<EnergyType[]>([])

// Computed: Filter energy unit stats based on selected energy type filters
const filteredStats = computed(() => {
  if (!props.energystats) return []

  // If no filters selected, return all unit stats
  if (selectedEnergyTypeFilters.value.length === 0) {
    return props.energystats.energyUnitStats
  }

  // Filter unit stats that contain any of the selected energy types
  return props.energystats.energyUnitStats.filter(unitStat =>
    unitStat.energyTypes?.some(type => selectedEnergyTypeFilters.value.includes(type))
  )
})

// Computed: Get unit label with proper formatting
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
</script>

<template>
  <v-card class="fuel-stats-card card-background" variant="flat" rounded="md-16px" height="520px">
    <template #title>Fuel Statistics</template>
    <template #append>
      <v-chip-group
        v-model="selectedEnergyTypeFilters"
        multiple
        filter
        column
        selected-class="filter-chip-selected"
      >
        <v-chip
          v-for="energyType in allowedEnergyTypes"
          :key="energyType"
          :value="energyType"
          variant="outlined"
          filter
          class="filter-chip"
        >
          {{ energyType }}
        </v-chip>
      </v-chip-group>
    </template>
    <v-card-text>
      <div v-if="statsLoading" class="d-flex justify-center align-center" style="height: 400px">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
      </div>
      <div v-else-if="energystats && filteredStats.length > 0">
        <!-- Always show total cost at the top -->
        <div class="mb-4 pa-3 rounded" style="background-color: rgba(var(--v-theme-primary), 0.08)">
          <div class="d-flex justify-space-between align-center">
            <div class="text-body-2 text-medium-emphasis">Total Cost</div>
            <div class="text-h6 font-weight-bold">${{ energystats.totalCost.toFixed(2) }}</div>
          </div>
        </div>

        <!-- Stats per unit -->
        <div v-for="(unitStat, index) in filteredStats" :key="unitStat.unit" :class="{ 'mt-4': index > 0 }">
          <!-- Unit header (only show if multiple units) -->
          <div v-if="filteredStats.length > 1" class="mb-2">
            <v-chip size="small" color="primary" variant="tonal">
              {{ unitStat.unit }} ({{ unitStat.energyTypes?.join(', ') }})
            </v-chip>
          </div>

          <!-- Unit statistics -->
          <div class="stats-grid">
            <div class="stat-item">
              <div class="text-body-2 text-medium-emphasis">Total Volume</div>
              <div class="text-body-2 font-weight-bold text-on-surface">
                {{ unitStat.totalVolume?.toFixed(2) ?? 'N/A' }} {{ getUnitLabel(unitStat.unit) }}
              </div>
            </div>
            <div class="stat-item">
              <div class="text-body-2 text-medium-emphasis">Average Consumption</div>
              <div class="text-body-2 font-weight-bold text-on-surface">
                {{ unitStat.averageConsumption?.toFixed(2) ?? 'N/A' }} {{ getUnitLabel(unitStat.unit) }}/100km
              </div>
            </div>
            <div class="stat-item">
              <div class="text-body-2 text-medium-emphasis">Average Price</div>
              <div class="text-body-2 font-weight-bold text-on-surface">
                ${{ unitStat.averagePricePerUnit?.toFixed(2) ?? 'N/A' }}/{{ getUnitLabel(unitStat.unit) }}
              </div>
            </div>
            <!-- Only show Total Cost in unit-specific section if multiple units -->
            <div v-if="filteredStats.length > 1" class="stat-item">
              <div class="text-body-2 text-medium-emphasis">Total Cost</div>
              <div class="text-body-2 font-weight-bold text-on-surface">${{ unitStat.totalCost?.toFixed(2) ?? 'N/A' }}</div>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="d-flex justify-center align-center flex-column" style="height: 400px">
        <v-icon size="48" color="grey" class="mb-2">mdi-chart-box-outline</v-icon>
        <span class="text-body-2 text-medium-emphasis">No statistics available</span>
      </div>
    </v-card-text>
  </v-card>
</template>

<style scoped>
/* Consistent card background matching navigation */
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
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

/* Fuel stats card - allow scrolling when multiple units */
.fuel-stats-card .v-card-text {
  max-height: 460px;
  overflow-y: auto;
}

/* Fuel stats card - chip group styling */
.fuel-stats-card :deep(.v-chip-group) {
  max-width: 400px;
}

/* MD3 Filter Chip styling */
.filter-chip {
  /* Unselected state - outlined variant */
  background-color: transparent !important;
  border-color: rgb(var(--v-theme-outline)) !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-radius: 8px !important;
}

.filter-chip-selected {
  /* Selected state - flat variant with secondary-container */
  background-color: rgb(var(--v-theme-secondary-container)) !important;
  border-width: 0 !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-radius: 8px !important;
}

/* Ensure filter icon inherits correct color */
.filter-chip :deep(.v-icon) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.filter-chip-selected :deep(.v-icon) {
  color: rgb(var(--v-theme-on-secondary-container)) !important;
}
</style>

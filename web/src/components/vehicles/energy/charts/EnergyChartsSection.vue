<script lang="ts" setup>
import { ref, computed } from 'vue'
import EnergyPriceChart from '@/components/vehicles/energy/charts/EnergyPriceChart.vue'
import ChartSelector, { type ChartType } from '@/components/vehicles/energy/charts/ChartSelector.vue'
import EnergyConsumptionChart from './EnergyConsumptionChart.vue'
import RefuelCostChart from './RefuelCostChart.vue'

import type { EnergyType, EnergyUnit } from '@/api/generated/apiV1.schemas'

export interface EnergyChartEntryDto {
  date: string
  type: EnergyType
  energyUnit: EnergyUnit
  pricePerUnit: number | null
  consumption: number | null
  cost: number | null
}

const props = defineProps<{
  entries: EnergyChartEntryDto[]
  dataPeriod: 0 | 1 | 2 | 3
}>()

const selectedChart = ref<ChartType>('price')

const priceEntries = computed(() =>
  props.entries.filter((e) => e.pricePerUnit !== null).map((e) => ({ date: e.date, type: e.type, pricePerUnit: e.pricePerUnit! })),
)

const consumptionEntries = computed(() =>
  props.entries
    .filter((e) => e.consumption !== null)
    .map((e) => ({ date: e.date, type: e.type, energyUnit: e.energyUnit, consumption: e.consumption! })),
)

const refuelCostEntries = computed(() =>
  props.entries.filter((e) => e.cost !== null).map((e) => ({ date: e.date, type: e.type, cost: e.cost! })),
)
</script>

<template>
  <v-card variant="flat" class="fuel-card d-flex flex-column" height="430px" rounded="md-16px">
    <v-card-title class="d-flex flex-row flex-grow-0 align-center">
      <span>Analytics</span>
      <v-spacer />
      <ChartSelector v-model="selectedChart" />
    </v-card-title>

    <v-card-text class="flex-grow-1 overflow-hidden">
      <EnergyPriceChart class="h-100" v-if="selectedChart === 'price'" :entries="priceEntries" :data-period="dataPeriod" />
      <EnergyConsumptionChart
        class="h-100"
        v-else-if="selectedChart === 'consumption'"
        :entries="consumptionEntries"
        :data-period="dataPeriod"
      />
      <RefuelCostChart class="h-100" v-else-if="selectedChart === 'refuel-cost'" :entries="refuelCostEntries" :data-period="dataPeriod" />
    </v-card-text>
  </v-card>
</template>

<style lang="css" scoped>
.fuel-card {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>

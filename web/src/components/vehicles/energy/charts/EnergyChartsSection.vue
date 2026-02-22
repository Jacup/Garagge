<script lang="ts" setup>
import EnergyPriceChart, { type FuelPriceEntry } from '@/components/vehicles/energy/charts/EnergyPriceChart.vue'

const props = defineProps<{
  entries: FuelPriceEntry[]
  dataPeriod: 0 | 1 | 2 | 3 // 0=Lifetime, 1=Year, 2=Month, 3=Week
}>()
</script>

<template>
  <v-card variant="flat" class="fuel-card" title="Analytics">
    <v-card-text>
      <EnergyPriceChart :entries="props.entries" :data-period="props.dataPeriod" />
    </v-card-text>
  </v-card>
</template>

<style lang="css" scoped>
.fuel-card {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>
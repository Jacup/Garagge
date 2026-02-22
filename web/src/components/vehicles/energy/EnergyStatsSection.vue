<script setup lang="ts">
import EnergyTypeStatCard from './EnergyTypeStatCard.vue'

defineProps<{
  stats: {
    type: string
    itemsCount: number
    totalCost: number
    totalVolume: number
    averageConsumption: number
    averagePricePerUnit: number
    averageCostPer100km: number
  }[]
}>()
</script>

<template>
  <v-card variant="flat" height="100%" min-height="424" rounded="md-16px" class="fuel-card fuel-type-stats-card">
    <v-card-item>
      <v-card-title>Stats per fuel type</v-card-title>
    </v-card-item>

    <v-card-text class="pb-2">
      <EnergyTypeStatCard v-for="stat in stats" :key="stat.type" :stats="stat" />
    </v-card-text>
  </v-card>
</template>

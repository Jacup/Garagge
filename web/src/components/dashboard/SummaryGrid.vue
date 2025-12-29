<script lang="ts" setup>
import StatCard from '@/components/dashboard/StatCard.vue'
import type { StatMetricDto } from '@/api/generated/apiV1.schemas'

defineProps<{
  fuelExpenses: StatMetricDto | null
  distanceDriven: StatMetricDto | null
}>()
</script>

<template>
  <v-row>
    <v-col cols="6" md="3">
      <StatCard
        title="Fuel Expenses"
        :metric="fuelExpenses"
        icon="mdi-gas-station"
        accent-color="primary"
      />
    </v-col>

    <v-col cols="6" md="3">
      <StatCard
        title="Distance Driven"
        :metric="distanceDriven"
        icon="mdi-map-marker-distance"
        accent-color="tertiary"
      />
    </v-col>
  </v-row>
</template>

<script lang="ts" setup>
import StatCard from '@/components/dashboard/StatCard.vue'
</script>

<template>
  <v-row>
    <v-col cols="6" md="3">
      <StatCard
        title="Fuel Expenses"
        subtitle="This Month"
        value="$1,284"
        chipValue="12%"
        chipColor="error"
        chipAppendText="vs last month"
        chipPrefixArrow="up"
        icon="mdi-gas-station"
        accent-color="primary"
      />
    </v-col>

    <v-col cols="6" md="3">
      <StatCard title="Maintenance" subtitle="Active alerts" value="3" icon="mdi-wrench" accent-color="warning" />
    </v-col>

    <v-col cols="6" md="3">
      <StatCard
        title="Distance"
        value="2,400 km"
        chipValue="+50 km"
        chipAppendText="this week"
        icon="mdi-map-marker-distance"
        accent-color="tertiary"
      />
    </v-col>

    <v-col cols="6" md="3">
      <StatCard
        title="Fleet Status"
        value="Optimal"
        chipValue="100% Active"
        secondaryTrend="good"
        icon="mdi-check-circle"
        accent-color="success"
      />
    </v-col>
  </v-row>
</template>

<style scoped></style>

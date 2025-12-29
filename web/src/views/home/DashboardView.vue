<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import { useUserStore } from '@/stores/user'
import SummaryGrid from '@/components/dashboard/SummaryGrid.vue'
import type { TimelineActivityDto, StatMetricDto } from '@/api/generated/apiV1.schemas'
import RecentActivity from '@/components/dashboard/RecentActivity.vue'
import EnergyChartCard from '@/components/dashboard/EnergyChartCard.vue'

import { getDashboard } from '@/api/generated/dashboard/dashboard'

const userStore = useUserStore()
const { getApiStats } = getDashboard()

const fuelExpenses = ref(null as StatMetricDto | null)
const distanceDriven = ref(null as StatMetricDto | null)
const recentActivity = ref([] as TimelineActivityDto[])
const loading = ref(true)

onMounted(() => {
  loadDashboardStats()
})

async function loadDashboardStats() {
  loading.value = true
  try {
    const res = await getApiStats()
    fuelExpenses.value = res.fuelExpenses ?? null
    distanceDriven.value = res.distanceDriven ?? null
    recentActivity.value = res.recentActivity ?? []
  } catch (error) {
    console.error('Error loading dashboard stats:', error)
  } finally {
    loading.value = false
  }
}

const username = ref('')
username.value = userStore.profile?.firstName || ''

</script>

<template>
  <div class="mb-6">
    <h1 class="text-h3 font-weight-light mb-2">Hi{{ username == '' ? '' : ', ' + username + '!👋' }}</h1>
    <p class="text-body-1 text-medium-emphasis">Here's what's happening with your fleet.</p>
  </div>

  <SummaryGrid :fuelExpenses="fuelExpenses" :distanceDriven="distanceDriven" />

  <v-row>
    <v-col cols="12" md="5" lg="4">
      <RecentActivity :recentActivity="recentActivity" />
    </v-col>

    <v-col cols="12" md="7" lg="8">
      <EnergyChartCard />
    </v-col>
  </v-row>
</template>

<style scoped>
.letter-spacing-1 {
  letter-spacing: 1px !important;
}
.leading-tight {
  line-height: 1.1 !important;
}
.z-index-1 {
  z-index: 1;
  position: relative;
}


.stat-icon {
  opacity: 0.7;
}

.chart-card .v-card-text {
  min-height: 200px;
}

.feature-card {
  transition:
    transform 0.2s ease-in-out,
    box-shadow 0.2s ease-in-out;
}

.feature-card:hover {
  transform: translateY(-4px);
}

.dashboard-view {
  background-color: rgb(var(--md-sys-color-background));
  color: rgb(var(--md-sys-color-on-background));
}
</style>

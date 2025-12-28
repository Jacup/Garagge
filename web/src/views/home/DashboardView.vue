<script lang="ts" setup>
import { ref } from 'vue'
import { useUserStore } from '@/stores/user'
import SummaryGrid from '@/components/dashboard/SummaryGrid.vue'
import RecentActivity from '@/components/dashboard/RecentActivity.vue'

const userStore = useUserStore()

const username = ref('')
username.value = userStore.profile?.firstName || ''

const recentActivity = ref([
  {
    title: 'Honda Civic - Fuel Added',
    subtitle: '$45.20 • 2 hours ago',
    date: '2024-06-10T14:30:00Z',
    icon: 'mdi-gas-station',
    color: 'primary',
  },
  {
    title: 'Toyota Prius - Service Completed',
    subtitle: 'Oil change • Yesterday',
    icon: 'mdi-wrench',
    color: 'success',
  },
  {
    title: 'BMW X3 - Inspection Due',
    subtitle: 'Due in 5 days',
    icon: 'mdi-clipboard-check',
    color: 'warning',
  },
  {
    title: 'Mercedes C-Class - Added to Fleet',
    subtitle: '3 days ago',
    icon: 'mdi-car-plus',
    color: 'secondary',
  },
])

const quickActions = ref([
  {
    title: 'Add Fuel Entry',
    icon: 'mdi-gas-station',
    color: 'primary',
  },
  {
    title: 'Log Maintenance',
    icon: 'mdi-wrench',
    color: 'secondary',
  },
  {
    title: 'Add Vehicle',
    icon: 'mdi-car-plus',
    color: 'tertiary',
  },
  {
    title: 'View Reports',
    icon: 'mdi-chart-line',
    color: 'success',
  },
])
</script>

<template>
  <!-- Header -->
  <div class="mb-6">
    <h1 class="text-h3 font-weight-light mb-2">Hi{{ username == '' ? '' : ', ' + username + '!👋' }}</h1>
    <p class="text-body-1 text-medium-emphasis">Here's what's happening with your fleet.</p>
  </div>

  <!-- Stats Cards -->
  <SummaryGrid />

  <!-- Recent Activity -->
  <v-row>
    <v-col cols="12" md="4">
    <RecentActivity :recentActivity="recentActivity" />
    </v-col>
  </v-row>


  <v-row>

    <!-- Quick Actions & Chart Placeholder -->
    <v-col cols="12" md="4">
      <v-row>
        <!-- Quick Actions -->
        <v-col cols="12">
          <v-card elevation="2" class="mb-4">
            <v-card-title class="d-flex align-center">
              <v-icon icon="mdi-lightning-bolt" class="me-2" />
              Quick Actions
            </v-card-title>
            <v-card-text>
              <v-row>
                <v-col v-for="action in quickActions" :key="action.title" cols="6">
                  <v-btn :color="action.color" variant="tonal" block class="flex-column py-4" height="80">
                    <v-icon :icon="action.icon" size="24" class="mb-1" />
                    <span class="text-caption">{{ action.title }}</span>
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </v-col>

        <!-- Chart Placeholder -->
        <v-col cols="12">
          <v-card elevation="2" class="chart-card">
            <v-card-title class="d-flex align-center">
              <v-icon icon="mdi-chart-donut" class="me-2" />
              Fuel Efficiency
            </v-card-title>
            <v-card-text class="d-flex flex-column align-center justify-center" style="height: 200px">
              <v-icon icon="mdi-chart-donut-variant" size="80" color="primary" class="mb-4" />
              <p class="text-body-2 text-center text-medium-emphasis">
                Chart will be displayed here<br />
                <small>Integration coming soon</small>
              </p>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </v-col>
  </v-row> -->

  <!-- Additional Cards Row -->
  <v-row class="mt-6">
    <v-col cols="12" md="4">
      <v-card elevation="2" class="feature-card">
        <v-card-text class="text-center pa-8">
          <v-icon icon="mdi-calendar-check" size="64" color="success" class="mb-4" />
          <h3 class="text-h6 mb-2">Maintenance Scheduler</h3>
          <p class="text-body-2 text-medium-emphasis mb-4">Keep track of upcoming maintenance and services</p>
          <v-btn color="success" variant="tonal"> Schedule Service </v-btn>
        </v-card-text>
      </v-card>
    </v-col>

    <v-col cols="12" md="4">
      <v-card elevation="2" class="feature-card">
        <v-card-text class="text-center pa-8">
          <v-icon icon="mdi-map-marker-path" size="64" color="info" class="mb-4" />
          <h3 class="text-h6 mb-2">Trip Tracking</h3>
          <p class="text-body-2 text-medium-emphasis mb-4">Monitor your routes and optimize fuel consumption</p>
          <v-btn color="info" variant="tonal"> View Trips </v-btn>
        </v-card-text>
      </v-card>
    </v-col>

    <v-col cols="12" md="4">
      <v-card elevation="2" class="feature-card">
        <v-card-text class="text-center pa-8">
          <v-icon icon="mdi-finance" size="64" color="warning" class="mb-4" />
          <h3 class="text-h6 mb-2">Cost Analysis</h3>
          <p class="text-body-2 text-medium-emphasis mb-4">Analyze expenses and optimize your fleet budget</p>
          <v-btn color="warning" variant="tonal"> View Reports </v-btn>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

<style scoped>
.letter-spacing-1 {
  letter-spacing: 1px !important;
}
/* Poprawa interlinii dla dużych cyfr */
.leading-tight {
  line-height: 1.1 !important;
}
/* Zapewnienie, że treść jest nad ikoną */
.z-index-1 {
  z-index: 1;
  position: relative;
}
.stat-card:hover {
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

/* Use Material Design colors for custom styling if needed */
.dashboard-view {
  background-color: rgb(var(--md-sys-color-background));
  color: rgb(var(--md-sys-color-on-background));
}
</style>

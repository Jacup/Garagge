<script lang="ts" setup>
import { ref } from 'vue'

// Placeholder data for demo cards
const statsCards = ref([
  {
    title: 'Total Vehicles',
    value: '12',
    icon: 'mdi-car-multiple',
    color: 'primary',
    subtitle: '3 active today',
  },
  {
    title: 'Fuel Expenses',
    value: '$1,284',
    icon: 'mdi-gas-station',
    color: 'secondary',
    subtitle: 'This month',
  },
  {
    title: 'Distance Driven',
    value: '2,847 km',
    icon: 'mdi-speedometer',
    color: 'tertiary',
    subtitle: 'Last 30 days',
  },
  {
    title: 'Maintenance Due',
    value: '3',
    icon: 'mdi-wrench',
    color: 'warning',
    subtitle: 'Upcoming services',
  },
])

const recentActivity = ref([
  {
    title: 'Honda Civic - Fuel Added',
    subtitle: '$45.20 • 2 hours ago',
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
  <v-container fluid class="pa-6">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-h3 font-weight-light mb-2">Dashboard</h1>
      <p class="text-body-1 text-medium-emphasis">Welcome back! Here's what's happening with your fleet.</p>
    </div>

    <!-- Stats Cards -->
    <v-row class="mb-6">
      <v-col v-for="card in statsCards" :key="card.title" cols="12" sm="6" md="3">
        <v-card class="stat-card" elevation="2" :color="card.color" variant="tonal">
          <v-card-text class="pb-3">
            <div class="d-flex justify-space-between align-start">
              <div>
                <p class="text-caption text-medium-emphasis mb-1">{{ card.title }}</p>
                <p class="text-h4 font-weight-bold">{{ card.value }}</p>
                <p class="text-caption text-medium-emphasis">{{ card.subtitle }}</p>
              </div>
              <v-icon :icon="card.icon" size="40" class="stat-icon" />
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <!-- Recent Activity -->
      <v-col cols="12" md="8">
        <v-card elevation="2" class="h-100">
          <v-card-title class="d-flex align-center">
            <v-icon icon="mdi-history" class="me-2" />
            Recent Activity
          </v-card-title>
          <v-card-text>
            <v-list class="py-0">
              <v-list-item v-for="(item, index) in recentActivity" :key="index" class="px-0">
                <template v-slot:prepend>
                  <v-avatar size="40" :color="item.color" variant="tonal">
                    <v-icon :icon="item.icon" />
                  </v-avatar>
                </template>
                <v-list-item-title>{{ item.title }}</v-list-item-title>
                <v-list-item-subtitle>{{ item.subtitle }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
          <v-card-actions>
            <v-btn variant="text" color="primary">
              View All Activity
              <v-icon end>mdi-arrow-right</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

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
    </v-row>

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
  </v-container>
</template>

<style scoped>
.stat-card {
  transition: transform 0.2s ease-in-out;
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

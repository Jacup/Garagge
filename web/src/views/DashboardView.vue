<script lang="ts" setup>
import { ref } from 'vue'
import Md3Card from '@/components/common/Md3Card.vue'
import Md3Button from '@/components/common/Md3Button.vue'

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
    <!-- Header with MD3 Expressive Typography -->
    <div class="mb-6">
      <h1 class="text-display-small mb-2">Dashboard</h1>
      <p class="text-body-large text-medium-emphasis">Welcome back! Here's what's happening with your fleet.</p>
    </div>

    <!-- Stats Cards using MD3 Expressive Cards -->
    <v-row class="mb-6">
      <v-col v-for="card in statsCards" :key="card.title" cols="12" sm="6" md="3">
        <Md3Card 
          variant="elevated"
          :color="card.color"
          class="stat-card"
          :expressive="true"
          :hover-elevation="true"
        >
          <div class="pa-4">
            <div class="d-flex justify-space-between align-start">
              <div>
                <p class="text-label-large text-medium-emphasis mb-1">{{ card.title }}</p>
                <p class="text-headline-large font-weight-bold">{{ card.value }}</p>
                <p class="text-body-medium text-medium-emphasis">{{ card.subtitle }}</p>
              </div>
              <v-icon :icon="card.icon" size="40" class="stat-icon" />
            </div>
          </div>
        </Md3Card>
      </v-col>
    </v-row>

    <v-row>
      <!-- Recent Activity using Enhanced Card -->
      <v-col cols="12" md="8">
        <Md3Card 
          variant="elevated" 
          class="h-100"
          :expressive="true"
        >
          <template #header>
            <div class="d-flex align-center">
              <v-icon icon="mdi-history" class="me-2" />
              <span class="text-headline-small">Recent Activity</span>
            </div>
          </template>

          <v-list class="py-0">
            <v-list-item v-for="(item, index) in recentActivity" :key="index" class="px-0">
              <template v-slot:prepend>
                <v-avatar size="40" :color="item.color" variant="tonal">
                  <v-icon :icon="item.icon" />
                </v-avatar>
              </template>
              <v-list-item-title class="text-title-medium">{{ item.title }}</v-list-item-title>
              <v-list-item-subtitle class="text-body-medium">{{ item.subtitle }}</v-list-item-subtitle>
            </v-list-item>
          </v-list>

          <template #actions>
            <Md3Button 
              variant="text" 
              color="primary"
              append-icon="mdi-arrow-right"
            >
              View All Activity
            </Md3Button>
          </template>
        </Md3Card>
      </v-col>

      <!-- Quick Actions & Chart Placeholder -->
      <v-col cols="12" md="4">
        <v-row>
          <!-- Quick Actions using Enhanced Card -->
          <v-col cols="12">
            <Md3Card variant="elevated" class="mb-4" :expressive="true">
              <template #header>
                <div class="d-flex align-center">
                  <v-icon icon="mdi-lightning-bolt" class="me-2" />
                  <span class="text-headline-small">Quick Actions</span>
                </div>
              </template>

              <v-row class="pa-4">
                <v-col v-for="action in quickActions" :key="action.title" cols="6">
                  <Md3Button 
                    :color="action.color" 
                    variant="tonal" 
                    block 
                    class="flex-column py-4" 
                    size="large"
                    style="height: 80px;"
                  >
                    <v-icon :icon="action.icon" size="24" class="mb-1" />
                    <span class="text-label-small">{{ action.title }}</span>
                  </Md3Button>
                </v-col>
              </v-row>
            </Md3Card>
          </v-col>

          <!-- Chart Placeholder using Enhanced Card -->
          <v-col cols="12">
            <Md3Card variant="elevated" class="chart-card" :expressive="true">
              <template #header>
                <div class="d-flex align-center">
                  <v-icon icon="mdi-chart-donut" class="me-2" />
                  <span class="text-headline-small">Fuel Efficiency</span>
                </div>
              </template>
              
              <div class="d-flex flex-column align-center justify-center pa-8" style="height: 200px">
                <v-icon icon="mdi-chart-donut-variant" size="80" color="primary" class="mb-4" />
                <p class="text-body-medium text-center text-medium-emphasis">
                  Chart will be displayed here<br />
                  <small class="text-body-small">Integration coming soon</small>
                </p>
              </div>
            </Md3Card>
          </v-col>
        </v-row>
      </v-col>
    </v-row>

    <!-- Additional Cards Row using Enhanced Cards -->
    <v-row class="mt-6">
      <v-col cols="12" md="4">
        <Md3Card 
          variant="elevated" 
          class="feature-card"
          :expressive="true"
          :hover-elevation="true"
        >
          <div class="text-center pa-8">
            <v-icon icon="mdi-calendar-check" size="64" color="success" class="mb-4" />
            <h3 class="text-headline-small mb-2">Maintenance Scheduler</h3>
            <p class="text-body-medium text-medium-emphasis mb-4">Keep track of upcoming maintenance and services</p>
            <Md3Button color="success" variant="tonal">
              Schedule Service
            </Md3Button>
          </div>
        </Md3Card>
      </v-col>

      <v-col cols="12" md="4">
        <Md3Card 
          variant="elevated" 
          class="feature-card"
          :expressive="true"
          :hover-elevation="true"
        >
          <div class="text-center pa-8">
            <v-icon icon="mdi-map-marker-path" size="64" color="info" class="mb-4" />
            <h3 class="text-headline-small mb-2">Trip Tracking</h3>
            <p class="text-body-medium text-medium-emphasis mb-4">Monitor your routes and optimize fuel consumption</p>
            <Md3Button color="info" variant="tonal">
              View Trips
            </Md3Button>
          </div>
        </Md3Card>
      </v-col>

      <v-col cols="12" md="4">
        <Md3Card 
          variant="elevated" 
          class="feature-card"
          :expressive="true"
          :hover-elevation="true"
        >
          <div class="text-center pa-8">
            <v-icon icon="mdi-finance" size="64" color="warning" class="mb-4" />
            <h3 class="text-headline-small mb-2">Cost Analysis</h3>
            <p class="text-body-medium text-medium-emphasis mb-4">Analyze expenses and optimize your fleet budget</p>
            <Md3Button color="warning" variant="tonal">
              View Reports
            </Md3Button>
          </div>
        </Md3Card>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
/* MD3 Expressive Dashboard Styles */

/* Use MD3 typography classes */
.text-display-small {
  font-family: var(--md-sys-typescale-display-small-font-family);
  font-size: var(--md-sys-typescale-display-small-font-size);
  font-weight: var(--md-sys-typescale-display-small-font-weight);
  line-height: var(--md-sys-typescale-display-small-line-height);
  letter-spacing: var(--md-sys-typescale-display-small-letter-spacing);
  color: rgb(var(--v-theme-on-surface));
}

.text-headline-large {
  font-family: var(--md-sys-typescale-headline-large-font-family);
  font-size: var(--md-sys-typescale-headline-large-font-size);
  font-weight: var(--md-sys-typescale-headline-large-font-weight);
  line-height: var(--md-sys-typescale-headline-large-line-height);
  letter-spacing: var(--md-sys-typescale-headline-large-letter-spacing);
  color: rgb(var(--v-theme-on-surface));
}

.text-headline-small {
  font-family: var(--md-sys-typescale-headline-small-font-family);
  font-size: var(--md-sys-typescale-headline-small-font-size);
  font-weight: var(--md-sys-typescale-headline-small-font-weight);
  line-height: var(--md-sys-typescale-headline-small-line-height);
  letter-spacing: var(--md-sys-typescale-headline-small-letter-spacing);
  color: rgb(var(--v-theme-on-surface));
}

.text-title-medium {
  font-family: var(--md-sys-typescale-title-medium-font-family);
  font-size: var(--md-sys-typescale-title-medium-font-size);
  font-weight: var(--md-sys-typescale-title-medium-font-weight);
  line-height: var(--md-sys-typescale-title-medium-line-height);
  letter-spacing: var(--md-sys-typescale-title-medium-letter-spacing);
  color: rgb(var(--v-theme-on-surface));
}

.text-body-large {
  font-family: var(--md-sys-typescale-body-large-font-family);
  font-size: var(--md-sys-typescale-body-large-font-size);
  font-weight: var(--md-sys-typescale-body-large-font-weight);
  line-height: var(--md-sys-typescale-body-large-line-height);
  letter-spacing: var(--md-sys-typescale-body-large-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

.text-body-medium {
  font-family: var(--md-sys-typescale-body-medium-font-family);
  font-size: var(--md-sys-typescale-body-medium-font-size);
  font-weight: var(--md-sys-typescale-body-medium-font-weight);
  line-height: var(--md-sys-typescale-body-medium-line-height);
  letter-spacing: var(--md-sys-typescale-body-medium-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

.text-body-small {
  font-family: var(--md-sys-typescale-body-small-font-family);
  font-size: var(--md-sys-typescale-body-small-font-size);
  font-weight: var(--md-sys-typescale-body-small-font-weight);
  line-height: var(--md-sys-typescale-body-small-line-height);
  letter-spacing: var(--md-sys-typescale-body-small-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

.text-label-large {
  font-family: var(--md-sys-typescale-label-large-font-family);
  font-size: var(--md-sys-typescale-label-large-font-size);
  font-weight: var(--md-sys-typescale-label-large-font-weight);
  line-height: var(--md-sys-typescale-label-large-line-height);
  letter-spacing: var(--md-sys-typescale-label-large-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

.text-label-small {
  font-family: var(--md-sys-typescale-label-small-font-family);
  font-size: var(--md-sys-typescale-label-small-font-size);
  font-weight: var(--md-sys-typescale-label-small-font-weight);
  line-height: var(--md-sys-typescale-label-small-line-height);
  letter-spacing: var(--md-sys-typescale-label-small-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

/* Stats card enhancements */
.stat-card {
  transition: 
    transform var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    box-shadow var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-emphasized);
}

.stat-icon {
  opacity: 0.7;
  transition: 
    opacity var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    transform var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard);
}

.stat-card:hover .stat-icon {
  opacity: 1;
  transform: scale(1.05);
}

/* Feature card enhancements */
.feature-card {
  transition:
    transform var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-emphasized),
    box-shadow var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-emphasized);
}

.feature-card:hover {
  transform: translateY(-2px);
}

/* Chart card specific styling */
.chart-card {
  min-height: 280px;
}

/* Responsive adjustments */
@media (max-width: 960px) {
  .text-display-small {
    font-size: var(--md-sys-typescale-headline-large-font-size);
  }
}

@media (max-width: 600px) {
  .text-display-small {
    font-size: var(--md-sys-typescale-headline-medium-font-size);
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .stat-card,
  .stat-icon,
  .feature-card {
    transition: none !important;
  }
  
  .feature-card:hover,
  .stat-card:hover .stat-icon {
    transform: none !important;
  }
}
</style>

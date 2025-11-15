<script setup lang="ts">
import ServiceRecordsTable from '@/components/vehicles/ServiceRecordsTable.vue'

interface Props {
  vehicleId: string
  selectedServiceRecords: string[]
  mockStats: {
    totalServiceCost: number
  }
}

defineProps<Props>()

const emit = defineEmits<{
  'update:selectedServiceRecords': [value: string[]]
}>()

// Utility function for date formatting
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
</script>

<template>
  <section class="service-section mb-6">
    <v-row class="equal-height-row">
      <v-col cols="12" md="4">
        <v-card class="service-stats-card card-background" height="400" variant="flat">
          <v-card-title>Service Statistics</v-card-title>
          <v-card-text>
            <div class="stats-grid">
              <div class="stat-item">
                <div class="text-body-2 text-medium-emphasis">Total Cost</div>
                <div class="text-body-2 font-weight-bold text-on-surface">{{ mockStats.totalServiceCost.toFixed(2) }} PLN</div>
              </div>
              <div class="stat-item">
                <div class="text-body-2 text-medium-emphasis">Last Service</div>
                <div class="text-body-2 font-weight-bold text-on-surface">{{ formatDate('2024-11-15') }}</div>
              </div>
              <div class="stat-item">
                <div class="text-body-2 text-medium-emphasis">Next Service</div>
                <div class="text-body-2 font-weight-bold text-on-surface">{{ formatDate('2025-03-15') }}</div>
              </div>
              <div class="stat-item">
                <div class="text-body-2 text-medium-emphasis">Average Cost</div>
                <div class="text-body-2 font-weight-bold text-on-surface">{{ (mockStats.totalServiceCost / 8).toFixed(2) }} PLN</div>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="8">
        <v-card class="service-timeline-card card-background" height="500" variant="flat">
          <template #title>Service Records</template>
          <template #append>
            <v-btn class="text-none" prepend-icon="mdi-plus" variant="flat" color="primary" disabled>Add</v-btn>
          </template>
          <v-card-text class="pa-4">
            <ServiceRecordsTable
              :vehicle-id="vehicleId"
              :selected="selectedServiceRecords"
              @update:selected="emit('update:selectedServiceRecords', $event)"
            />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </section>
</template>

<style scoped>
/* Equal height rows */
.equal-height-row {
  align-items: stretch;
}

.equal-height-row .v-col {
  display: flex;
}

.equal-height-row .v-card {
  flex: 1;
}

/* Card background */
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
</style>

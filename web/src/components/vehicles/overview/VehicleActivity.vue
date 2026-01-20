<script lang="ts" setup>
import type { VehicleActivityDto, ActivityType } from '@/api/generated/apiV1.schemas'
import { ActivityType as ActivityTypeEnum } from '@/api/generated/apiV1.schemas'
import { useFormatting } from '@/composables/useFormatting'

defineProps<{
  vehicleActivities: VehicleActivityDto[]
}>()

const { formatDateOnly, formatCurrencyString } = useFormatting()

const getActivityIcon = (type: ActivityType): string => {
  switch (type) {
    case ActivityTypeEnum.VehicleAdded:
      return 'mdi-car'
    case ActivityTypeEnum.VehicleUpdated:
      return 'mdi-car-info'
    case ActivityTypeEnum.Refuel:
      return 'mdi-gas-station'
    case ActivityTypeEnum.Charge:
      return 'mdi-ev-station'
    case ActivityTypeEnum.ServiceAdded:
      return 'mdi-wrench'
    default:
      return 'mdi-information'
  }
}

const getActivityColor = (type: ActivityType): string => {
  switch (type) {
    case ActivityTypeEnum.VehicleAdded:
      return 'success'
    case ActivityTypeEnum.VehicleUpdated:
      return 'success'
    case ActivityTypeEnum.Refuel:
      return 'tertiary'
    case ActivityTypeEnum.Charge:
      return 'tertiary'
    case ActivityTypeEnum.ServiceAdded:
      return 'secondary'
    default:
      return 'surface'
  }
}

const getActivityTitle = (activity: VehicleActivityDto): string => {
  switch (activity.type) {
    case ActivityTypeEnum.VehicleAdded:
      return `Vehicle Created`
    case ActivityTypeEnum.VehicleUpdated:
      return `Vehicle Updated`
    case ActivityTypeEnum.Refuel:
      return `Refuel`
    case ActivityTypeEnum.Charge:
      return `Charge`
    case ActivityTypeEnum.ServiceAdded: {
      return `Service Added`
    }
    default:
      return 'Unknown Activity'
  }
}
</script>

<template>
  <v-card class="card-background" variant="flat" rounded="md-16px">
    <v-card-title>Recent Activity</v-card-title>
    <v-card-text>
      <v-timeline direction="vertical" side="end">
        <v-timeline-item v-for="activity in vehicleActivities" :key="activity.date">
          <template v-slot:icon>
            <v-avatar size="40" :color="getActivityColor(activity.type)" variant="flat">
              <v-icon :icon="getActivityIcon(activity.type)" />
            </v-avatar>
          </template>

          <div class="text-body-2 font-weight-medium">{{ getActivityTitle(activity) }}</div>

          <div class="text-caption text-medium-emphasis">
            {{ formatDateOnly(activity.date) }}
          </div>

          <div v-if="activity.activityDetails.length > 0" class="mt-2 chips-container">
            <v-chip
              v-for="detail in activity.activityDetails"
              :key="detail.name"
              size="small"
              density="comfortable"
              variant="outlined"
              class="suggestion-chip"
            >
              {{ detail.name == "Cost" ? formatCurrencyString(detail.value) : detail.value }}
            </v-chip>
          </div>
        </v-timeline-item>
      </v-timeline>
    </v-card-text>
  </v-card>
</template>

<style scoped>
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.suggestion-chip {
  border-radius: 8px !important;
  border: 1px solid rgb(var(--v-theme-outline-variant)) !important;
  font-weight: 500 !important;
  line-height: 20px !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.chips-container {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
</style>

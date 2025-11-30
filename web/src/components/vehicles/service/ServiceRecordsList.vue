<script lang="ts" setup>
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: ServiceRecordDto[]
}

defineProps<Props>()

const emit = defineEmits<{
  select: [record: ServiceRecordDto]
}>()

const handleRowClick = (item: ServiceRecordDto) => {
  emit('select', item)
}

const getIconForServiceType = (type: string | undefined): string => {
  switch (type) {
    case 'General':
      return 'mdi-cog'
    case 'OilChange':
      return 'mdi-oil'
    case 'Brakes':
      return 'mdi-car-brake-abs'
    case 'Tires':
      return 'mdi-tire'
    case 'Engine':
      return 'mdi-engine'
    case 'Transmission':
      return 'mdi-car-shift-pattern'
    case 'Suspension':
      return 'mdi-car-esp'
    case 'Electrical':
      return 'mdi-flash'
    case 'Bodywork':
      return 'mdi-hammer-wrench'
    case 'Interior':
      return 'mdi-car-seat'
    case 'Inspection':
      return 'mdi-clipboard-check-outline'
    case 'Emergency':
      return 'mdi-alert-decagram'
    case 'Other':
      return 'mdi-help-circle-outline'
    default:
      return 'mdi-tools'
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}
</script>

<template>
  <v-list lines="two">
    <v-list-item v-for="record in items" :key="record.id" :title="record.title" class="list-item" @click="handleRowClick(record)">
      <template #prepend>
        <v-badge color="info" :model-value="record.serviceItems && record.serviceItems.length > 0" :content="record.serviceItems.length">
          <v-avatar color="primary-container">
            <v-icon :icon="getIconForServiceType(record.type)" color="on-primary-container"></v-icon>
          </v-avatar>
        </v-badge>
      </template>
      <template v-slot:subtitle>
        {{ formatDate(record.serviceDate) }}
        <span v-if="record.mileage"> • {{ record.mileage }} km</span>
      </template>
      <template v-slot:append>
        <div v-if="record.totalCost" class="trailing-supporting-text">
          {{ new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(record.totalCost) }}
        </div>
      </template>
    </v-list-item>
  </v-list>
</template>

<style scoped>
.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
  border-radius: 2px !important;
}

.list-item:first-child {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}

.list-item:last-child {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
  margin-bottom: 0 !important;
}

.trailing-supporting-text {
  color: rgb(var(--v-theme-on-surface-variant));
  font-size: 11px;
  font-weight: 500;
  line-height: 16px;
  letter-spacing: 0.5px;
}
</style>

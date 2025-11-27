<script setup lang="ts">
import { computed } from 'vue'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: ServiceRecordDto[]
  itemsLength: number
  loading: boolean
  page: number
  itemsPerPage: number
  selectedId?: string | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  select: [record: ServiceRecordDto]
  'update:page': [value: number]
  'update:items-per-page': [value: number]
  'update:sort-by': [value: { key: string; order: string }[]]
}>()

const selectedIds = computed(() => props.selectedId ? [props.selectedId] : [])

// Table configuration
const serviceHeaders = [
  {
    title: 'Title',
    key: 'title',
    sortable: true,
    headerProps: { class: 'service-table-header service-table-header-first' },
  },
  { title: 'Type', key: 'type', sortable: false, headerProps: { class: 'service-table-header' } },
  {
    title: 'Service Date',
    key: 'serviceDate',
    sortable: true,
    headerProps: { class: 'service-table-header' },
  },
  {
    title: 'Mileage',
    key: 'mileage',
    sortable: true,
    headerProps: { class: 'service-table-header' },
  },
  {
    title: 'Total Cost',
    key: 'totalCost',
    sortable: true,
    headerProps: { class: 'service-table-header service-table-header-last' },
  },
]

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

const formatCurrency = (value: number | undefined) => {
  if (!value) return 'N/A'
  return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(value)
}

const handleRowClick = (_event: Event, { item }: { item: ServiceRecordDto }) => {
  emit('select', item)
}
</script>

<template>
<v-data-table-server
    :model-value="selectedIds"
    item-value="id"
    :headers="serviceHeaders"
    :items="items"
    :items-length="itemsLength"
    :loading="loading"
    :page="page"
    :items-per-page="itemsPerPage"
    select-strategy="single"
    hover
    @update:page="emit('update:page', $event)"
    @update:items-per-page="emit('update:items-per-page', $event)"
    @update:sort-by="emit('update:sort-by', $event)"
    @click:row="handleRowClick"
    density="comfortable"
    class="service-records-table cursor-pointer"
    fixed-header
    bg-color="transparent"
    :row-props="{ class: 'service-table-row' }"
  >
    <template #[`item.serviceDate`]="{ item }">
      {{ formatDate(item.serviceDate) }}
    </template>
    <template #[`item.mileage`]="{ item }">
      {{ item.mileage ? item.mileage.toLocaleString('pl-PL') + ' km' : 'N/A' }}
    </template>
    <template #[`item.totalCost`]="{ item }">
      {{ formatCurrency(item.totalCost) }}
    </template>
  </v-data-table-server>
</template>

<style scoped>
.service-records-table {
}

.service-records-table :deep(.service-table-header) {
  background-color: rgba(var(--v-theme-primary), 0.2) !important;
}

.service-records-table :deep(.service-table-row) {
  background-color: rgba(var(--v-theme-primary), 0.08);
}

.service-records-table :deep(tbody tr.v-data-table__tr--selected) {
  background: rgba(var(--v-theme-primary), 0.16);
}

.service-records-table :deep(.v-data-table-footer) {
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
  border-top: thin solid rgba(var(--v-border-color), 0.1);
}
</style>

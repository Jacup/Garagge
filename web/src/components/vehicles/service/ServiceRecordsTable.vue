<script setup lang="ts">
import { computed } from 'vue'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: ServiceRecordDto[]
  totalCount: number
  loading: boolean
  page: number
  itemsPerPage: number
  selectedId?: string | null
  selectedIds?: string[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  select: [record: ServiceRecordDto]
  'update:page': [value: number]
  'update:items-per-page': [value: number]
  'update:sort-by': [value: { key: string; order: string }[]]
  'update:selectedIds': [ids: string[]]
  'delete-selected': []
}>()

const tableSelectedIds = computed({
  get: () => props.selectedIds ?? [],
  set: (val: string[]) => emit('update:selectedIds', val),
})

const hasSelection = computed(() => tableSelectedIds.value.length > 0)
const selectedCount = computed(() => tableSelectedIds.value.length)

const headers = [
  {
    title: '',
    key: 'data-table-select',
    sortable: false,
    width: '48px',
  },
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

const formatDate = (dateString: string) =>
  new Date(dateString).toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })

const formatCurrency = (value: number | undefined) => {
  if (!value) return 'N/A'
  return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(value)
}

const handleRowClick = (_event: Event, { item }: { item: ServiceRecordDto }) => {
  if (!hasSelection.value) {
    emit('select', item)
  }
}
</script>

<template>
  <div>
    <!-- Context bar -->
    <v-fade-transition mode="out-in" duration="200">
      <div v-if="hasSelection" key="context-bar" class="context-bar d-flex align-center rounded-pill px-2 mb-3">
        <v-tooltip text="Clear selection" location="bottom" open-delay="200">
          <template #activator="{ props: tooltipProps }">
            <v-btn v-bind="tooltipProps" icon="mdi-close" variant="text" density="comfortable" @click="emit('update:selectedIds', [])" />
          </template>
        </v-tooltip>

        <span class="text-subtitle-2 font-weight-medium ml-2"> {{ selectedCount }} selected </span>

        <v-spacer />

        <v-tooltip text="Delete selected" location="bottom" open-delay="200">
          <template #activator="{ props: tooltipProps }">
            <v-btn
              v-bind="tooltipProps"
              icon="mdi-delete"
              variant="text"
              color="error"
              density="comfortable"
              @click="emit('delete-selected')"
            />
          </template>
        </v-tooltip>
      </div>
      <div v-else key="standard-bar" class="d-flex justify-end align-center w-100 py-1">
        <v-btn icon="mdi-filter-variant" variant="text" disabled />
      </div>
    </v-fade-transition>

    <!-- Table -->
    <v-data-table-server
      v-model="tableSelectedIds"
      item-value="id"
      :headers="headers"
      :items="items"
      :items-length="totalCount"
      :loading="loading"
      :page="page"
      :items-per-page="itemsPerPage"
      show-select
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
  </div>
</template>

<style scoped>
.context-bar {
  background-color: rgb(var(--v-theme-secondary-container));
  color: rgb(var(--v-theme-on-secondary-container));
  min-height: 48px;
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

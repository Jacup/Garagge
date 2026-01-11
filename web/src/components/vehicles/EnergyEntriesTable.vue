<script setup lang="ts">
import { computed } from 'vue'

import { useFormatting } from '@/composables/useFormatting'
import { formattingUtils } from '@/utils/formattingUtils'
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: EnergyEntryDto[]
  totalCount: number
  loading: boolean
  page: number
  itemsPerPage: number
  selectedIds?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  items: () => [],
  selectedIds: () => [],
})

const emit = defineEmits<{
  'update:selectedIds': [value: string[]]
  'update:page': [value: number]
  'update:itemsPerPage': [value: number]
  edit: [item: EnergyEntryDto]
  delete: [item: EnergyEntryDto]
}>()

const selectedItems = computed({
  get: () => props.selectedIds,
  set: (value) => emit('update:selectedIds', value),
})

const { formatCurrency, formatDate, formatMileage } = useFormatting()
const { formatEnergyUnit } = formattingUtils()

const headers = [
  {
    title: 'Date',
    key: 'date',
    sortable: false,
    align: 'start' as const,
  },
  {
    title: 'Mileage',
    key: 'mileage',
    sortable: false,
    align: 'end' as const,
  },
  {
    title: 'Type',
    key: 'type',
    sortable: false,
    align: 'start' as const,
  },
  {
    title: 'Volume',
    key: 'volume',
    sortable: false,
    align: 'end' as const,
  },
  {
    title: 'Cost',
    key: 'cost',
    sortable: false,
    align: 'end' as const,
  },
  {
    title: '',
    key: 'actions',
    sortable: false,
    align: 'end' as const,
    width: '100px',
  },
]
</script>

<template>
  <v-data-table-server
    v-model="selectedItems"
    :headers="headers"
    :items="items"
    :items-length="totalCount"
    :loading="loading"
    :page="page"
    :items-per-page="itemsPerPage"
    show-select
    item-value="id"
    density="comfortable"
    class="energy-table"
    :row-props="{ class: 'energy-table-row' }"
    @update:page="emit('update:page', $event)"
    @update:items-per-page="emit('update:itemsPerPage', $event)"
  >
    <template v-slot:[`item.date`]="{ item }"> {{ formatDate(item.date) }} </template>

    <template v-slot:[`item.mileage`]="{ item }"> {{ formatMileage(item.mileage) }} </template>

    <template v-slot:[`item.volume`]="{ item }"> {{ item.volume }} {{ formatEnergyUnit(item.energyUnit) }} </template>

    <template v-slot:[`item.cost`]="{ item }"> {{ formatCurrency(item.cost) }} </template>

    <template v-slot:[`item.actions`]="{ item }">
      <div class="d-flex justify-end action-buttons">
        <v-tooltip text="Edit" location="bottom" open-delay="200" close-delay="200">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-pencil" color="secondary" variant="text" size="x-small" @click.stop="$emit('edit', item)" />
          </template>
        </v-tooltip>
        <v-tooltip text="Delete" location="bottom" open-delay="200" close-delay="200">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-delete" color="error" variant="text" size="x-small" @click.stop="$emit('delete', item)" />
          </template>
        </v-tooltip>
      </div>
    </template>

    <template v-slot:no-data>
      <div class="py-6 text-center text-medium-emphasis">No fuel data</div>
    </template>
  </v-data-table-server>
</template>

<style scoped lang="scss">
.energy-table {
  background-color: transparent !important;

  :deep(table) {
    border-collapse: separate !important;
    border-spacing: 0;
  }

  :deep(thead),
  :deep(tbody tr),
  :deep(tbody tr:hover) {
    background: transparent !important;
  }

  :deep(th),
  :deep(td) {
    border-bottom: none !important;
  }

  :deep(th) {
    background-color: rgba(var(--v-theme-primary), 0.2) !important;
    color: rgb(var(--v-theme-on-surface));
    text-transform: uppercase;
    white-space: nowrap;
  }

  :deep(td) {
    transition: background-color 0.2s ease-in-out;
  }

  :deep(tbody tr:hover td) {
    background-color: rgba(var(--v-theme-primary), 0.08) !important;
  }

  :deep(th:first-child),
  :deep(td:first-child) {
    border-top-left-radius: 8px;
    border-bottom-left-radius: 8px;
  }

  :deep(th:last-child),
  :deep(td:last-child) {
    border-top-right-radius: 8px;
    border-bottom-right-radius: 8px;
  }
}

.action-buttons {
  opacity: 0;
  visibility: hidden;
  transition:
    opacity 0.2s ease-in-out,
    visibility 0.2s;
}

.energy-table :deep(tbody tr:hover) .action-buttons {
  opacity: 1;
  visibility: visible;
}
</style>

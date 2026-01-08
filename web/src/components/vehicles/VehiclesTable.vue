<script lang="ts" setup>
import { computed } from 'vue'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: VehicleDto[]
  loading?: boolean
  sortBy?: { key: string; order: 'asc' | 'desc' }[]
  modelValue?: string[]
}

interface Emits {
  (e: 'edit', id: string): void
  (e: 'delete', id: string): void
  (e: 'view', id: string): void
  (e: 'update:sort-by', sortBy: { key: string; order: 'asc' | 'desc' }[]): void
  (e: 'update:modelValue', value: string[]): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const selectedIds = computed({
  get: () => props.modelValue || [],
  set: (value) => emit('update:modelValue', value),
})

function handleRowClick(_: Event, { item }: { item: VehicleDto }) {
  if (item.id) {
    emit('view', item.id)
  }
}

const headers = [
  { title: 'Brand', key: 'brand', sortable: true },
  { title: 'Model', key: 'model', sortable: true },
  { title: 'Engine Type', key: 'engineType', sortable: true },
  { title: 'Year', key: 'manufacturedYear', sortable: true },
  { title: 'Type', key: 'type', sortable: true },
  { title: 'VIN', key: 'vin', sortable: true },
  { title: '', key: 'actions', sortable: false, align: 'end' as const },
]
</script>

<template>
  <v-data-table
    v-model="selectedIds"
    show-select
    :headers="headers"
    :items="items"
    :loading="loading"
    item-value="id"
    :sort-by="sortBy"
    hide-default-footer
    density="comfortable"
    class="vehicle-table rounded-lg elevation-0"
    color="primary"
    @update:sort-by="$emit('update:sort-by', $event)"
    @click:row="handleRowClick"
  >
    <template v-slot:[`item.actions`]="{ item }">
      <div class="d-flex gap-1 justify-end action-buttons">
        <v-tooltip text="Edit Vehicle" location="bottom" open-delay="200" close-delay="200">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-pencil" color="secondary" variant="text" size="small" @click.stop="$emit('edit', item.id!)" />
          </template>
        </v-tooltip>
        <v-tooltip text="Delete Vehicle" location="bottom" open-delay="200" close-delay="200">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-delete" color="error" variant="text" size="small" @click.stop="$emit('delete', item.id!)" />
          </template>
        </v-tooltip>
      </div>
    </template>

    <template v-slot:[`item.brand`]="{ item }">
      <span class="font-weight-medium">{{ item.brand }}</span>
    </template>

    <template v-slot:[`item.manufacturedYear`]="{ item }">
      <span class="text-medium-emphasis">{{ item.manufacturedYear || '—' }}</span>
    </template>

    <template v-slot:[`item.type`]="{ item }">
      <v-chip v-if="item.type" size="small" variant="tonal" density="compact" class="text-capitalize">
        {{ item.type }}
      </v-chip>
      <span v-else class="text-disabled">—</span>
    </template>

    <template v-slot:[`item.vin`]="{ item }">
      <span class="text-caption text-medium-emphasis font-monospace">{{ item.vin || '—' }}</span>
    </template>

    <template v-slot:[`item.engineType`]="{ item }">
      {{ item.engineType || '—' }}
    </template>
  </v-data-table>
</template>

<style scoped lang="scss">
.vehicle-table {
  :deep(table) {
    border-collapse: separate;
    border-spacing: 0;
  }

  :deep(td) {
    border-bottom: none !important;
    transition: background-color 0.2s ease-in-out;
  }

  :deep(thead) {
    background-color: transparent !important;
  }

  :deep(th) {
    background-color: rgba(var(--v-theme-primary), 0.14) !important;
    font-weight: 600 !important;
    white-space: nowrap;
    border-bottom: none !important;
  }

  :deep(thead th:first-child) {
    border-top-left-radius: 12px;
  }

  :deep(thead th:last-child) {
    border-top-right-radius: 12px;
  }

  :deep(tbody tr:hover) {
    background-color: transparent !important;
  }

  :deep(tbody tr:hover td) {
    cursor: pointer;
    background-color: rgba(var(--v-theme-primary), 0.08) !important;
  }

  :deep(tbody td:first-child) {
    border-top-left-radius: 12px;
    border-bottom-left-radius: 12px;
  }

  :deep(tbody td:last-child) {
    border-top-right-radius: 12px;
    border-bottom-right-radius: 12px;
  }
}

.action-buttons {
  opacity: 0;
  visibility: hidden;
  transition:
    opacity 0.2s ease-in-out,
    visibility 0.2s;
}

:deep(.v-data-table__tr:hover) .action-buttons {
  opacity: 1;
  visibility: visible;
}

.font-monospace {
  font-family: monospace;
}
</style>

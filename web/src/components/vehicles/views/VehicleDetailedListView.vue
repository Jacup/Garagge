<script lang="ts" setup>
import type { VehicleDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: VehicleDto[]
  loading?: boolean
  page: number
  itemsPerPage: number
  totalItems: number
  sortBy: { key: string; order: 'asc' | 'desc' }[]
}

interface Emits {
  (e: 'edit', id: string): void
  (e: 'delete', id: string): void
  (e: 'view', id: string): void
  (e: 'update:options', options: { page: number; itemsPerPage: number }): void
  (e: 'update:sort-by', sortBy: { key: string; order: 'asc' | 'desc' }[]): void
}

defineProps<Props>()
defineEmits<Emits>()

const headers = [
  { title: 'Brand', key: 'brand', sortable: true },
  { title: 'Model', key: 'model', sortable: true },
  { title: 'Engine Type', key: 'engineType', sortable: true },
  { title: 'Year', key: 'manufacturedYear', sortable: true },
  { title: 'Type', key: 'type', sortable: true },
  { title: 'VIN', key: 'vin', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const },
]
</script>

<template>
  <v-data-table-server
    :items-per-page="itemsPerPage"
    :headers="headers"
    :items="items"
    :items-length="totalItems"
    :loading="loading"
    item-value="id"
    :page="page"
    :sort-by="sortBy"
    show-select
    @update:options="$emit('update:options', $event)"
    @update:sort-by="$emit('update:sort-by', $event)"
  >
    <template v-slot:[`item.actions`]="{ item }">
      <div class="d-flex gap-1 justify-end">
        <v-tooltip text="View Details">
          <template #activator="{ props }">
            <v-btn
              v-bind="props"
              @click="$emit('view', item.id!)"
              variant="tonal"
              prepend-icon="mdi-eye"
              color="primary"
              text="View"
              size="small"
              class="action-btn-small"
            />
          </template>
        </v-tooltip>
        <v-tooltip text="Edit Vehicle">
          <template #activator="{ props }">
            <v-btn
              v-bind="props"
              @click="$emit('edit', item.id!)"
              variant="tonal"
              prepend-icon="mdi-pencil"
              color="info"
              text="Edit"
              size="small"
              class="action-btn-small"
            />
          </template>
        </v-tooltip>
        <v-tooltip text="Delete Vehicle">
          <template #activator="{ props }">
            <v-btn
              v-bind="props"
              @click="$emit('delete', item.id!)"
              variant="tonal"
              prepend-icon="mdi-delete"
              color="error"
              text="Delete"
              size="small"
              class="action-btn-small"
            />
          </template>
        </v-tooltip>
      </div>
    </template>

    <template v-slot:[`item.manufacturedYear`]="{ item }">
      {{ item.manufacturedYear || 'N/A' }}
    </template>

    <template v-slot:[`item.type`]="{ item }">
      <v-chip v-if="item.type" size="small" variant="tonal">{{ item.type }}</v-chip>
      <span v-else class="text-medium-emphasis">N/A</span>
    </template>

    <template v-slot:[`item.vin`]="{ item }">
      {{ item.vin || 'N/A' }}
    </template>

    <template v-slot:[`item.engineType`]="{ item }">
      {{ item.engineType || 'N/A' }}
    </template>
  </v-data-table-server>
</template>

<style scoped>
.action-btn-small {
  text-transform: none;
  font-weight: 500;
  height: 32px;
  min-width: 70px;
  font-size: 0.75rem;
}
</style>

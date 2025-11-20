<script setup lang="ts">
import { ref, computed } from 'vue'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

interface Props {
  vehicleId: string
  selected?: string[]
  items: ServiceRecordDto[]
  itemsLength: number
  loading: boolean
  page: number
  itemsPerPage: number
  error: string | null
}

const props = withDefaults(defineProps<Props>(), {
  selected: () => []
})

// Define emits
const emit = defineEmits<{
  'update:selected': [value: string[]]
  'update:page': [value: number]
  'update:items-per-page': [value: number]
  'update:sort-by': [value: { key: string; order: string }[]]
  'delete': [id: string]
}>()

// Selection state - computed to sync with parent
const selectedItems = computed({
  get: () => props.selected,
  set: (value: string[]) => emit('update:selected', value)
})

// Dialog state
const deleteDialog = ref(false)
const selectedRecord = ref<ServiceRecordDto | null>(null)

// Table configuration
const serviceHeaders = [
  { title: 'Title', key: 'title', value: 'title', sortable: true },
  { title: 'Type', key: 'type', value: 'type', sortable: true },
  { title: 'Service Date', key: 'serviceDate', value: 'serviceDate', sortable: true },
  { title: 'Mileage', key: 'mileage', value: 'mileage', sortable: true },
  { title: 'Total Cost', key: 'totalCost', value: 'totalCost', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const },
]

// Utility functions
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function openDeleteDialog(id: string) {
  selectedRecord.value = props.items.find((record) => record.id === id) || null
  deleteDialog.value = true
}

function closeDeleteDialog() {
  deleteDialog.value = false
  selectedRecord.value = null
}

function confirmDelete() {
  if (selectedRecord.value?.id) {
    emit('delete', selectedRecord.value.id)
    deleteDialog.value = false
    selectedRecord.value = null
  }
}

</script>

<template>
  <div>
    <v-alert
      v-if="error"
      type="error"
      density="compact"
      class="mb-4"
      closable
    >
      {{ error }}
    </v-alert>

    <v-data-table-server
      v-model="selectedItems"
      :headers="serviceHeaders"
      :items="items"
      :items-length="itemsLength"
      :loading="loading"
      :page="page"
      :items-per-page="itemsPerPage"
      @update:page="(value) => emit('update:page', value)"
      @update:items-per-page="(value) => emit('update:items-per-page', value)"
      @update:sort-by="(value) => emit('update:sort-by', value)"
      show-select
      density="compact"
      fixed-header
      :height="407"
    >
      <template v-slot:[`item.title`]="{ item }">
        <div class="font-weight-medium">{{ item.title }}</div>
      </template>
      <template v-slot:[`item.serviceDate`]="{ item }">
        {{ formatDate(item.serviceDate) }}
      </template>
      <template v-slot:[`item.mileage`]="{ item }">
        {{ item.mileage ? item.mileage.toLocaleString() + ' km' : 'N/A' }}
      </template>
      <template v-slot:[`item.totalCost`]="{ item }">
        {{ item.totalCost ? item.totalCost.toFixed(2) + ' PLN' : 'N/A' }}
      </template>
      <template v-slot:[`item.actions`]="{ item }">
        <v-btn icon="mdi-eye" variant="text" size="x-small" />
        <v-btn icon="mdi-delete" variant="text" size="x-small" color="error" @click="openDeleteDialog(item.id)" />
      </template>
    </v-data-table-server>

    <DeleteDialog :is-open="deleteDialog" item-to-delete="service record" :on-confirm="confirmDelete" :on-cancel="closeDeleteDialog" />
  </div>
</template>

<style scoped>
/* Make table background transparent */
:deep(.v-table) {
  background: transparent !important;
}

/* Table header styling */
:deep(.v-data-table__th) {
  background-color: rgba(var(--v-theme-primary), 0.12) !important;
  position: sticky !important;
  top: 0 !important;
  z-index: 2 !important;
}

/* Make header opaque for proper sticky behavior and rounded corners */
:deep(.v-data-table__thead) {
  background-color: rgba(var(--v-theme-surface), 1) !important;
}

/* Ensure only first and last header cells have proper radius */
:deep(.v-data-table__th:first-child) {
  border-top-left-radius: 8px !important;
}

:deep(.v-data-table__th:last-child) {
  border-top-right-radius: 8px !important;
}

</style>

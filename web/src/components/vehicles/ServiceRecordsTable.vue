<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { getServiceRecords } from '@/api/generated/service-records/service-records'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

interface Props {
  vehicleId: string
  selected?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  selected: () => []
})

// Define emits
const emit = defineEmits<{
  'update:selected': [value: string[]]
  'record-changed': []
}>()

const { getApiVehiclesVehicleIdServiceRecords, deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId } = getServiceRecords()

// Service records data
const serviceRecords = ref<ServiceRecordDto[]>([])
const serviceRecordsLoading = ref(false)
const serviceRecordsPage = ref(1)
const serviceRecordsPageSize = ref(10)
const serviceRecordsTotal = ref(0)
const error = ref<string | null>(null)

// Sorting state
const sortBy = ref<string | undefined>(undefined)
const sortDescending = ref(false)

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

// Load service records from API
async function loadServiceRecords() {
  if (!props.vehicleId) return

  serviceRecordsLoading.value = true
  error.value = null
  try {
    const response = await getApiVehiclesVehicleIdServiceRecords(props.vehicleId, {
      page: serviceRecordsPage.value,
      pageSize: serviceRecordsPageSize.value,
      sortBy: sortBy.value,
      sortDescending: sortDescending.value,
    })
    serviceRecords.value = response.data.items ?? []
    serviceRecordsTotal.value = response.data.totalCount ?? 0
  } catch (err) {
    console.error('Failed to load service records:', err)
    error.value = 'Failed to load service records'
    serviceRecords.value = []
    serviceRecordsTotal.value = 0
  } finally {
    serviceRecordsLoading.value = false
  }
}

async function remove(id: string) {
  try {
    await deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, id)
    loadServiceRecords()
    emit('record-changed')
  } catch (err) {
    console.error('Failed to delete service record:', err)
  }
}

// Bulk delete function
async function removeMultiple(ids: string[]) {
  try {
    await Promise.all(ids.map(id => deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, id)))
    // Clear selection after successful delete
    selectedItems.value = []
    loadServiceRecords()
    emit('record-changed')
  } catch (err) {
    console.error('Failed to delete service records:', err)
  }
}

function openDeleteDialog(id: string) {
  selectedRecord.value = serviceRecords.value.find((record) => record.id === id) || null
  deleteDialog.value = true
}

function closeDeleteDialog() {
  deleteDialog.value = false
  selectedRecord.value = null
}

async function confirmDelete() {
  if (selectedRecord.value?.id) {
    await remove(selectedRecord.value.id)
    deleteDialog.value = false
    selectedRecord.value = null
  }
}

function handlePageUpdate(page: number) {
  serviceRecordsPage.value = page
  loadServiceRecords()
}

function handlePageSizeUpdate(pageSize: number) {
  serviceRecordsPageSize.value = pageSize
  loadServiceRecords()
}

function handleSortUpdate(sortOptions: { key: string; order: string }[]) {
  if (sortOptions && sortOptions.length > 0) {
    const sort = sortOptions[0]
    sortBy.value = sort.key
    sortDescending.value = sort.order === 'desc'
  } else {
    sortBy.value = undefined
    sortDescending.value = false
  }
  loadServiceRecords()
}

onMounted(() => {
  loadServiceRecords()
})

// Expose loadServiceRecords function to parent components
defineExpose({
  loadServiceRecords,
  removeMultiple
})

</script>

<template>
  <div>
    <v-alert
      v-if="error"
      type="error"
      density="compact"
      class="mb-4"
      closable
      @click:close="error = null"
    >
      {{ error }}
    </v-alert>

    <v-data-table-server
      v-model:items-per-page="serviceRecordsPageSize"
      v-model="selectedItems"
      :headers="serviceHeaders"
      :items="serviceRecords"
      :items-length="serviceRecordsTotal"
      :loading="serviceRecordsLoading"
      :page="serviceRecordsPage"
      @update:page="handlePageUpdate"
      @update:items-per-page="handlePageSizeUpdate"
      @update:sort-by="handleSortUpdate"
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

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { getServiceRecords } from '@/api/generated/service-records/service-records'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import ServiceRecordsTable from '@/components/vehicles/ServiceRecordsTable.vue'

const { isMobile } = useResponsiveLayout()

interface Props {
  vehicleId: string
  selectedServiceRecords: string[]
  mockStats: {
    totalServiceCost: number
  }
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:selectedServiceRecords': [value: string[]]
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
const hasMoreRecords = ref(true)

// Sorting state
const sortBy = ref<string | undefined>(undefined)
const sortDescending = ref(false)

// Format date helper
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
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

    // For mobile infinite scroll - append items
    if (isMobile.value && serviceRecordsPage.value > 1) {
      serviceRecords.value = [...serviceRecords.value, ...(response.data.items ?? [])]
    } else {
      serviceRecords.value = response.data.items ?? []
    }

    serviceRecordsTotal.value = response.data.totalCount ?? 0
    hasMoreRecords.value = serviceRecords.value.length < serviceRecordsTotal.value
  } catch (err) {
    console.error('Failed to load service records:', err)
    error.value = 'Failed to load service records'
    serviceRecords.value = []
    serviceRecordsTotal.value = 0
    hasMoreRecords.value = false
  } finally {
    serviceRecordsLoading.value = false
  }
}

// Load more for infinite scroll
async function loadMore({ done }: { done: (status: 'ok' | 'empty' | 'error') => void }) {
  if (!hasMoreRecords.value) {
    done('empty')
    return
  }

  serviceRecordsPage.value++
  await loadServiceRecords()

  done(hasMoreRecords.value ? 'ok' : 'empty')
}

async function deleteServiceRecord(id: string) {
  try {
    await deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, id)
    loadServiceRecords()
    emit('record-changed')
  } catch (err) {
    console.error('Failed to delete service record:', err)
  }
}

// Bulk delete function
async function deleteMultipleServiceRecords(ids: string[]) {
  try {
    await Promise.all(ids.map((id) => deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, id)))
    // Clear selection after successful delete
    emit('update:selectedServiceRecords', [])
    loadServiceRecords()
    emit('record-changed')
  } catch (err) {
    console.error('Failed to delete service records:', err)
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

// Expose functions for parent components
defineExpose({
  loadServiceRecords,
  deleteMultipleServiceRecords,
})
</script>

<template>
  <!-- Desktop/Tablet: Grid + Table -->
  <template v-if="!isMobile">
    <v-row>
      <v-col cols="6" sm="6" md="3">
        <v-card class="summary-card" height="120" color="primary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
            <div class="text-caption text-on-primary-container">Title</div>
            <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="6" sm="6" md="3">
        <v-card class="summary-card" height="120" color="primary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
            <div class="text-caption text-on-primary-container">Title</div>
            <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="6" sm="6" md="3">
        <v-card class="summary-card" height="120" color="primary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
            <div class="text-caption text-on-primary-container">Title</div>
            <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="6" sm="6" md="3">
        <v-card class="summary-card" height="120" color="primary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
            <div class="text-caption text-on-primary-container">Title</div>
            <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card class="service-records" variant="flat">
          <ServiceRecordsTable
            :vehicle-id="vehicleId"
            :selected="selectedServiceRecords"
            :items="serviceRecords"
            :items-length="serviceRecordsTotal"
            :loading="serviceRecordsLoading"
            :page="serviceRecordsPage"
            :items-per-page="serviceRecordsPageSize"
            :error="error"
            @update:selected="emit('update:selectedServiceRecords', $event)"
            @update:page="handlePageUpdate"
            @update:items-per-page="handlePageSizeUpdate"
            @update:sort-by="handleSortUpdate"
            @delete="deleteServiceRecord"
          />
        </v-card>
      </v-col>
    </v-row>
  </template>

  <!-- Mobile -->
  <template v-else>
    <v-alert v-if="error" type="error" density="compact" class="mb-4" closable>
      {{ error }}
    </v-alert>

    <v-infinite-scroll class="mobile-infinite-scroll" :onLoad="loadMore" :items="serviceRecords">
      <!-- Summary Cards -->
      <!-- <v-row class="mobile-summary-cards mb-4">
        <v-col cols="6">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="6">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="6">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="6">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row> -->

      <!-- Service Records List -->
      <v-list lines="two">
        <v-list-item
          v-for="record in serviceRecords"
          :key="record.id"
          :title="record.title"
          class="list-item"
        >
          <template v-slot:subtitle>
            <v-chip-group column>
              <v-chip class="subtitle-chip" variant="outlined" density="compact" size="small" disabled>{{ formatDate(record.serviceDate) }}</v-chip>
              <v-chip class="subtitle-chip" variant="outlined" density="compact" size="small" disabled>{{ record.mileage }} km</v-chip>
            </v-chip-group>
          </template>
          <template v-slot:append>
            <v-btn color="grey-lighten-1" icon="mdi-information" variant="text"></v-btn>
          </template>
        </v-list-item>
      </v-list>
    </v-infinite-scroll>
  </template>
</template>

<style scoped>
/* Mobile: Extend summary cards to container edges */
.mobile-summary-cards {
  padding: 12px;
}


.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
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

.subtitle-chip {
  /* Non-interactive info chip */
  background-color: transparent !important;
  border-color: rgb(var(--v-theme-outline)) !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-radius: 8px !important;
  pointer-events: none !important;
  opacity: 1 !important;
}

.filter-chip-selected {
  /* Selected state - flat variant with secondary-container */
  background-color: rgb(var(--v-theme-secondary-container)) !important;
  border-width: 0 !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-radius: 8px !important;
}

/* Ensure filter icon inherits correct color */
.subtitle-chip :deep(.v-icon) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}
</style>

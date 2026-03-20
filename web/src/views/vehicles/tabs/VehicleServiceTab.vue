<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useNotificationsStore } from '@/stores/notifications'

import { getServiceRecords } from '@/api/generated/service-records/service-records'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

import ServiceRecordsTable from '@/components/vehicles/service/ServiceRecordsTable.vue'
import ServiceRecordsList from '@/components/vehicles/service/ServiceRecordsList.vue'
import ServiceDetailsWrapper from '@/components/vehicles/service/ServiceDetailsWrapper.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

import { useServiceDetailsState } from '@/composables/vehicles/useServiceDetailsState'

interface Props {
  vehicleId: string
}

const props = defineProps<Props>()

const { isMobile } = useResponsiveLayout()
const notifications = useNotificationsStore()
const detailsState = useServiceDetailsState()

const { getApiVehiclesVehicleIdServiceRecords, deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId } = getServiceRecords()

// List state
const serviceRecords = ref<ServiceRecordDto[]>([])
const serviceRecordsLoading = ref(false)
const serviceRecordsPage = ref(1)
const serviceRecordsPageSize = ref(10)
const serviceRecordsTotal = ref(0)
const hasMoreRecords = ref(true)
const error = ref<string | null>(null)
const infiniteScrollKey = ref(0)

// Sorting
const sortBy = ref<string | undefined>(undefined)
const sortDescending = ref(false)

// Selection — shared between mobile list and desktop table
const selectedIds = ref<string[]>([])
const hasSelection = computed(() => selectedIds.value.length > 0)
const selectedCount = computed(() => selectedIds.value.length)

function clearSelection() {
  selectedIds.value = []
}

function resetList() {
  serviceRecordsPage.value = 1
  serviceRecords.value = []
  selectedIds.value = []
  infiniteScrollKey.value++
}

// Delete
const entryToDeleteId = ref<string | null>(null)
const showSingleDeleteDialog = ref(false)
const showBulkDeleteDialog = ref(false)

function openSingleDeleteDialog(id: string) {
  entryToDeleteId.value = id
  showSingleDeleteDialog.value = true
}

async function confirmSingleDelete() {
  if (!entryToDeleteId.value) return
  try {
    await deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, entryToDeleteId.value)
    notifications.show('Service record deleted successfully.')
    resetList()
    if (!isMobile.value) loadServiceRecords()
  } catch (err) {
    console.error('Delete failed', err)
  } finally {
    showSingleDeleteDialog.value = false
    entryToDeleteId.value = null
  }
}

async function confirmBulkDelete() {
  if (selectedIds.value.length === 0) return
  const idsToDelete = [...selectedIds.value]
  showBulkDeleteDialog.value = false
  resetList()
  try {
    await Promise.all(idsToDelete.map((id) => deleteApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, id)))
    notifications.show('Service records deleted successfully.')
    if (!isMobile.value) loadServiceRecords()
  } catch (err) {
    console.error('Bulk delete failed', err)
  }
}

// Select — list emits id, table emits full record
function handleSelectById(id: string) {
  const record = serviceRecords.value.find((r) => r.id === id)
  if (record) detailsState.open(record)
}

// Data loading
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

    const fetchedItems = response.items ?? []
    const totalCount = response.totalCount ?? 0

    if (isMobile.value && serviceRecordsPage.value > 1) {
      serviceRecords.value = [...serviceRecords.value, ...fetchedItems]
    } else {
      serviceRecords.value = fetchedItems
    }

    serviceRecordsTotal.value = totalCount
    hasMoreRecords.value = serviceRecords.value.length < serviceRecordsTotal.value

    // Refresh open record if it's in the fetched batch
    if (detailsState.isOpen.value && detailsState.mode.value === 'view' && detailsState.selectedRecord.value) {
      const freshRecord = fetchedItems.find((r) => r.id === detailsState.selectedRecord.value?.id)
      if (freshRecord) detailsState.updateSelectedRecord(freshRecord)
    }
  } catch (err) {
    console.error('Failed to load service records:', err)
    error.value = 'Failed to load service records. Please try again.'
    serviceRecords.value = []
    serviceRecordsTotal.value = 0
    hasMoreRecords.value = false
  } finally {
    serviceRecordsLoading.value = false
  }
}

async function loadMore({ done }: { done: (status: 'ok' | 'empty' | 'error') => void }) {
  if (serviceRecordsLoading.value) {
    done('ok')
    return
  }
  serviceRecordsPage.value = serviceRecords.value.length > 0 ? serviceRecordsPage.value + 1 : 1
  await loadServiceRecords()
  if (error.value) {
    done('error')
  } else if (!hasMoreRecords.value) {
    done('empty')
  } else {
    done('ok')
  }
}

function handleSortUpdate(sortOptions: { key: string; order: string }[]) {
  if (sortOptions?.length > 0) {
    sortBy.value = sortOptions[0].key
    sortDescending.value = sortOptions[0].order === 'desc'
  } else {
    sortBy.value = undefined
    sortDescending.value = false
  }
  serviceRecordsPage.value = 1
  loadServiceRecords()
}

function handlePageChange(page: number) {
  serviceRecordsPage.value = page
  loadServiceRecords()
}

function handlePageSizeChange(size: number) {
  serviceRecordsPageSize.value = size
  loadServiceRecords()
}

onMounted(() => {
  if (!isMobile.value) loadServiceRecords()
})

onUnmounted(() => {
  detailsState.close()
})
</script>

<template>
  <template v-if="!isMobile">
    <!-- Desktop: stats cards -->
    <v-row>
      <v-col v-for="n in 4" :key="n" cols="6" sm="6" md="3">
        <v-card class="summary-card" height="120" color="primary-container" variant="flat">
          <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
            <v-icon icon="mdi-gas-station" size="32" class="position-absolute text-primary" style="top: 12px; right: 16px; opacity: 0.6" />
            <div class="text-caption text-on-primary-container">Title</div>
            <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Desktop: table -->
    <v-row>
      <v-col cols="12">
        <v-card variant="flat">
          <v-alert v-if="error" type="error" density="compact" class="mb-4" closable>
            {{ error }}
          </v-alert>
          <ServiceRecordsTable
            :items="serviceRecords"
            :total-count="serviceRecordsTotal"
            :loading="serviceRecordsLoading"
            :page="serviceRecordsPage"
            :items-per-page="serviceRecordsPageSize"
            :selected-id="detailsState.selectedRecord.value?.id"
            :selected-ids="selectedIds"
            @select="detailsState.open"
            @update:selected-ids="selectedIds = $event"
            @delete-selected="showBulkDeleteDialog = true"
            @update:page="handlePageChange"
            @update:items-per-page="handlePageSizeChange"
            @update:sort-by="handleSortUpdate"
          />
        </v-card>
      </v-col>
    </v-row>
  </template>

  <template v-else>
    <!-- Mobile: context bar -->
    <div class="topbar-container">
      <v-fade-transition mode="out-in" duration="200">
        <div v-if="hasSelection" key="context-bar" class="context-bar d-flex align-center w-100 rounded-pill px-2 py-1">
          <v-tooltip text="Clear Selection" location="bottom" open-delay="200" close-delay="500">
            <template #activator="{ props: tooltipProps }">
              <v-btn v-bind="tooltipProps" icon="mdi-close" variant="text" @click="clearSelection" />
            </template>
          </v-tooltip>
          <div class="text-subtitle-1 font-weight-medium ml-2">{{ selectedCount }} items selected</div>
          <v-spacer />
          <v-tooltip text="Delete selected" location="bottom" open-delay="200" close-delay="500">
            <template #activator="{ props: tooltipProps }">
              <v-btn v-bind="tooltipProps" icon="mdi-delete" variant="text" color="error" @click="showBulkDeleteDialog = true" />
            </template>
          </v-tooltip>
        </div>

        <div v-else key="standard-bar" class="d-flex justify-end align-center w-100 py-1">
          <v-btn icon="mdi-filter-variant" variant="text" disabled />
        </div>
      </v-fade-transition>
    </div>

    <!-- Mobile: infinite scroll list -->
    <v-alert v-if="error" type="error" density="compact" class="mb-4" closable>
      {{ error }}
    </v-alert>

    <v-infinite-scroll :key="infiniteScrollKey" :onLoad="loadMore" :items="serviceRecords">
      <ServiceRecordsList v-model="selectedIds" :items="serviceRecords" @select="handleSelectById" @delete="openSingleDeleteDialog" />
      <template #empty>
        <div class="pa-4 text-center text-medium-emphasis text-caption">No more records</div>
      </template>
    </v-infinite-scroll>
  </template>

  <!-- Detail panel -->
  <ServiceDetailsWrapper
    v-if="detailsState.selectedRecord.value || detailsState.mode.value === 'create'"
    :model-value="detailsState.isOpen.value"
    @update:model-value="(v) => !v && detailsState.close()"
    :record="detailsState.selectedRecord.value"
    :vehicle-id="vehicleId"
    @refresh-data="loadServiceRecords"
  />

  <!-- Delete dialogs -->
  <DeleteDialog
    item-to-delete="service record"
    :is-open="showSingleDeleteDialog"
    :on-confirm="confirmSingleDelete"
    :on-cancel="() => (showSingleDeleteDialog = false)"
  />
  <DeleteDialog
    :item-to-delete="selectedCount > 1 ? `${selectedCount} service records` : `${selectedCount} service record`"
    :is-open="showBulkDeleteDialog"
    :on-confirm="confirmBulkDelete"
    :on-cancel="() => (showBulkDeleteDialog = false)"
  />
</template>

<style scoped lang="scss">
.topbar-container {
  display: flex;
  align-items: center;
  min-height: 64px;
  padding: 0;
}

.context-bar {
  background-color: rgb(var(--v-theme-secondary-container));
  color: rgb(var(--v-theme-on-secondary-container));
}

.summary-card {
  transition: background-color 0.2s ease-in-out;
}

.summary-card:hover {
  background-color: rgb(var(--v-theme-surface-container));
}
</style>

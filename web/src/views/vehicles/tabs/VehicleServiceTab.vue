<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue' // Dodane onUnmounted
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'

import { getServiceRecords } from '@/api/generated/service-records/service-records'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

import ServiceRecordsTable from '@/components/vehicles/service/ServiceRecordsTable.vue'
import ServiceRecordsList from '@/components/vehicles/service/ServiceRecordsList.vue'
import ServiceDetailsWrapper from '@/components/vehicles/service/ServiceDetailsWrapper.vue'

import { useServiceDetailsState } from '@/composables/vehicle/useServiceDetailsState'

const { isMobile } = useResponsiveLayout()

const detailsState = useServiceDetailsState()

interface Props {
  vehicleId: string
  selectedServiceRecords: string[]
  mockStats: {
    totalServiceCost: number
  }
}

const props = defineProps<Props>()

const { getApiVehiclesVehicleIdServiceRecords } = getServiceRecords()

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

    if (detailsState.isOpen.value && detailsState.mode.value === 'view' && detailsState.selectedRecord.value) {
      const currentRecordId = detailsState.selectedRecord.value.id
      const freshRecord = fetchedItems.find((r) => r.id === currentRecordId)

      if (freshRecord) {
        detailsState.updateSelectedRecord(freshRecord)
      }
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
  if (!hasMoreRecords.value) {
    done('empty')
    return
  }

  serviceRecordsPage.value++
  await loadServiceRecords()

  done(hasMoreRecords.value ? 'ok' : 'empty')
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
  serviceRecordsPage.value = 1
  loadServiceRecords()
}

onMounted(() => {
  loadServiceRecords()
})

onUnmounted(() => {
  detailsState.close()
})

defineExpose({
  loadServiceRecords,
})
</script>

<template>
  <div class="h-100 position-relative">
    <template v-if="!isMobile">
      <v-row>
        <v-col cols="6" sm="6" md="3">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon
                icon="mdi-gas-station"
                size="32"
                class="position-absolute text-primary"
                style="top: 12px; right: 16px; opacity: 0.6"
              />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="6" sm="6" md="3">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon
                icon="mdi-gas-station"
                size="32"
                class="position-absolute text-primary"
                style="top: 12px; right: 16px; opacity: 0.6"
              />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="6" sm="6" md="3">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon
                icon="mdi-gas-station"
                size="32"
                class="position-absolute text-primary"
                style="top: 12px; right: 16px; opacity: 0.6"
              />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="6" sm="6" md="3">
          <v-card class="summary-card" height="120" color="primary-container" variant="flat">
            <v-card-text class="d-flex flex-column justify-center h-100 position-relative">
              <v-icon
                icon="mdi-gas-station"
                size="32"
                class="position-absolute text-primary"
                style="top: 12px; right: 16px; opacity: 0.6"
              />
              <div class="text-caption text-on-primary-container">Title</div>
              <div class="text-h6 font-weight-bold text-on-primary-container">Description</div>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <v-card class="service-records" variant="flat">
            <v-alert v-if="error" type="error" density="compact" class="mb-4" closable>
              {{ error }}
            </v-alert>
            <ServiceRecordsTable
              :items="serviceRecords"
              :items-length="serviceRecordsTotal"
              :loading="serviceRecordsLoading"
              :page="serviceRecordsPage"
              :items-per-page="serviceRecordsPageSize"
              :selected-id="detailsState.selectedRecord.value?.id"
              @update:page="
                serviceRecordsPage = $event;
                loadServiceRecords();
              "
              @update:items-per-page="
                serviceRecordsPageSize = $event;
                loadServiceRecords();
              "
              @update:sort-by="handleSortUpdate"
              @select="detailsState.open"
            />
          </v-card>
        </v-col>
      </v-row>
    </template>

    <template v-else>
      <v-alert v-if="error" type="error" density="compact" class="mb-4" closable>
        {{ error }}
      </v-alert>

      <v-infinite-scroll class="mobile-infinite-scroll" :onLoad="loadMore" :items="serviceRecords">
        <ServiceRecordsList :items="serviceRecords" @select="detailsState.open" />
      </v-infinite-scroll>
    </template>

    <ServiceDetailsWrapper
      v-if="detailsState.selectedRecord.value || detailsState.mode.value === 'create'"
      v-model="detailsState.isOpen.value"
      :record="detailsState.selectedRecord.value"
      :vehicle-id="vehicleId"
      @refresh-data="loadServiceRecords"
    />
  </div>
</template>

<style scoped>

.mobile-summary-cards {
  padding: 12px;
}

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

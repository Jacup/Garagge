<script setup lang="ts">
import { onMounted, ref, watch, computed } from 'vue'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useEnergyEntriesState } from '@/composables/vehicles/useEnergyEntriesState'
import { useNotificationsStore } from '@/stores/notifications'

import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import {
  NullableOfContextTrend,
  NullableOfTrendMode,
  type EnergyEntryDto,
  type EnergyStatsDto,
  type EnergyType,
} from '@/api/generated/apiV1.schemas'

import EnergyEntriesList from '@/components/vehicles/energy/EnergyEntriesList.vue'
import EnergyEntriesTable from '@/components/vehicles/energy/EnergyEntriesTable.vue'
import EnergyStatCard from '@/components/vehicles/energy/EnergyStatCard.vue'

import EnergyEntryDialog from '@/components/vehicles/energy/EnergyEntryDialog.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'
import ConnectedButtonGroup from '@/components/common/ConnectedButtonGroup.vue'
import StatCard from '@/components/dashboard/StatCard.vue'
import EnergyStatsSection from '@/components/vehicles/energy/EnergyStatsSection.vue'
import EnergyPriceChart from '@/components/vehicles/energy/charts/EnergyPriceChart.vue'
import EnergyChartsSection, { type EnergyChartEntryDto } from '@/components/vehicles/energy/charts/EnergyChartsSection.vue'

interface Props {
  vehicleId: string
  allowedEnergyTypes: EnergyType[] | undefined
  energystats: EnergyStatsDto | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'entry-changed': []
}>()

const { isMobile } = useResponsiveLayout()
const { getApiVehiclesVehicleIdEnergyEntries, deleteApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()
const { showEntryDialog, selectedEntry, openEditDialog, closeDialog } = useEnergyEntriesState()
const notifications = useNotificationsStore()

const energyEntries = ref<EnergyEntryDto[]>([])
const energyEntriesLoading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const itemsPerPage = ref(10)
const hasMoreRecords = ref(true)
const error = ref<string | null>(null)
const infiniteScrollKey = ref(0)

const selectedEnergyTypeFilters = ref<EnergyType[]>([])
const selectedEntryIds = ref<string[]>([])

const hasSelection = computed(() => selectedEntryIds.value.length > 0)
const selectedCount = computed(() => selectedEntryIds.value.length)

function clearSelection() {
  selectedEntryIds.value = []
}

function resetList() {
  page.value = 1
  energyEntries.value = []
  selectedEntryIds.value = []
  infiniteScrollKey.value++
}

async function loadEnergyEntries() {
  if (!props.vehicleId) return

  energyEntriesLoading.value = true
  error.value = null

  try {
    const response = await getApiVehiclesVehicleIdEnergyEntries(props.vehicleId, {
      page: page.value,
      pageSize: itemsPerPage.value,
      energyTypes: selectedEnergyTypeFilters.value.length > 0 ? selectedEnergyTypeFilters.value : undefined,
    })

    const fetchedItems = response.items ?? []
    const total = response.totalCount ?? 0

    if (isMobile.value && page.value > 1) {
      energyEntries.value = [...energyEntries.value, ...fetchedItems]
    } else {
      energyEntries.value = fetchedItems
    }

    totalCount.value = total
    hasMoreRecords.value = energyEntries.value.length < totalCount.value
  } catch (err) {
    console.error('Failed to load energy entries:', err)
    error.value = 'Failed to load data'
    if (page.value === 1) energyEntries.value = []
    hasMoreRecords.value = false
  } finally {
    energyEntriesLoading.value = false
  }
}

async function loadMore({ done }: { done: (status: 'ok' | 'empty' | 'error') => void }) {
  if (energyEntriesLoading.value) {
    done('ok')
    return
  }

  page.value = energyEntries.value.length > 0 ? page.value + 1 : 1

  await loadEnergyEntries()

  if (error.value) {
    done('error')
  } else if (!hasMoreRecords.value) {
    done('empty')
  } else {
    done('ok')
  }
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  loadEnergyEntries()
}

const handlePageSizeChange = (newSize: number) => {
  itemsPerPage.value = newSize
  page.value = 1
  loadEnergyEntries()
}

function onEntrySaved() {
  resetList()
  if (!isMobile.value) loadEnergyEntries()
  emit('entry-changed')
}

const entryToDeleteId = ref<string | null>(null)
const showSingleDeleteDialog = ref(false)
const showBulkDeleteDialog = ref(false)

function openSingleDeleteDialog(id: string | undefined) {
  if (!id) return
  entryToDeleteId.value = id
  showSingleDeleteDialog.value = true
}

async function confirmSingleDelete() {
  if (!entryToDeleteId.value) return

  try {
    await deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, entryToDeleteId.value)
    notifications.show('Fuel entry deleted successfully.')
    resetList()
    if (!isMobile.value) loadEnergyEntries()
    emit('entry-changed')
  } catch (err) {
    console.error('Delete failed', err)
  } finally {
    showSingleDeleteDialog.value = false
    entryToDeleteId.value = null
  }
}

async function confirmBulkDelete() {
  if (selectedEntryIds.value.length === 0) return

  const idsToDelete = [...selectedEntryIds.value]
  showBulkDeleteDialog.value = false
  resetList()

  try {
    await Promise.all(idsToDelete.map((id) => deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, id)))
    notifications.show('Fuel entries deleted successfully.')
    if (!isMobile.value) loadEnergyEntries()
    emit('entry-changed')
  } catch (err) {
    console.error('Delete failed', err)
  }
}

onMounted(() => {
  if (!isMobile.value) {
    loadEnergyEntries()
  }
})

watch(
  () => props.vehicleId,
  () => {
    resetList()
    loadEnergyEntries()
  },
)

watch(selectedEnergyTypeFilters, () => {
  resetList()
  loadEnergyEntries()
})

const dataPeriod = ref<0 | 1 | 2 | 3>(1)
const viewModeOptions = [
  { value: 3 as const, text: 'Week' },
  { value: 2 as const, text: 'Month' },
  { value: 1 as const, text: 'Year' },
  { value: 0 as const, text: 'Lifetime' },
]

const energyTypeStats = [
  {
    type: 'Gasoline',
    itemsCount: 12 as number,
    totalCost: 1200.5 as number,
    totalVolume: 100.42 as number,
    averageConsumption: 8.3 as number,
    averagePricePerUnit: 12.23 as number,
    averageCostPer100km: 96.23 as number,
  },
  {
    type: 'Electricity',
    itemsCount: 8 as number,
    totalCost: 1000 as number,
    totalVolume: 500 as number,
    averageConsumption: 6 as number,
    averagePricePerUnit: 12.5 as number,
    averageCostPer100km: 75 as number,
  },
]
const chartEntries: EnergyChartEntryDto[] = [
  { date: '2025-02-25T08:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.88, consumption: null, cost: null },
  { date: '2025-03-18T11:30:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.92, consumption: 4.2, cost: 38.4 },
  { date: '2025-05-05T09:15:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.95, consumption: 3.8, cost: 39.0 },
  { date: '2025-06-20T14:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.91, consumption: 4.5, cost: 38.2 },
  { date: '2025-08-01T10:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.89, consumption: 5.1, cost: 37.8 },
  { date: '2025-09-14T08:45:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.85, consumption: 4.0, cost: 37.0 },
  { date: '2025-11-02T12:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.82, consumption: 4.8, cost: 36.4 },
  { date: '2025-12-28T09:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.8, consumption: 5.3, cost: 36.0 },
  { date: '2026-01-15T10:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.83, consumption: 5.0, cost: 36.6 },
  { date: '2026-02-10T09:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.86, consumption: 4.6, cost: 37.2 },
  { date: '2026-02-17T08:30:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.88, consumption: 4.3, cost: 37.6 },
  { date: '2026-02-21T07:00:00', type: 'Gasoline', energyUnit: 'Liter', pricePerUnit: 1.87, consumption: 4.1, cost: 37.4 },

  { date: '2025-02-25T08:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.68, consumption: null, cost: null },
  { date: '2025-03-05T19:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.68, consumption: 18.2, cost: 13.6 },
  { date: '2025-03-20T20:30:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.65, consumption: 17.8, cost: 13.0 },
  { date: '2025-04-10T18:45:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.62, consumption: 16.5, cost: 12.4 },
  { date: '2025-05-22T21:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.6, consumption: 15.9, cost: 12.0 },
  { date: '2025-07-08T20:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.58, consumption: 15.2, cost: 11.6 },
  { date: '2025-08-19T19:30:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.6, consumption: 15.8, cost: 12.0 },
  { date: '2025-10-03T20:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.63, consumption: 17.1, cost: 12.6 },
  { date: '2025-11-15T19:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.66, consumption: 19.4, cost: 13.2 },
  { date: '2025-12-10T20:30:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.7, consumption: 21.0, cost: 14.0 },
  { date: '2026-01-22T19:15:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.68, consumption: 20.2, cost: 13.6 },
  { date: '2026-02-12T20:00:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.65, consumption: 18.8, cost: 13.0 },
  { date: '2026-02-19T19:30:00', type: 'Electric', energyUnit: 'kWh', pricePerUnit: 0.64, consumption: 17.9, cost: 12.8 },
]
</script>

<template>
  <v-row class="ma-0 mb-4">
    <div class="d-flex flex-row align-center w-100">
      <v-spacer />
      <ConnectedButtonGroup v-model="dataPeriod" :options="viewModeOptions" mandatory />
    </div>
  </v-row>

  <v-row>
    <v-col cols="12" md="9">
      <v-row>
        <v-col cols="12" sm="4">
          <StatCard
            title="Driving cost"
            :metric="{
              value: '32,50 PLN',
              subtitle: 'per 100 km',
              contextValue: '12%',
              contextAppendText: 'vs last month',
              contextTrend: NullableOfContextTrend.Up,
              contextTrendMode: NullableOfTrendMode.Bad,
            }"
            icon="mdi-gauge"
            accent-color="primary"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <StatCard title="Fuel cost" :metric="{ value: '2 450 PLN' }" icon="mdi-cash-multiple" accent-color="secondary" />
        </v-col>
        <v-col cols="12" sm="4">
          <StatCard title="Distance driven" :metric="{ value: '1200 km' }" icon="mdi-map-marker-distance" accent-color="tertiary" />
        </v-col>

        <v-col cols="12">
          <EnergyChartsSection :entries="chartEntries" :data-period="dataPeriod" />
        </v-col>
      </v-row>
    </v-col>

    <v-col cols="12" md="3">
      <EnergyStatsSection :stats="energyTypeStats" />
    </v-col>
  </v-row>

  <v-divider class="my-6"></v-divider>

  <v-row>
    <v-col cols="12">
      <v-card variant="flat" color="secondary-container" height="400" title="Records history table"> </v-card>
    </v-col>
  </v-row>

  <EnergyEntryDialog
    :model-value="showEntryDialog"
    :vehicleId="vehicleId"
    :entry="selectedEntry"
    :allowedEnergyTypes="allowedEnergyTypes"
    @update:model-value="(val) => !val && closeDialog()"
    @saved="onEntrySaved"
  />

  <DeleteDialog
    item-to-delete="fuel entry"
    :is-open="showSingleDeleteDialog"
    :on-confirm="confirmSingleDelete"
    :on-cancel="() => (showSingleDeleteDialog = false)"
  />
  <DeleteDialog
    :item-to-delete="selectedCount > 1 ? `${selectedCount} fuel entries` : `${selectedCount} fuel entry`"
    :is-open="showBulkDeleteDialog"
    :on-confirm="confirmBulkDelete"
    :on-cancel="() => (showBulkDeleteDialog = false)"
  />
</template>

<style scoped lang="scss">
.fuel-card {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.fuel-type-stats-card {
}
.fuel-container-row {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  align-items: flex-start;
}

.table-container-flex {
  flex: 1 1 65%;
  min-width: 600px;
  max-width: 1400px;

  @media (max-width: 960px) {
    flex: 1 1 100%;
    min-width: 100%;
  }
}

.statistics-container-flex {
  flex: 1 1 300px;
  max-width: 600px;

  @media (max-width: 960px) {
    flex: 1 1 100%;
    max-width: 100%;
  }
}

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

.filter-chip {
  background-color: transparent !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-color: rgb(var(--v-theme-outline)) !important;
  border-radius: 8px !important;
  margin-top: 0px;
  margin-left: 8px;
  margin-right: 0px;
}

.filter-chip-selected {
  background-color: rgb(var(--v-theme-secondary-container)) !important;
  border-width: 0 !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-radius: 8px !important;
}

.filter-chip :deep(.v-icon) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.filter-chip-selected :deep(.v-icon) {
  color: rgb(var(--v-theme-on-secondary-container)) !important;
}

.equal-height-row {
  align-items: stretch;
}

.opacity-20 {
  opacity: 0.2;
}

.summary-card {
  transition: background-color 0.2s ease-in-out;
}

.summary-card:hover {
  background-color: rgb(var(--v-theme-surface-container));
}
</style>

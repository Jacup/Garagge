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
import EnergyChartsSection from '@/components/vehicles/energy/charts/EnergyChartsSection.vue'

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
const fuelEntries: FuelPriceEntry[] = [
  // Diesel
  { datetime: '2025-02-25T08:00:00', pricePerUnit: 1.72, type: 'Diesel' }, // ~4 tyg. temu
  { datetime: '2025-03-18T11:30:00', pricePerUnit: 1.68, type: 'Diesel' },
  { datetime: '2025-05-05T09:15:00', pricePerUnit: 1.75, type: 'Diesel' },
  { datetime: '2025-06-20T14:00:00', pricePerUnit: 1.8, type: 'Diesel' },
  { datetime: '2025-08-01T10:00:00', pricePerUnit: 1.77, type: 'Diesel' },
  { datetime: '2025-09-14T08:45:00', pricePerUnit: 1.65, type: 'Diesel' },
  { datetime: '2025-11-02T12:00:00', pricePerUnit: 1.6, type: 'Diesel' },
  { datetime: '2025-12-28T09:00:00', pricePerUnit: 1.58, type: 'Diesel' },
  { datetime: '2026-01-15T10:00:00', pricePerUnit: 1.62, type: 'Diesel' }, // ~5 tyg. temu
  { datetime: '2026-02-10T09:00:00', pricePerUnit: 1.64, type: 'Diesel' }, // ~12 dni temu (month)
  { datetime: '2026-02-17T08:30:00', pricePerUnit: 1.66, type: 'Diesel' }, // ~5 dni temu (week)
  { datetime: '2026-02-21T07:00:00', pricePerUnit: 1.67, type: 'Diesel' }, // wczoraj

  // Petrol 95
  { datetime: '2025-03-10T10:00:00', pricePerUnit: 1.88, type: 'Petrol 95' },
  { datetime: '2025-04-22T13:00:00', pricePerUnit: 1.92, type: 'Petrol 95' },
  { datetime: '2025-06-05T09:30:00', pricePerUnit: 1.95, type: 'Petrol 95' },
  { datetime: '2025-07-19T11:00:00', pricePerUnit: 1.89, type: 'Petrol 95' },
  { datetime: '2025-09-30T14:30:00', pricePerUnit: 1.82, type: 'Petrol 95' },
  { datetime: '2025-11-18T08:00:00', pricePerUnit: 1.78, type: 'Petrol 95' },
  { datetime: '2026-01-07T10:15:00', pricePerUnit: 1.8, type: 'Petrol 95' },
  { datetime: '2026-01-28T11:00:00', pricePerUnit: 1.83, type: 'Petrol 95' }, // ~3 tyg. temu (month)
  { datetime: '2026-02-08T14:00:00', pricePerUnit: 1.86, type: 'Petrol 95' }, // ~2 tyg. temu (month)
  { datetime: '2026-02-16T12:00:00', pricePerUnit: 1.88, type: 'Petrol 95' }, // ~6 dni temu (week)
  { datetime: '2026-02-20T16:30:00', pricePerUnit: 1.9, type: 'Petrol 95' }, // przedwczoraj
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
          <EnergyChartsSection :entries="fuelEntries" :data-period="dataPeriod" />
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

  <!-- <EnergyEntryDialog
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
  /> -->
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

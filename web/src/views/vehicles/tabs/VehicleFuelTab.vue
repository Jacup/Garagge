<script setup lang="ts">
import { ref, watch } from 'vue'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import {
  StatsPeriod,
  type EnergyEntryDto,
  type EnergyStatsDto,
  type EnergyType,
  type EnergyTypeStatsDto,
} from '@/api/generated/apiV1.schemas'

import ConnectedButtonGroup from '@/components/common/ConnectedButtonGroup.vue'
import StatCard from '@/components/dashboard/StatCard.vue'
import EnergyStatsSection from '@/components/vehicles/energy/stats/EnergyStatsSection.vue'
import EnergyChartsSection from '@/components/vehicles/energy/charts/EnergyChartsSection.vue'
import EnergyEntriesSection from '@/components/vehicles/energy/entriesData/EnergyEntriesSection.vue'

interface Props {
  vehicleId: string
  allowedEnergyTypes: EnergyType[] | undefined
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'entry-changed': []
}>()

const { getApiVehiclesVehicleIdEnergyEntriesStats } = getEnergyEntries()

const dataPeriod = ref<StatsPeriod>(StatsPeriod.Year)
const stats = ref<EnergyStatsDto | null>(null)
const statsLoading = ref(false)

const viewModeOptions = [
  { value: StatsPeriod.Week, text: 'Week' },
  { value: StatsPeriod.Month, text: 'Month' },
  { value: StatsPeriod.Year, text: 'Year' },
  { value: StatsPeriod.Lifetime, text: 'Lifetime' },
]

async function loadStats() {
  if (!props.vehicleId) return

  try {
    statsLoading.value = true
    stats.value = await getApiVehiclesVehicleIdEnergyEntriesStats(props.vehicleId, {
      period: dataPeriod.value,
    })
  } catch (err) {
    console.error('Failed to load energy stats:', err)
    stats.value = null
  } finally {
    statsLoading.value = false
  }
}

async function handleEntryChanged() {
  await loadStats()
  emit('entry-changed')
}

watch(dataPeriod, loadStats, { immediate: true })
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
            title="Fuel cost"
            :metric="{ value: stats ? `${stats.totalFuelCost} PLN` : '—', subtitle: 'total' }"
            icon="mdi-cash-multiple"
            accent-color="primary"
            :loading="statsLoading"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <StatCard
            title="Fuel entries"
            :metric="{ value: stats ? String(stats.totalEntries) : '—' }"
            icon="mdi-gas-station"
            accent-color="secondary"
            :loading="statsLoading"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <StatCard
            title="Distance driven"
            :metric="{ value: stats ? `${stats.distanceDriven} km` : '—' }"
            icon="mdi-map-marker-distance"
            accent-color="tertiary"
            :loading="statsLoading"
          />
        </v-col>

        <v-col cols="12">
          <EnergyChartsSection :entries="stats?.chartEntries ?? []" :data-period="dataPeriod" :loading="statsLoading" />
        </v-col>
      </v-row>
    </v-col>

    <v-col cols="12" md="3">
      <EnergyStatsSection :stats="stats?.statsByType ?? []" :loading="statsLoading" />
    </v-col>
  </v-row>

  <v-divider class="my-6" />

  <v-row>
    <v-col cols="12">
      <EnergyEntriesSection :vehicle-id="vehicleId" :allowed-energy-types="allowedEnergyTypes" @entry-changed="handleEntryChanged" />
    </v-col>
  </v-row>
</template>

<style scoped lang="scss">
.fuel-card {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>

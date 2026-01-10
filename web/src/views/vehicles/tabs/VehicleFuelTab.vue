<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'

import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import type { EnergyEntryDto, EnergyStatsDto, EnergyType } from '@/api/generated/apiV1.schemas'

import EnergyEntriesTable from '@/components/vehicles/EnergyEntriesTable.vue'
import EnergyStatisticsCard from '@/components/vehicles/EnergyStatisticsCard.vue'
import EnergyEntriesList from '@/components/vehicles/fuel/EnergyEntriesList.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

interface Props {
  vehicleId: string
  energyTypes?: EnergyType[]
  energystats: EnergyStatsDto | null
  statsLoading: boolean
  selectedEnergyEntries: string[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:selectedEnergyEntries': [value: string[]]
  'entry-changed': []
  'add-entry': []
  'bulk-delete': []
}>()

const { isMobile } = useResponsiveLayout()
const { getApiVehiclesVehicleIdEnergyEntries, deleteApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()

const energyEntries = ref<EnergyEntryDto[]>([])
const energyEntriesLoading = ref(false)
const energyEntriesPage = ref(1)
const energyEntriesPageSize = ref(10)
const energyEntriesTotal = ref(0)
const error = ref<string | null>(null)
const hasMoreRecords = ref(true)

const showSingleDeleteDialog = ref(false)
const energyEntryToDeleteId = ref<string | null>(null)

const energyEntriesTableRef = ref<InstanceType<typeof EnergyEntriesTable> | null>(null)

async function loadEnergyEntries() {
  if (!props.vehicleId) return

  energyEntriesLoading.value = true
  error.value = null // Reset błędu przed nowym zapytaniem

  try {
    const response = await getApiVehiclesVehicleIdEnergyEntries(props.vehicleId, {
      page: energyEntriesPage.value, // Używamy aktualnej strony
      pageSize: energyEntriesPageSize.value,
      energyTypes: props.energyTypes,
    })

    const fetchedItems = response.items ?? []
    const totalCount = response.totalCount ?? 0

    // Logika łączenia tablic
    // Jeśli to mobile i mamy już jakieś dane (i nie jest to przeładowanie strony 1), to doklejamy
    if (isMobile.value && energyEntriesPage.value > 1) {
      energyEntries.value = [...energyEntries.value, ...fetchedItems]
    } else {
      // W przeciwnym razie (Desktop lub pierwsza strona Mobile) nadpisujemy
      energyEntries.value = fetchedItems
    }

    energyEntriesTotal.value = totalCount

    // Ważne: Sprawdzamy czy pobraliśmy tyle ile chcieliśmy, lub czy total został osiągnięty
    hasMoreRecords.value = energyEntries.value.length < energyEntriesTotal.value
  } catch (err) {
    console.error('Failed to load energy entries:', err)
    error.value = 'Failed'
    // W przypadku błędu nie czyścimy listy, żeby user nie stracił tego co widzi
    if (energyEntriesPage.value === 1) {
      energyEntries.value = []
    }
    hasMoreRecords.value = false
  } finally {
    energyEntriesLoading.value = false
  }
}

async function loadMore({ done }: { done: (status: 'ok' | 'empty' | 'error') => void }) {
  if (energyEntriesLoading.value) return

  if (energyEntries.value.length > 0) {
    energyEntriesPage.value++
  } else {
    energyEntriesPage.value = 1
  }

  await loadEnergyEntries()

  if (error.value) {
    done('error')
  } else if (!hasMoreRecords.value) {
    done('empty')
  } else {
    done('ok')
  }
}

function openSingleDeleteDialog(id: string | undefined) {
  if (id) {
    energyEntryToDeleteId.value = id
    showSingleDeleteDialog.value = true
  }
}

async function confirmSingleDelete() {
  if (!energyEntryToDeleteId.value) return
  const idToDelete = energyEntryToDeleteId.value

  const energyEntryIndex = energyEntries.value.findIndex((e) => e.id === energyEntryToDeleteId.value)
  const deletedEntry = energyEntryIndex >= 0 ? energyEntries.value[energyEntryIndex] : null

  if (energyEntryIndex >= 0) {
    energyEntries.value = energyEntries.value.filter((e) => e.id != idToDelete)
    energyEntriesTotal.value = Math.max(0, energyEntriesTotal.value - 1)
  }

  try {
    await deleteApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, idToDelete)
    await loadEnergyEntries()
    return true
  } catch (error) {
    if (deletedEntry && energyEntryIndex >= 0) {
      energyEntries.value.splice(energyEntryIndex, 0, deletedEntry)
      energyEntriesTotal.value++
    }
    console.error('Failed to delete energy entry:', error)
    return false
  } finally {
    showSingleDeleteDialog.value = false
    energyEntryToDeleteId.value = null
  }
}

onMounted(() => {
  if (!isMobile.value) {
    loadEnergyEntries()
  }
})

defineExpose({
  energyEntriesTableRef,
})
</script>

<template>
  <template v-if="!isMobile">
    <v-row class="equal-height-row">
      <v-col cols="12" md="8">
        <v-card class="card-background" variant="flat" rounded="md-16px" height="520px">
          <template #title>Fuel History</template>
          <template #append>
            <v-btn
              v-if="selectedEnergyEntries.length > 0"
              class="text-none mr-2"
              prepend-icon="mdi-delete"
              variant="flat"
              color="error"
              size="small"
              @click="emit('bulk-delete')"
            >
              Delete ({{ selectedEnergyEntries.length }})
            </v-btn>
            <v-btn class="text-none" prepend-icon="mdi-plus" variant="flat" color="primary" size="small" @click="emit('add-entry')">
              Add
            </v-btn>
          </template>
          <v-card-text>
            <EnergyEntriesTable
              ref="energyEntriesTableRef"
              :vehicle-id="vehicleId"
              :allowed-energy-types="energyTypes"
              :selected="selectedEnergyEntries"
              @update:selected="emit('update:selectedEnergyEntries', $event)"
              @entry-changed="emit('entry-changed')"
            />
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <EnergyStatisticsCard
          :vehicle-id="vehicleId"
          :allowed-energy-types="energyTypes"
          :energystats="energystats"
          :stats-loading="statsLoading"
        />
      </v-col>
    </v-row>
  </template>

  <template v-else>
    <v-infinite-scroll :onLoad="loadMore" :items="energyEntries">
      <EnergyEntriesList :items="energyEntries" @delete="openSingleDeleteDialog" />
    </v-infinite-scroll>
  </template>

  <DeleteDialog
    item-to-delete="fuel entry"
    :is-open="showSingleDeleteDialog"
    :on-confirm="confirmSingleDelete"
    :on-cancel="() => (showSingleDeleteDialog = false)"
  />
</template>

<style scoped>
/* Equal height rows */
.equal-height-row {
  align-items: stretch;
}

.equal-height-row .v-col {
  display: flex;
}

.equal-height-row .v-card {
  flex: 1;
}

/* Card background */
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>

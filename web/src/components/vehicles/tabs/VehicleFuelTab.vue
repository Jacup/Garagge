<script setup lang="ts">
import type { EnergyStatsDto, EnergyType } from '@/api/generated/apiV1.schemas'
import EnergyEntriesTable from '@/components/vehicles/EnergyEntriesTable.vue'
import EnergyStatisticsCard from '@/components/vehicles/EnergyStatisticsCard.vue'

interface Props {
  vehicleId: string
  allowedEnergyTypes?: EnergyType[]
  energystats: EnergyStatsDto | null
  statsLoading: boolean
  selectedEnergyEntries: string[]
}

defineProps<Props>()

const emit = defineEmits<{
  'update:selectedEnergyEntries': [value: string[]]
  'entry-changed': []
  'add-entry': []
  'bulk-delete': []
}>()

// Expose energyEntriesTableRef to parent
const energyEntriesTableRef = defineExpose({
  energyEntriesTableRef: null as InstanceType<typeof EnergyEntriesTable> | null
})
</script>

<template>
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
            :allowed-energy-types="allowedEnergyTypes"
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
        :allowed-energy-types="allowedEnergyTypes"
        :energystats="energystats"
        :stats-loading="statsLoading"
      />
    </v-col>
  </v-row>
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

<script setup lang="ts">
import type { EnergyTypeStatsDto } from '@/api/generated/apiV1.schemas'
import { useFormatting } from '@/composables/useFormatting'

const { formatCurrency, formatCurrencyPerUnit, formatConsumption, formatVolume } = useFormatting()

defineProps<{
  stats: EnergyTypeStatsDto
}>()
</script>

<template>
  <v-card variant="flat" rounded="lg" class="mb-2">
    <v-card-title class="d-flex flex-row text-subtitle-1 font-weight-bold pt-3">
      {{ stats.type }}
      <v-spacer />
      <v-chip size="small" color="tertiary" variant="flat" class="suggestion-chip"> {{ stats.itemsCount }} entries </v-chip>
    </v-card-title>
    <v-divider />
    <v-card-text class="py-2 px-4">
      <v-list density="compact" bg-color="transparent">
        <v-list-item
          v-for="item in [
            { label: 'Total cost', value: formatCurrency(stats.totalCost) },
            { label: 'Total volume', value: formatVolume(stats.totalVolume) },
            { label: 'Avg. consumption', value: formatConsumption(stats.averageConsumption) },
            { label: 'Avg. price', value: formatCurrencyPerUnit(stats.averagePricePerUnit) },
            { label: 'Cost / 100 km', value: formatCurrency(stats.averageCostPer100km) },
          ]"
          :key="item.label"
          class="px-0"
        >
          <div class="d-flex justify-space-between align-center">
            <span class="text-body-2 text-medium-emphasis">{{ item.label }}</span>
            <span class="text-body-2 font-weight-bold">{{ item.value }}</span>
          </div>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script lang="ts" setup>
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'

import { useFormatting } from '@/composables/useFormatting'
import { formattingUtils } from '@/utils/formattingUtils'
import { energyUtils } from '@/utils/energyUtils'

interface Props {
  items: EnergyEntryDto[]
}

defineProps<Props>()

const { formatDate, formatMileage } = useFormatting()
const { formatEnergyUnit } = formattingUtils()
const { getFuelIcon, getFuelColor } = energyUtils()

const formatCost = (cost: number | null) =>
  cost ? new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(cost) : ''

const formatUnitCost = (value: number | null): string => {
  if (value == null) return '---'

  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency: 'PLN',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}
</script>

<template>
  <v-list lines="two">
    <v-list-item v-for="entry in items" :key="entry.id" class="list-item">
      <template v-slot:prepend>
        <v-avatar variant="tonal">
          <v-icon :color="getFuelColor(entry.type)">{{ getFuelIcon(entry.type) }}</v-icon>
        </v-avatar>
      </template>

      <v-list-item-title>
        <span>{{ entry.type }} • {{ entry.volume }} {{ formatEnergyUnit(entry.energyUnit) }}</span>
      </v-list-item-title>

      <v-list-item-subtitle class="text-caption text-medium-emphasis">
        {{ formatDate(entry.date) }} • {{ formatMileage(entry.mileage) }}
      </v-list-item-subtitle>

      <template v-slot:append>
        <div class="d-flex flex-column align-end justify-center">
          <span class="text-body-2 font-weight-bold text-high-emphasis">
            {{ formatCost(entry.cost) }}
          </span>

          <span v-if="entry.pricePerUnit" class="text-caption text-disabled" style="font-size: 0.7rem; line-height: 1">
            {{ formatUnitCost(entry.pricePerUnit) }}/{{ formatEnergyUnit(entry.energyUnit) }}
          </span>
        </div>
      </template>
    </v-list-item>
  </v-list>
</template>

<style scoped>
.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
  border-radius: 4px !important;
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

.suggestion-chip {
  border-radius: 8px !important;
  border: 1px solid rgb(var(--v-theme-outline-variant)) !important;
  font-weight: 500 !important;
  line-height: 20px !important;
}
</style>

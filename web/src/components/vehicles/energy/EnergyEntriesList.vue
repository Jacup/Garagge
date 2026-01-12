<script lang="ts" setup>
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'

import { useFormatting } from '@/composables/useFormatting'
import { formattingUtils } from '@/utils/formattingUtils'
import { energyUtils } from '@/utils/energyUtils'
import SwipeableItem from '@/components/common/SwipeableItem.vue'

interface Props {
  items: EnergyEntryDto[]
}

defineProps<Props>()

const emit = defineEmits<{
  // (e: 'select', id: string | undefined): void
  (e: 'delete', id: string | undefined): void
  // (e: 'update:modelValue', ids: string[]): void
}>()

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

const handleDelete = (item: EnergyEntryDto) => {
  emit('delete', item.id)
}
</script>

<template>
  <v-list lines="two" class="list-container" bg-color="transparent">
    <transition-group name="list" tag="div">
      <div
        v-for="(entry, index) in items"
        :key="entry.id"
        class="list-item-wrapper"
        :class="{
          'is-first': index === 0,
          'is-last': index === items.length - 1,
        }"
      >
        <SwipeableItem @delete="handleDelete(entry)">
          <v-list-item :key="entry.id" class="list-item">
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
        </SwipeableItem>
      </div>
    </transition-group>
  </v-list>
</template>

<style scoped lang="scss">
.list-container {
  padding-top: 0px;
  padding-bottom: 0px;
  position: relative;
}
.list-item-wrapper {
  margin-bottom: 2px;
  transition: all 0.4s cubic-bezier(0.55, 0, 0.1, 1);
}
.list-item-wrapper:last-child {
  margin-bottom: 0;
}
.list-item-wrapper :deep(.swipe-content) {
  border-radius: 4px;
}

.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 4px !important;
  transition:
    background-color 0.2s ease,
    border-radius 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.is-first .list-item {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}
.is-first :deep(.swipe-content) {
  border-top-left-radius: 12px;
  border-top-right-radius: 12px;
}

.is-last .list-item {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
}
.is-last :deep(.swipe-content) {
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
}

.list-leave-active {
  position: absolute;
  width: 100%;
  z-index: 0;
  transition: all 0.2s ease;
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

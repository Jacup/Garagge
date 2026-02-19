<script lang="ts" setup>
import { computed } from 'vue'
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'
import { useFormatting } from '@/composables/useFormatting'
import { formattingUtils } from '@/utils/formattingUtils'
import { energyUtils } from '@/utils/energyUtils'
import InteractiveItem from '@/components/common/InteractiveItem.vue'

interface Props {
  items: EnergyEntryDto[]
  modelValue?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => [],
})

const emit = defineEmits<{
  (e: 'select', id: string | undefined): void
  (e: 'delete', id: string | undefined): void
  (e: 'update:modelValue', ids: string[]): void
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

const selectedIds = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
})

const isSelected = (id: string | undefined) => {
  return id ? selectedIds.value.includes(id) : false
}

const toggleSelection = (id: string | undefined) => {
  if (!id) return

  const currentIds = [...selectedIds.value]
  const index = currentIds.indexOf(id)

  if (index === -1) {
    currentIds.push(id)
  } else {
    currentIds.splice(index, 1)
  }

  selectedIds.value = currentIds
}

const handleSelectedUpdate = (item: EnergyEntryDto) => {
  toggleSelection(item.id)
}

const handleRowClick = (item: EnergyEntryDto) => {
  emit('select', item.id)
}

const handleLongPress = (item: EnergyEntryDto) => {
  toggleSelection(item.id)
}

const handleDelete = (item: EnergyEntryDto) => {
  emit('delete', item.id)
}
</script>

<template>
  <v-list lines="two" class="material-list" rounded>
    <transition-group name="list" tag="div">
      <div
        v-for="entry in items"
        :key="entry.id"
        class="list-item-wrapper"
        :class="{
          'material-list__item--selected': isSelected(entry.id),
        }"
      >
        <InteractiveItem
          :selected="isSelected(entry.id)"
          @update:selected="handleSelectedUpdate(entry)"
          @delete="handleDelete(entry)"
          @click="handleRowClick(entry)"
          @long-press="handleLongPress(entry)"
        >
          <template #default="{ selected, onIndicatorClick }">
            <v-list-item link :active="selected">
              <template #prepend>
                <div class="avatar-flip-container mr-3" @click.stop="onIndicatorClick">
                  <div class="avatar-flipper" :class="{ flipped: selected }">
                    <div class="avatar-front">
                      <v-avatar variant="tonal">
                        <v-icon :color="getFuelColor(entry.type)">{{ getFuelIcon(entry.type) }}</v-icon>
                      </v-avatar>
                    </div>
                    <div class="avatar-back">
                      <v-avatar color="secondary" variant="flat">
                        <v-icon icon="mdi-check" color="on-secondary" />
                      </v-avatar>
                    </div>
                  </div>
                </div>
              </template>

              <template #title> {{ entry.type }} • {{ entry.volume }} {{ formatEnergyUnit(entry.energyUnit) }} </template>

              <template #subtitle>
                <div class="text-caption text-medium-emphasis">{{ formatDate(entry.date) }} • {{ formatMileage(entry.mileage) }}</div>
              </template>

              <template #append>
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
          </template>
        </InteractiveItem>
      </div>
    </transition-group>
  </v-list>
</template>

<style scoped lang="scss">
.avatar-flip-container {
  width: 40px;
  height: 40px;
  perspective: 1000px;
  cursor: pointer;
}

.avatar-flipper {
  position: relative;
  width: 100%;
  height: 100%;
  text-align: center;
  transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  transform-style: preserve-3d;
}

.avatar-flipper.flipped {
  transform: rotateY(180deg);
}

.avatar-front,
.avatar-back {
  position: absolute;
  width: 100%;
  height: 100%;
  -webkit-backface-visibility: hidden;
  backface-visibility: hidden;
  top: 0;
  left: 0;
  border-radius: 50%;
}

.avatar-front {
  transform: rotateY(0deg);
  z-index: 2;
}

.avatar-back {
  transform: rotateY(180deg);
  z-index: 1;
}

.list-leave-active {
  position: absolute;
  width: 100%;
  z-index: 0;
  transition: all 0.2s ease;
}

.list-leave-to {
  opacity: 0;
}

.list-move {
  transition: transform 0.4s cubic-bezier(0.55, 0, 0.1, 1);
}
</style>

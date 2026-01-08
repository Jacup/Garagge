<script lang="ts" setup>
import { ref, computed } from 'vue'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { getVehicleIcon } from '@/utils/vehicleUtils'

interface Props {
  items: VehicleDto[]
  loading?: boolean
  modelValue?: string[]
}

interface Emits {
  (e: 'edit', id: string): void
  (e: 'delete', id: string): void
  (e: 'view', id: string): void
  (e: 'update:modelValue', ids: string[]): void
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => [],
})
const emit = defineEmits<Emits>()

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
  if (index === -1) currentIds.push(id)
  else currentIds.splice(index, 1)
  selectedIds.value = currentIds
}

const longPressTimeout = ref<ReturnType<typeof setTimeout> | null>(null)
const isLongPress = ref(false)

const handleTouchStart = (id: string | undefined) => {
  isLongPress.value = false
  if (!id) return

  longPressTimeout.value = setTimeout(() => {
    isLongPress.value = true
    if (navigator.vibrate) navigator.vibrate(20)
    toggleSelection(id)
  }, 500)
}

const handleTouchEnd = () => {
  if (longPressTimeout.value) {
    clearTimeout(longPressTimeout.value)
    longPressTimeout.value = null
  }
}

const handleCardClick = (id: string | undefined) => {
  if (isLongPress.value) {
    isLongPress.value = false
    return
  }
  if (id) emit('view', id)
}

const handleAvatarClick = (id: string | undefined, event: Event) => {
  event.stopPropagation()
  toggleSelection(id)
}
</script>

<template>
  <div class="cards-grid">
    <v-row class="ma-n2">
      <v-col v-for="vehicle in items" :key="vehicle.id" cols="12" sm="6" md="4" lg="3" class="pa-2">
        <v-card
          class="vehicle-card d-flex flex-column"
          :class="{ 'is-selected': isSelected(vehicle.id) }"
          variant="flat"
          height="100%"
          :ripple="false"
          @click="handleCardClick(vehicle.id)"
          @touchstart="handleTouchStart(vehicle.id)"
          @touchend="handleTouchEnd"
          @touchmove="handleTouchEnd"
        >
          <div class="d-flex align-center pt-4 px-4 pb-0">
            <div class="avatar-flip-container mr-4" @click="(e) => handleAvatarClick(vehicle.id, e)">
              <div class="avatar-flipper" :class="{ flipped: isSelected(vehicle.id) }">
                <div class="avatar-front">
                  <v-avatar color="primary" variant="tonal">
                    <v-icon :icon="getVehicleIcon(vehicle.type)" color="primary" />
                  </v-avatar>
                </div>
                <div class="avatar-back">
                  <v-avatar color="secondary" variant="flat">
                    <v-icon icon="mdi-check" color="on-secondary" />
                  </v-avatar>
                </div>
              </div>
            </div>

            <div class="d-flex flex-column" style="min-width: 0">
              <div class="text-subtitle-1 font-weight-bold text-truncate">{{ vehicle.brand }} {{ vehicle.model }}</div>
              <div class="text-caption text-medium-emphasis card-subtitle">
                {{ vehicle.manufacturedYear || '-' }}
              </div>
            </div>
          </div>

          <v-card-text class="pt-3 pb-0">
            <div class="d-flex flex-wrap gap-2">
              <v-chip v-if="vehicle.engineType" class="suggestion-chip" variant="outlined" size="small" density="comfortable">
                <v-icon start icon="mdi-engine" size="x-small"></v-icon>
                {{ vehicle.engineType }}
              </v-chip>
            </div>

            <div v-if="vehicle.vin" class="mt-3 text-caption text-disabled font-monospace d-flex align-center card-vin">
              <v-icon icon="mdi-barcode" size="small" class="mr-1"></v-icon>
              {{ vehicle.vin }}
            </div>
          </v-card-text>

          <v-spacer />

          <v-card-actions class="px-2 pb-2">
            <v-spacer />
            <v-tooltip text="Edit" location="bottom" open-delay="200" close-delay="500">
              <template #activator="{ props }">
                <v-btn
                  v-bind="props"
                  icon="mdi-pencil"
                  variant="text"
                  density="comfortable"
                  color="secondary"
                  class="action-btn"
                  @click.stop="$emit('edit', vehicle.id!)"
                />
              </template>
            </v-tooltip>

            <v-tooltip text="Delete" location="bottom" open-delay="200" close-delay="500">
              <template #activator="{ props }">
                <v-btn
                  v-bind="props"
                  icon="mdi-delete"
                  variant="text"
                  density="comfortable"
                  color="error"
                  class="action-btn"
                  @click.stop="$emit('delete', vehicle.id!)"
                />
              </template>
            </v-tooltip>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<style scoped lang="scss">
.cards-grid {
  width: 100%;
  overflow-x: clip;
  overflow-y: visible;
}

.vehicle-card {
  cursor: pointer;
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 12px !important;
  position: relative;
  overflow: hidden;
  transition:
    background-color 0.2s ease,
    color 0.2s ease;
  user-select: none;
}

.vehicle-card.is-selected {
  background-color: rgb(var(--v-theme-secondary-container)) !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-color: transparent;
}

.vehicle-card.is-selected .card-subtitle,
.vehicle-card.is-selected .card-vin,
.vehicle-card.is-selected .text-disabled {
  color: inherit !important;
  opacity: 0.8;
}

.vehicle-card.is-selected .suggestion-chip {
  background-color: rgba(var(--v-theme-surface), 0.3) !important;
  border-color: rgba(255, 255, 255, 0.2) !important;
}

.font-monospace {
  font-family: 'Roboto Mono', monospace;
  letter-spacing: -0.5px;
}

.suggestion-chip {
  border-radius: 8px !important;
  border: 1px solid rgb(var(--v-theme-outline-variant)) !important;
  font-weight: 500 !important;
  line-height: 20px !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.gap-2 {
  gap: 8px;
}

.avatar-flip-container {
  width: 40px;
  height: 40px;
  perspective: 1000px;
  cursor: pointer;
  z-index: 2;
}

.avatar-flipper {
  position: relative;
  width: 100%;
  height: 100%;
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
</style>

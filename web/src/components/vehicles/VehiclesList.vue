<script lang="ts" setup>
import { ref, computed, onUnmounted } from 'vue'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { vehicleUtils } from '@/utils/vehicleUtils'
import SwipeableItem from '@/components/common/SwipeableItem.vue'

interface Props {
  items: VehicleDto[]
  showDetails: boolean
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

const { getVehicleIcon } = vehicleUtils()

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

const handleAvatarClick = (item: VehicleDto, event: Event) => {
  event.stopPropagation()
  toggleSelection(item.id)
}

const longPressTimeout = ref<ReturnType<typeof setTimeout> | null>(null)
const isLongPress = ref(false)

const handleTouchStart = (item: VehicleDto) => {
  isLongPress.value = false
  longPressTimeout.value = setTimeout(() => {
    isLongPress.value = true
    if (navigator.vibrate) navigator.vibrate(20)
    toggleSelection(item.id)
  }, 400)
}

const handleTouchEnd = () => {
  if (longPressTimeout.value) {
    clearTimeout(longPressTimeout.value)
    longPressTimeout.value = null
  }
}

const handleTouchMove = () => {
  if (longPressTimeout.value) {
    clearTimeout(longPressTimeout.value)
    longPressTimeout.value = null
  }
}

const handleRowClick = (item: VehicleDto) => {
  if (isLongPress.value) {
    isLongPress.value = false
    return
  }

  emit('select', item.id)
}

const handleDelete = (item: VehicleDto) => {
  emit('delete', item.id)
}

onUnmounted(() => {
  if (longPressTimeout.value) {
    clearTimeout(longPressTimeout.value)
  }
})
</script>

<template>
  <v-list lines="two" class="list-container" bg-color="transparent">
    <transition-group name="list" tag="div">
      <div
        v-for="(record, index) in items"
        :key="record.id"
        class="list-item-wrapper"
        :class="{
          'is-first': index === 0,
          'is-last': index === items.length - 1,
          'is-selected': isSelected(record.id),
        }"
      >
        <SwipeableItem @delete="handleDelete(record)" @click="handleRowClick(record)">
          <v-list-item
            class="list-item"
            link
            :active="isSelected(record.id)"
            @touchstart="handleTouchStart(record)"
            @touchend="handleTouchEnd"
            @touchmove="handleTouchMove"
            @mousedown="handleTouchEnd"
          >
            <template #prepend>
              <div class="avatar-flip-container mr-3" @click="(e) => handleAvatarClick(record, e)">
                <div class="avatar-flipper" :class="{ flipped: isSelected(record.id) }">
                  <div class="avatar-front">
                    <v-avatar color="primary" variant="tonal">
                      <v-icon :icon="getVehicleIcon(record.type)" color="primary" />
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

            <template #title> {{ record.brand }} {{ record.model }} </template>
            <template #subtitle v-if="showDetails"> {{ record.manufacturedYear }} </template>
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

.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 4px !important;
  transition:
    background-color 0.2s ease,
    border-radius 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  user-select: none;
}

.is-selected .list-item {
  background-color: rgba(var(--v-theme-secondary-container), 1) !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-radius: 12px !important;
}

.is-selected :deep(.swipe-content) {
  border-radius: 12px !important;
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

.list-item-wrapper :deep(.swipe-content) {
  border-radius: 4px;
}

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

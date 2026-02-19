<script lang="ts" setup>
import { ref, computed, onUnmounted } from 'vue'

interface Props {
  selected?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  selected: false,
})

const emit = defineEmits<{
  (e: 'delete'): void
  (e: 'click'): void
  (e: 'long-press'): void
  (e: 'update:selected', value: boolean): void
}>()

const startX = ref(0)
const startY = ref(0)
const currentX = ref(0)
const isSwiping = ref(false)
const isLongPress = ref(false)
const actionThreshold = -100

let longPressTimeout: ReturnType<typeof setTimeout> | null = null
let deleteAnimationTimeout: ReturnType<typeof setTimeout> | null = null
let resetPositionTimeout: ReturnType<typeof setTimeout> | null = null

const isThresholdMet = computed(() => currentX.value < actionThreshold)
const indicatorWidth = computed(() => Math.max(0, Math.abs(currentX.value) - 2))
const iconOpacity = computed(() => (isThresholdMet.value ? 1 : 0))
const iconScale = computed(() => (isThresholdMet.value ? 1.2 : 0.5))

function onIndicatorClick() {
  emit('update:selected', !props.selected)
}

function clearLongPress() {
  if (longPressTimeout) {
    clearTimeout(longPressTimeout)
    longPressTimeout = null
  }
}

function onTouchStart(e: TouchEvent) {
  startX.value = e.touches[0].clientX
  startY.value = e.touches[0].clientY
  isSwiping.value = true
  isLongPress.value = false

  longPressTimeout = setTimeout(() => {
    isLongPress.value = true
    if (navigator.vibrate) navigator.vibrate(20)
    emit('long-press')
  }, 400)
}

function onTouchMove(e: TouchEvent) {
  if (!isSwiping.value) return

  const touchX = e.touches[0].clientX
  const touchY = e.touches[0].clientY
  const deltaX = touchX - startX.value
  const deltaY = touchY - startY.value
  const startThreshold = 10

  if (currentX.value === 0) {
    if (deltaX > 0) {
      clearLongPress()
      return
    }

    if (Math.abs(deltaY) > Math.abs(deltaX)) {
      clearLongPress()
      isSwiping.value = false
      return
    }

    if (Math.abs(deltaX) < startThreshold) return
  }

  if (deltaX < 0) {
    clearLongPress()
    currentX.value = deltaX
  }
}

function onTouchEnd() {
  clearLongPress()
  isSwiping.value = false

  if (isThresholdMet.value) {
    currentX.value = -window.innerWidth * 1.5

    deleteAnimationTimeout = setTimeout(() => {
      emit('delete')
      resetPositionTimeout = setTimeout(() => {
        currentX.value = 0
      }, 100)
    }, 300)
  } else {
    currentX.value = 0
  }
}

function onClick() {
  if (currentX.value === 0 && !isLongPress.value) {
    emit('click')
  }
  isLongPress.value = false
}

onUnmounted(() => {
  clearLongPress()
  if (deleteAnimationTimeout) clearTimeout(deleteAnimationTimeout)
  if (resetPositionTimeout) clearTimeout(resetPositionTimeout)
})
</script>

<template>
  <div class="interactive-container">
    <div
      class="swipe-indicator d-flex align-center justify-center bg-error text-on-error"
      :class="{ 'is-animating': !isSwiping }"
      :style="{ width: `${indicatorWidth}px` }"
    >
      <v-icon
        icon="mdi-delete-outline"
        :style="{
          opacity: iconOpacity,
          transform: `scale(${iconScale})`,
          transition: 'opacity 0.1s, transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1)',
        }"
      />
    </div>

    <div
      class="swipe-content"
      :class="{ 'is-animating': !isSwiping, 'is-active': Math.abs(currentX) > 0 }"
      :style="{ transform: `translateX(${currentX}px)` }"
      @touchstart="onTouchStart"
      @touchmove="onTouchMove"
      @touchend="onTouchEnd"
      @click="onClick"
    >
      <slot :selected="selected" :on-indicator-click="onIndicatorClick" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.interactive-container {
  position: relative;
  overflow: visible;
}

.swipe-indicator {
  position: absolute;
  top: 0;
  bottom: 0;
  right: 0;
  height: 100%;
  z-index: 0;
  max-width: 100%;
  border-radius: 999px;
  overflow: hidden;
}

.swipe-content {
  position: relative;
  z-index: 1;
  background-color: transparent;
  touch-action: pan-y;
  user-select: none;
}

.is-active {
  z-index: 100 !important;
}

.is-animating {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.5, 1);
}
</style>

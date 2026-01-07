<script lang="ts" setup>
import { ref, computed } from 'vue'

const emit = defineEmits<{
  (e: 'delete'): void
  (e: 'click'): void
}>()

const startX = ref(0)
const startY = ref(0)
const currentX = ref(0)
const isSwiping = ref(false)
const actionThreshold = -100
const startThreshold = 10

const indicatorWidth = computed(() => Math.max(0, Math.abs(currentX.value) - 2))

const iconOpacity = computed(() => {
  const width = indicatorWidth.value
  if (width < 30) return 0
  return Math.min((width - 30) / 50, 1)
})

const iconScale = computed(() => {
  const width = indicatorWidth.value
  if (width < 30) return 0.5
  return Math.min(0.8 + ((width - 30) / 90) * 0.4, 1.2)
})

function onTouchStart(e: TouchEvent) {
  startX.value = e.touches[0].clientX
  startY.value = e.touches[0].clientY
  isSwiping.value = true
}

function onTouchMove(e: TouchEvent) {
  if (!isSwiping.value) return

  const touchX = e.touches[0].clientX
  const touchY = e.touches[0].clientY

  const deltaX = touchX - startX.value
  const deltaY = touchY - startY.value

  if (currentX.value === 0) {
    if (deltaX > 0) return

    if (Math.abs(deltaY) > Math.abs(deltaX)) {
      isSwiping.value = false
      return
    }

    if (Math.abs(deltaX) < startThreshold) {
      return
    }
  }

  if (deltaX < 0) {
    currentX.value = deltaX
  }
}

function onTouchEnd() {
  isSwiping.value = false

  if (currentX.value < actionThreshold) {
    currentX.value = -window.innerWidth * 1.5

    setTimeout(() => {
      emit('delete')
      setTimeout(() => {
        currentX.value = 0
      }, 100)
    }, 300)
  } else {
    currentX.value = 0
  }
}

function onClick() {
  if (currentX.value === 0) {
    emit('click')
  }
}
</script>

<template>
  <div class="swipe-container">
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
          transition: 'opacity 0.1s, transform 0.1s',
        }"
      ></v-icon>
    </div>

    <div
      ref="content"
      class="swipe-content"
      :class="{ 'is-animating': !isSwiping, 'is-active': Math.abs(currentX) > 0 }"
      :style="{ transform: `translateX(${currentX}px)` }"
      @touchstart="onTouchStart"
      @touchmove="onTouchMove"
      @touchend="onTouchEnd"
      @click="onClick"
    >
      <slot></slot>
    </div>
  </div>
</template>

<style scoped lang="scss">
.swipe-container {
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
}

.is-active {
  z-index: 100 !important;
}

.is-animating {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.5, 1);
}
</style>

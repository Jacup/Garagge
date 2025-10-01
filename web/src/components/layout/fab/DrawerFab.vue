<script lang="ts" setup>
import { computed } from 'vue'
import { useLayoutFab } from '@/composables/useLayoutFab'

/**
 * Floating Action Button component for navigation drawer
 * Uses new simplified layout logic - no need for isRail prop
 * Layout system automatically determines display mode
 */

const { fabConfig, isDrawerExtended, isDrawerCompact } = useLayoutFab()

const commonButtonProps = computed(() => ({
  variant: 'flat' as const,
  rounded: 'md-16px' as const,
  color: 'tertiary-container' as const,
}))

const handleFabClick = () => {
  if (fabConfig.value?.action) {
    fabConfig.value.action()
  } else {
    console.warn('DrawerFab: No action defined for FAB')
  }
}

const fabAriaLabel = computed(() =>
  fabConfig.value?.text ? `${fabConfig.value.text} button` : 'Action button'
)
</script>

<template>
  <!-- Extended FAB for desktop mode -->
  <div v-if="isDrawerExtended" class="drawer-fab-container">
    <v-btn
      v-bind="commonButtonProps"
      height="56px"
      class="drawer-extended-btn"
      :aria-label="fabAriaLabel"
      :disabled="!fabConfig"
      @click="handleFabClick"
    >
      <template #prepend>
        <v-icon :size="24">{{ fabConfig?.icon }}</v-icon>
      </template>
      <template #default>
        <span class="fab-text">{{ fabConfig?.text }}</span>
      </template>
    </v-btn>
  </div>

  <!-- Compact FAB for tablet/rail mode -->
  <div v-else-if="isDrawerCompact" class="rail-fab-container">
    <v-btn
      v-bind="commonButtonProps"
      size="56px"
      class="rail-compact-btn"
      :aria-label="fabAriaLabel"
      :disabled="!fabConfig"
      @click="handleFabClick"
    >
      <v-icon :size="24">{{ fabConfig?.icon }}</v-icon>
    </v-btn>
  </div>
</template>

<style lang="scss" scoped>
.drawer-fab-container,
.rail-fab-container {
  --fab-padding-vertical: 8px;
  --fab-padding-bottom: 4px;
}

.drawer-fab-container {
  --fab-padding-horizontal: 16px;

  padding: var(--fab-padding-vertical) var(--fab-padding-horizontal) var(--fab-padding-bottom);
  display: flex;
  justify-content: flex-start;
}

.rail-fab-container {
  --fab-padding-horizontal: 8px;

  padding: var(--fab-padding-vertical) var(--fab-padding-horizontal) var(--fab-padding-bottom);
  display: flex;
  justify-content: center;
}

.fab-text {
  font-family: 'Roboto', sans-serif;
  font-weight: 500;
  font-size: 16px;
  line-height: 24px;
  letter-spacing: 0.15px;
}
</style>

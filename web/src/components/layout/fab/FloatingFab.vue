<script lang="ts" setup>
import { computed } from 'vue'
import { useLayoutFab } from '@/composables/useLayoutFab'

/**
 * Floating Action Button for mobile layout
 * Displays as fixed position floating button
 */

const { fabConfig, shouldShowFloating } = useLayoutFab()

const handleFabClick = () => {
  if (fabConfig.value?.action) {
    fabConfig.value.action()
  } else {
    console.warn('FloatingFab: No action defined for FAB')
  }
}

const fabAriaLabel = computed(() =>
  fabConfig.value?.text ? `${fabConfig.value.text} button` : 'Action button'
)
</script>

<template>
  <Teleport to="body">
    <v-fab
      v-if="shouldShowFloating"
      :icon="fabConfig?.icon"
      color="tertiary-container"
      size="80"
      location="bottom end"
      :aria-label="fabAriaLabel"
      :disabled="!fabConfig"
      class="floating-fab"
      @click="handleFabClick"
    />
  </Teleport>
</template>

<style lang="scss" scoped>
.floating-fab {
  --fab-margin: 16px;
  --bottom-nav-height: 64px;

  position: fixed !important;
  right: var(--fab-margin);
  z-index: 1001; /* Above bottom navigation (z-index: 1000) */

  /* Default positioning (desktop/tablet) */
  bottom: var(--fab-margin);

  /* Mobile - position above bottom navigation */
  @media (max-width: 959px) {
    bottom: calc(var(--bottom-nav-height) + var(--fab-margin));
  }

  /* Larger icon for better touch target */
  :deep(.v-icon) {
    font-size: 28px !important;
  }
}
</style>

<script lang="ts" setup>
import { computed } from 'vue'
import { useLayoutFab } from '@/composables/useLayoutFab'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import RegularFab from './RegularFab.vue'
import FabMenu from './FabMenu.vue'

const { fabConfig } = useLayoutFab()
const { mode } = useResponsiveLayout()

const isExtendedFab = computed(() => mode.value === 'desktop' && fabConfig.value?.type === 'regular')

// Create unique key for transition - changes on every FAB config change
const fabKey = computed(() => {
  if (!fabConfig.value) return ''
  return `${fabConfig.value.type}-${fabConfig.value.icon}-${fabConfig.value.text}`
})
</script>

<template>
  <Teleport to="body">
    <Transition name="fab-appear" mode="out-in">
      <div v-if="fabConfig" class="floating-action-button" :key="fabKey">
        <FabMenu v-if="fabConfig.type === 'menu'" :icon="fabConfig.icon" :text="fabConfig.text" :menu-items="fabConfig.menuItems" />

        <RegularFab
          v-else-if="fabConfig.type === 'regular'"
          :icon="fabConfig.icon"
          :text="fabConfig.text"
          :is-extended="isExtendedFab"
          @click="fabConfig.action"
        />
      </div>
    </Transition>
  </Teleport>
</template>

<style lang="scss" scoped>
.floating-action-button {
  --fab-margin: 24px;
  --fab-margin-mobile: 16px;
  --bottom-nav-height: 64px;

  position: fixed;
  right: var(--fab-margin);
  bottom: var(--fab-margin);
  z-index: 1001;

  @media (max-width: 959px) {
    right: var(--fab-margin-mobile);
    bottom: calc(var(--bottom-nav-height) + var(--fab-margin-mobile));
  }
}

.fab-appear-enter-active {
  animation: fab-spring-in 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  transform-origin: bottom right;
}

.fab-appear-leave-active {
  transition: all 0.15s cubic-bezier(0.4, 0, 0.2, 1);
  transform-origin: bottom right;
}

.fab-appear-enter-from {
  opacity: 0;
  transform: scale(0);
}

.fab-appear-enter-to {
  opacity: 1;
  transform: scale(1);
}

.fab-appear-leave-from {
  opacity: 1;
  transform: scale(1);
}

.fab-appear-leave-to {
  opacity: 0;
  transform: scale(0);
}

@keyframes fab-spring-in {
  0% {
    opacity: 0;
    transform: scale(0);
  }
  50% {
    opacity: 1;
  }
  70% {
    transform: scale(1.02);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}
</style>

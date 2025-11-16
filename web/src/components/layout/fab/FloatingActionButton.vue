<script lang="ts" setup>
import { computed } from 'vue'
import { useLayoutFab } from '@/composables/useLayoutFab'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import RegularFab from './RegularFab.vue'
import FabMenu from './FabMenu.vue'

const { fabConfig } = useLayoutFab()
const { mode } = useResponsiveLayout()

const isExtendedFab = computed(() => mode.value === 'desktop' && fabConfig.value?.type === 'regular')
</script>

<template>
  <Teleport to="body">
    <div v-if="fabConfig" class="floating-action-button">
      <FabMenu
        v-if="fabConfig.type === 'menu'"
        :icon="fabConfig.icon"
        :text="fabConfig.text"
        :menu-items="fabConfig.menuItems"
      />

      <RegularFab
        v-else-if="fabConfig.type === 'regular'"
        :icon="fabConfig.icon"
        :text="fabConfig.text"
        :is-extended="isExtendedFab"
        @click="fabConfig.action"
      />
    </div>
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
</style>

<script lang="ts" setup>
import { MAIN_NAVIGATION_ITEMS } from '@/constants/navigation'
import NavigationButton from './NavigationButton.vue'

interface Props {
  layout?: 'vertical' | 'horizontal'
}

const { layout = 'vertical' } = defineProps<Props>()
</script>

<template>
  <nav :class="['m3-bottom-navigation', `m3-bottom-navigation--${layout}`]" role="navigation" aria-label="Mobile navigation">
    <div class="nav-container">
      <NavigationButton v-for="item in MAIN_NAVIGATION_ITEMS" :key="item.title" :item="item" :layout="layout" />
    </div>
  </nav>
</template>

<style scoped>
.m3-bottom-navigation {
  background-color: rgb(var(--v-theme-surface-container, var(--v-theme-surface)));
  height: 64px;
  border-radius: 0;
  box-shadow: none;

  width: 100%;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1000;

  padding-bottom: env(safe-area-inset-bottom);
}

.nav-container {
  display: flex;
  height: 100%;
  align-items: center;
  padding: 6px 8px;
}

.m3-bottom-navigation--vertical .nav-container {
  justify-content: space-evenly;
}

.m3-bottom-navigation--horizontal .nav-container {
  justify-content: center;
  gap: 8px;
}
</style>

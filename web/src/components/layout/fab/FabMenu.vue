<script lang="ts" setup>
import { ref } from 'vue'
import type { FabMenuItem } from '@/composables/useLayoutFab'

/**
 * FAB Menu - v-speed-dial with multiple actions
 */

interface Props {
  icon: string
  text: string
  menuItems: readonly FabMenuItem[]
}

const { menuItems } = defineProps<Props>()

const speedDialOpen = ref(false)
</script>

<template>
  <div class="fab-menu-container">
    <v-speed-dial
      v-model="speedDialOpen"
      transition="none"
      location="top end"
    >
      <template v-slot:activator="{ props: activatorProps, isActive }">
        <v-fab
          v-bind="activatorProps"
          :icon="isActive ? 'mdi-close' : icon"
          :color="isActive ? 'tertiary' : 'tertiary-container'"
          :size="isActive ? 56 : 80"
          :rounded="isActive ? 'circle' : 'xl'"
          :aria-label="`${text} button`"
          :class="{ 'fab-menu-active': isActive }"
        />
      </template>

      <v-btn
        v-for="(item, index) in menuItems"
        :key="item.key"
        :prepend-icon="item.icon"
        :text="item.text"
        color="tertiary-container"
        :aria-label="item.text"
        height="56"
        rounded="pill"
        class="menu-item-btn"
        :class="{ 'menu-item-exit': !speedDialOpen }"
        :style="{ '--item-index': menuItems.length - 1 - index }"
        @click="item.action"
      />
    </v-speed-dial>
  </div>
</template>

<style lang="scss" scoped>
// Fixed 80x80 container for consistent positioning
.fab-menu-container {
  width: 80px;
  height: 80px;
  position: relative;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
}

// Default FAB icon size
:deep(.v-btn .v-icon) {
  font-size: 28px !important;
  transition: font-size 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

// Close button (active state) - 56x56, circle, icon 20px, aligned to top-right
.fab-menu-active {
  :deep(.v-icon) {
    font-size: 20px !important;
  }
}// Align all wrappers to top-right corner of 80x80 container
:deep(.v-speed-dial),
:deep(.v-speed-dial > div),
:deep(.v-fab) {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
}

// v-fab__container - override Vuetify's center alignment
:deep(.v-fab__container) {
  display: flex !important;
  align-items: flex-start !important;
  justify-content: flex-end !important;
  align-self: flex-start !important;
}:deep(.v-fab .v-btn) {
  margin: 0 !important;
}

// Speed dial list - menu items appear from top-right, 8px gap from close button
:deep(.v-speed-dial__list) {
  flex-direction: column-reverse;
  align-items: flex-end;
  transform-origin: top right;
  margin-top: 8px;
}

// Menu item buttons - dynamic width, 24px padding, pill shape, icon 24px
.menu-item-btn {
  width: fit-content !important;
  min-width: 0 !important;
  padding-inline: 24px !important;
  flex: 0 0 auto !important;
  display: inline-flex !important;
  transform-origin: right center;

  // Enter animation - expand from right, staggered from bottom
  animation: fab-item-enter 0.2s cubic-bezier(0.4, 0, 0.2, 1) both;
  animation-delay: calc(var(--item-index) * 0.05s);

  :deep(.v-icon) {
    font-size: 24px !important;
  }

  :deep(.v-btn__prepend) {
    margin-inline-start: 0;
    margin-inline-end: 8px;
  }

  :deep(.v-btn__content) {
    white-space: nowrap;
  }
}

// Exit animation - reverse of enter, collapse to right
.menu-item-exit {
  animation: fab-item-exit 0.15s cubic-bezier(0.4, 0, 0.2, 1) both;
  // Reverse stagger - top items exit first
  animation-delay: calc((var(--item-index)) * 0.03s);
}

@keyframes fab-item-enter {
  0% {
    opacity: 0;
    transform: scaleX(0.3) translateY(8px);
  }
  50% {
    opacity: 1;
  }
  70% {
    transform: scaleX(1.02) translateY(0);
  }
  100% {
    opacity: 1;
    transform: scaleX(1) translateY(0);
  }
}

@keyframes fab-item-exit {
  from {
    opacity: 1;
    transform: translateY(0);
  }
  to {
    opacity: 0;
    transform: translateY(8px);
  }
}
</style>

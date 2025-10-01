<script lang="ts" setup>
import { MAIN_NAVIGATION_ITEMS } from '@/constants/navigation'
import { useLayoutFab } from '@/composables/useLayoutFab'
import DrawerNavigationItem from './DrawerNavigationItem.vue'
import NavigationButton from './NavigationButton.vue'
import ServerStatus from './ServerStatus.vue'

interface Props {
  isRail?: boolean
}

const { isRail = false } = defineProps<Props>()
const { fabConfig, shouldShowInDrawer } = useLayoutFab()

const emit = defineEmits<{
  navigate: []
}>()
</script>

<template>
  <div class="drawer-navigation" :class="{ 'drawer-navigation--rail': isRail }">
    <div class="nav-logo"></div>

    <!-- Extended FAB positioned at top for desktop -->
    <div v-if="!isRail && shouldShowInDrawer('desktop')" class="drawer-fab-container">
      <v-btn
        variant="flat"
        height="56px"
        rounded="md-16px"
        color="tertiary-container"
        class="drawer-extended-btn"
        @click="fabConfig?.action"
      >
        <template #prepend>
          <v-icon :size="24">{{ fabConfig?.icon }}</v-icon>
        </template>
        <template #default>
          <span class="fab-text">{{ fabConfig?.text }}</span>
        </template>
      </v-btn>
    </div>

    <!-- Compact FAB for rail mode -->
    <div v-else-if="isRail && shouldShowInDrawer('tablet')" class="rail-fab-container">
      <v-btn variant="flat" size="56px" rounded="md-16px" color="tertiary-container" class="rail-compact-btn" @click="fabConfig?.action">
        <v-icon :size="24">{{ fabConfig?.icon }}</v-icon>
      </v-btn>
    </div>

    <div class="main-nav">
      <v-list nav class="px-4" base-color="on-surface-variant" active-color="secondary">
        <!-- Drawer mode -->
        <template v-if="!isRail">
          <DrawerNavigationItem v-for="item in MAIN_NAVIGATION_ITEMS" :key="item.title" :item="item" @navigate="emit('navigate')" />
        </template>

        <!-- Rail mode -->
        <template v-else>
          <NavigationButton v-for="item in MAIN_NAVIGATION_ITEMS" :key="item.title" :item="item" @navigate="emit('navigate')" />
        </template>
      </v-list>
    </div>

    <ServerStatus v-if="!isRail" />
  </div>
</template>

<style lang="scss" scoped>
.drawer-navigation {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 32px);
  box-sizing: border-box;
  margin: 16px 0 16px 16px;
  overflow: hidden;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
  gap: 8px;

  :deep(.v-list-item__prepend) {
    width: 32px;
  }

  :deep(.v-list-item) {
    width: fit-content;
    min-width: auto;

    &.v-list-item--active {
      background-color: rgb(var(--v-theme-secondary-container)) !important;

      .v-icon {
        color: rgb(var(--v-theme-on-secondary-container)) !important;
      }
    }

    &:hover:not(.v-list-item--active) {
      background-color: rgba(var(--v-theme-on-surface-variant), 0.08);
    }

    &.v-list-item--active:hover {
      background-color: color-mix(
        in srgb,
        rgb(var(--v-theme-secondary-container)),
        rgba(var(--v-theme-on-secondary-container), 0.08)
      ) !important;
    }
  }
}

.nav-logo {
  height: 64px;
}

.main-nav {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

/* Rail mode adjustments */
.drawer-navigation--rail {
  align-items: center;
  gap: 8px; /* Match expanded gap */

  .main-nav {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 0 8px; /* Smaller horizontal padding for rail */
  }
}

.drawer-fab-container {
  padding: 8px 16px 4px;
  display: flex;
  justify-content: flex-start;
}

/* FAB in rail drawer positioning */
.rail-fab-container {
  padding: 8px 8px 4px; /* Same vertical padding as expanded, smaller horizontal for rail */
  display: flex;
  justify-content: center;
}

/* FAB text styling according to MD3 guidelines */
.fab-text {
  font-family: 'Roboto', sans-serif;
  font-weight: 500;
  font-size: 16px;
  line-height: 24px;
  letter-spacing: 0.15px;
}
</style>

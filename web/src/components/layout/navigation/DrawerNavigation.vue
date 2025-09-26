<script lang="ts" setup>
import { MAIN_NAVIGATION_ITEMS } from '@/constants/navigation'
import DrawerNavigationItem from './DrawerNavigationItem.vue'
import NavigationButton from './NavigationButton.vue'
import ServerStatus from './ServerStatus.vue'

interface Props {
  isRail?: boolean
}

const { isRail = false } = defineProps<Props>()

const emit = defineEmits<{
  navigate: []
}>()
</script>

<template>
  <div class="drawer-navigation" :class="{ 'drawer-navigation--rail': isRail }">
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

.main-nav {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

/* Rail mode adjustments */
.drawer-navigation--rail {
  align-items: center;
  padding: 44px 8px 8px;

  .main-nav {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
  }
}
</style>

<script lang="ts" setup>
import { MAIN_NAVIGATION_ITEMS } from '@/constants/navigation'
import { DrawerNavigationItem, NavigationButton, ServerStatus } from '.'

interface Props {
  isRail?: boolean
}

const { isRail = false } = defineProps<Props>()

const emit = defineEmits<{
  navigate: [void]
}>()
</script>

<template>
  <div class="drawer-navigation" :class="{ 'drawer-navigation--rail': isRail }">
    <div class="nav-logo"></div>

    <div class="main-nav">
      <v-list nav class="px-4" base-color="on-surface-variant" color="secondary">
        <component
          :is="isRail ? NavigationButton : DrawerNavigationItem"
          v-for="item in MAIN_NAVIGATION_ITEMS"
          :key="item.title"
          :item="item"
          @navigate="emit('navigate')"
        />
      </v-list>
    </div>

    <ServerStatus v-if="!isRail" />
  </div>
</template>

<style lang="scss" scoped>
.drawer-navigation {
  --nav-margin: 16px;
  --nav-border-radius: 12px;
  --nav-gap: 8px;
  --nav-height: calc(100vh - 32px);
  --logo-height: 64px;
  --rail-padding: 8px;

  display: flex;
  flex-direction: column;
  height: var(--nav-height);
  box-sizing: border-box;
  margin: var(--nav-margin) 0 var(--nav-margin) var(--nav-margin);
  overflow: hidden;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: var(--nav-border-radius);
  gap: var(--nav-gap);

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
  height: var(--logo-height);
}

.main-nav {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

.drawer-navigation--rail {
  align-items: center;
  gap: var(--nav-gap);

  .main-nav {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 0 var(--rail-padding);
  }
}
</style>

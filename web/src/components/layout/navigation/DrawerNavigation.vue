<script lang="ts" setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { MAIN_NAVIGATION_ITEMS } from '@/constants/navigation'
import ServerInfo from './ServerInfo.vue'

interface Props {
  isRail?: boolean
}

const { isRail = false } = defineProps<Props>()

const router = useRouter()

const emit = defineEmits<{
  navigate: []
}>()

const currentRoute = computed(() => router.currentRoute.value.path)

const isActive = (item: (typeof MAIN_NAVIGATION_ITEMS)[0]) => {
  return currentRoute.value === item.link
}
</script>

<template>
  <div class="drawer-navigation">
    <div class="main-nav">
      <v-list nav class="px-4" base-color="on-surface-variant" active-color="secondary">
        <v-list-item
          v-for="item in MAIN_NAVIGATION_ITEMS"
          :key="item.title"
          :title="item.title"
          :to="item.link"
          :min-height="56"
          rounded="pill"
          class="px-4"
          base-color="on-surface-variant"
          active-color="secondary"
          link
          @click="emit('navigate')"
        >
          <template #prepend>
            <v-icon :size="24" class="mr-2">{{ isActive(item) ? item.activeIcon : item.icon }}</v-icon>
          </template>
        </v-list-item>
      </v-list>
    </div>
    <div class="system-navigation">
      <v-list nav class="px-4" base-color="on-surface-variant" active-color="secondary">
        <v-list-item
          :key="'Server Online'"
          :title="'Server Online'"
          :min-height="48"
          rounded="pill"
          class="px-4 server-status-item"
          base-color="on-surface-variant"
          active-color="secondary"
          :ripple="false"
          link
          variant="tonal"
        >
          <template #prepend>
            <v-badge location="top right" :offset-y="-5" :offset-x="3" color="success" dot>
              <v-icon :size="24" class="mr-2">mdi-server</v-icon>
            </v-badge>
          </template>
          <template #append>
            <v-chip size="x-small" flat rounded="pill" class="roboto-mono">v1.2.0</v-chip>
          </template>
        </v-list-item>
      </v-list>
    </div>
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

.system-navigation {
  flex: 0 0 auto;
  margin-top: auto;
}

.server-info {
  display: flex;
  flex-direction: row;
  align-items: center;
}

.drawer-navigation--rail {
  align-items: center;
}

.nav-header {
  display: flex;
  flex: 0 0 auto;
  flex-direction: row;
  height: 64px;
  padding: 16px;
  align-items: center;
  justify-content: space-between;
}

.drawer-navigation--rail .nav-header {
  justify-content: center;
  padding: 16px 8px;
}

.logo-btn-expanded {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  color: rgb(var(--v-theme-on-primary-container));
}

.logo-btn-expanded:focus {
  outline: 2px solid rgb(var(--v-theme-on-primary-container));
  outline-offset: 2px;
}

.logo-btn-expanded .v-icon {
  color: inherit;
}

.logo-btn-expanded span {
  font-family: 'Bitcount Grid Single', monospace;
  font-weight: 600;
  font-size: 1.2rem;
}

.logo-btn-rail {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  color: rgb(var(--v-theme-on-primary-container));
}

.logo-btn-rail:focus {
  outline: 2px solid rgb(var(--v-theme-on-primary-container));
  outline-offset: 2px;
}

.logo-btn-rail .v-icon {
  color: inherit;
}

.status-indicator {
  overflow: visible;
  position: relative;
  width: 36px;
  height: 36px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

.nav-menu {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

.nav-footer {
  flex: 0 0 auto;
}

/* ==============================================
   RAIL NAVIGATION - MD3 EXPRESSIVE STYLE
   ============================================== */

.rail-navigation {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 8px;
  width: 100%;
}

.rail-nav-item {
  /* Reset button styles */
  background: none;
  border: none;
  border-radius: 0;
  cursor: pointer;
  text-decoration: none;

  /* MD3 Colors: Inactive */
  color: rgb(var(--v-theme-on-surface-variant));

  /* Layout - Stacked (icon above text) */
  /* display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center; */

  /* Fixed dimensions for rail items */
  /* width: 56px;
  min-height: 56px;
  padding: 8px 4px; */

  /* Smooth transitions */
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Active Rail Item */
.rail-nav-item--active {
  color: rgb(var(--v-theme-secondary)); /* MD3: Active text */
}

/* ==============================================
   RAIL ICON CONTAINERS (PILL SHAPES)
   ============================================== */

.rail-nav-item__icon-container {
  /* display: flex;
  align-items: center;
  justify-content: center; */

  /* MD3 Expressive - Pill shape */
  /* width: 32px;
  height: 32px;
  border-radius: 16px;
  margin-bottom: 4px; */

  /* Smooth transitions */
  transition: all 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Active Icon Container - Pill Background */
.rail-nav-item--active .rail-nav-item__icon-container {
  background-color: rgb(var(--v-theme-secondary-container)); /* MD3: Secondary container */
}

/* ==============================================
   RAIL ICONS & LABELS
   ============================================== */

/* Rail Icon Styling */
.rail-nav-item .v-icon {
  font-size: 24px; /* MD3: 24dp icon size */
  color: rgb(var(--v-theme-on-surface-variant)); /* MD3: Inactive icon */
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Active Rail Icon */
.rail-nav-item--active .v-icon {
  color: rgb(var(--v-theme-on-secondary-container)); /* MD3: Active icon */
}

/* Rail Label Styling */
.rail-nav-item__label {
  font-size: 12px; /* MD3: Label typography */
  font-weight: 500;
  line-height: 16px;
  text-align: center;
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);

  /* Wrap long text */
  word-break: break-word;
  max-width: 48px;
}

/* ==============================================
   RAIL INTERACTION STATES (PILL ONLY)
   ============================================== */

/* Hover State - 8% state layer */
.rail-nav-item--active:hover .rail-nav-item__icon-container {
  background-color: color-mix(in srgb, rgb(var(--v-theme-secondary-container)), rgba(var(--v-theme-on-secondary-container), 0.08));
}

/* Focus State - 12% state layer */
.rail-nav-item:focus-visible .rail-nav-item__icon-container {
  background-color: color-mix(in srgb, rgb(var(--v-theme-secondary-container)), rgba(var(--v-theme-on-secondary-container), 0.12));
  outline: 2px solid rgb(var(--v-theme-secondary));
  outline-offset: 2px;
}

/* Pressed State - 16% state layer */
.rail-nav-item--active:active .rail-nav-item__icon-container {
  background-color: color-mix(in srgb, rgb(var(--v-theme-secondary-container)), rgba(var(--v-theme-on-secondary-container), 0.16));
}

/* Remove default focus outline */
.rail-nav-item:focus {
  outline: none;
}

.server-status-item {
  width: 100% !important;
  min-width: 100% !important;
}
</style>

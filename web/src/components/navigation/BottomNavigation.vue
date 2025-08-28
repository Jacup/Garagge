<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { MAIN_NAVIGATION_ITEMS } from '@/constants/navigation'

interface Props {
  layout?: 'vertical' | 'horizontal'
}

const { layout } = withDefaults(defineProps<Props>(), {
  layout: 'vertical'
})

const router = useRouter()
const currentRoute = ref(router.currentRoute.value.path)

// Update route tracking
router.afterEach((to) => {
  currentRoute.value = to.path
})

const handleNavigation = (path: string) => {
  router.push(path)
}

const isActive = (item: (typeof MAIN_NAVIGATION_ITEMS)[0]) => {
  return currentRoute.value === item.link
}

// MD3: Use filled icon for active destination, outlined for inactive
const getIconForState = (item: (typeof MAIN_NAVIGATION_ITEMS)[0], active: boolean) => {
  // Map of base icons to their outlined variants
  const iconVariants: Record<string, { filled: string; outlined: string }> = {
    'mdi-view-dashboard': {
      filled: 'mdi-view-dashboard',
      outlined: 'mdi-view-dashboard-outline'
    },
    'mdi-car': {
      filled: 'mdi-car',
      outlined: 'mdi-car-outline'
    },
    'mdi-palette': {
      filled: 'mdi-palette',
      outlined: 'mdi-palette-outline'
    },
    'mdi-card': {
      filled: 'mdi-card',
      outlined: 'mdi-card-outline'
    }
  }

  const variant = iconVariants[item.icon]
  if (variant) {
    return active ? variant.filled : variant.outlined
  }

  // Fallback: return original icon if no variant defined
  return item.icon
}
</script>

<template>
  <nav
    :class="[
      'm3-bottom-navigation',
      `m3-bottom-navigation--${layout}`
    ]"
    role="navigation"
    aria-label="Mobile navigation"
  >
    <div class="nav-container">
      <button
        v-for="item in MAIN_NAVIGATION_ITEMS"
        :key="item.title"
        :class="[
          'nav-item',
          `nav-item--${layout}`,
          { 'nav-item--active': isActive(item) }
        ]"
        :aria-label="`Navigate to ${item.title}`"
        :aria-current="isActive(item) ? 'page' : undefined"
        @click="handleNavigation(item.link)"
      >
        <div class="nav-item__icon-container">
          <v-icon :size="24">{{ getIconForState(item, isActive(item)) }}</v-icon>
        </div>
        <span class="nav-item__label">{{ item.title }}</span>
      </button>
    </div>
  </nav>
</template>

<style scoped>
/* ==============================================
   MD3 BOTTOM NAVIGATION COMPONENT
   ============================================== */

/* Navigation Container */
.m3-bottom-navigation {
  background-color: rgb(var(--v-theme-surface-container, var(--v-theme-surface)));
  height: 64px; /* MD3 Nav bar height: 64dp */
  border-radius: 0; /* MD3 Nav bar shape: corner.none */
  box-shadow: none;

  /* Positioning */
  width: 100%;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1000;

  /* Safe area support */
  padding-bottom: env(safe-area-inset-bottom);
}

.nav-container {
  display: flex;
  height: 100%;
  align-items: center;
  padding: 6px 8px;
}

/* Vertical: Items dynamically change width to equally fit container */
.m3-bottom-navigation--vertical .nav-container {
  justify-content: space-evenly; /* MD3: evenly distributed, dynamic width */
}

/* Horizontal: Items have fixed width, extra space added to ends */
.m3-bottom-navigation--horizontal .nav-container {
  justify-content: center; /* MD3: fixed width items, space on ends */
  gap: 8px; /* Space between horizontal items */
}

/* ==============================================
   NAVIGATION ITEMS
   ============================================== */

/* Base Nav Item */
.nav-item {
  /* Reset button styles */
  background: none;
  border: none;
  border-radius: 0;
  cursor: pointer;
  text-decoration: none;

  /* MD3 Colors: Inactive */
  color: rgb(var(--v-theme-on-surface-variant));

  /* Smooth transitions */
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Vertical Layout - Stacked (default) */
.nav-item--vertical {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  /* MD3: Vertical items dynamically change width to equally fit container */
  flex: 1; /* Equal distribution */
  min-width: 64px; /* Touch target minimum */
  min-height: 64px;
  padding: 8px;
}

/* Horizontal Layout - Side by side */
.nav-item--horizontal {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: flex-start;

  /* MD3: Horizontal items have fixed width */
  flex: none; /* Fixed width, no grow */
  width: auto; /* Content-based width */
  min-width: 80px; /* Minimum for horizontal items */
  min-height: 48px; /* Smaller height for horizontal */
  padding: 4px 8px;
}

/* Active Nav Item */
.nav-item--active {
  color: rgb(var(--v-theme-secondary)); /* MD3: Active text */
}

/* ==============================================
   ICON CONTAINERS (PILL SHAPES)
   ============================================== */

/* Base Icon Container - Vertical Layout */
.nav-item__icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Vertical Layout - Stacked (icon above text) */
.nav-item--vertical .nav-item__icon-container {
  padding: 4px 20px; /* height = 32px (24px icon + 8px padding) */
  width: 56px; /* Fixed width */
  height: 32px; /* Fixed height */
  border-radius: 16px; /* Pill shape */
  margin-bottom: 4px; /* MD3: 4dp space between icon and label */
}

/* Horizontal Layout - Side by side (icon next to text) */
.nav-item--horizontal .nav-item__icon-container {
  padding: 8px 20px; /* height = 40px (24px icon + 16px padding) */
  width: auto; /* Auto width to fit content */
  height: 40px; /* Fixed height */
  border-radius: 20px; /* More rounded for horizontal */
  margin-bottom: 0; /* No margin for horizontal layout */
  margin-right: 8px; /* Space between icon and text */
}

/* Active Icon Container - Pill Background */
.nav-item--active .nav-item__icon-container {
  background-color: rgb(var(--v-theme-secondary-container)); /* MD3: Secondary container */
}

/* ==============================================
   ICONS & LABELS
   ============================================== */

/* Icon Styling */
.nav-item .v-icon {
  font-size: 24px; /* MD3: 24dp icon size */
  color: rgb(var(--v-theme-on-surface-variant)); /* MD3: Inactive icon */
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Active Icon */
.nav-item--active .v-icon {
  color: rgb(var(--v-theme-on-secondary-container)); /* MD3: Active icon */
}

/* Label Styling */
.nav-item__label {
  font-size: 12px; /* MD3: Label typography */
  font-weight: 500;
  line-height: 16px;
  text-align: center;
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* ==============================================
   INTERACTION STATES (PILL ONLY)
   ============================================== */

/* Hover State - 8% state layer */
.nav-item--active:hover .nav-item__icon-container {
  background-color: color-mix(
    in srgb,
    rgb(var(--v-theme-secondary-container)),
    rgba(var(--v-theme-on-secondary-container), 0.08)
  );
}

/* Focus State - 10% state layer */
.nav-item--active:focus-visible .nav-item__icon-container {
  background-color: color-mix(
    in srgb,
    rgb(var(--v-theme-secondary-container)),
    rgba(var(--v-theme-on-secondary-container), 0.1)
  );
}

/* Pressed State - 12% state layer */
.nav-item--active:active .nav-item__icon-container {
  background-color: color-mix(
    in srgb,
    rgb(var(--v-theme-secondary-container)),
    rgba(var(--v-theme-on-secondary-container), 0.12)
  );
}

/* Remove default focus outline */
.nav-item:focus-visible {
  outline: none;
}

/* ==============================================
   RESPONSIVE DESIGN
   ============================================== */

@media (max-width: 360px) {
  .nav-item {
    min-width: 56px;
    padding: 8px 4px;
  }

  .nav-item__label {
    font-size: 11px;
  }
}
</style>

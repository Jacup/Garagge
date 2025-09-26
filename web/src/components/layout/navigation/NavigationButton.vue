<script lang="ts" setup>
import type { NavigationItem } from '@/constants/navigation'
import { useNavigationItem } from '@/composables/useNavigation'

interface Props {
  item: NavigationItem
  layout?: 'vertical' | 'horizontal'
}

const { item, layout = 'vertical' } = defineProps<Props>()

const emit = defineEmits<{
  navigate: []
}>()

const { isActive, currentIcon, navigate } = useNavigationItem(item)

const handleClick = () => {
  emit('navigate')
  navigate()
}
</script>

<template>
  <button
    :class="['nav-item', `nav-item--${layout}`, { 'nav-item--active': isActive }]"
    :aria-label="`Navigate to ${item.title}`"
    :aria-current="isActive ? 'page' : undefined"
    @click="handleClick"
  >
    <div class="nav-item__icon-container">
      <v-icon :size="24">{{ currentIcon }}</v-icon>
    </div>
    <span class="nav-item__label">{{ item.title }}</span>
  </button>
</template>

<style lang="scss" scoped>
/* Base Nav Item */
.nav-item {
  /* Reset button styles */
  background: none;
  border: none;
  border-radius: 0;
  cursor: pointer;
  text-decoration: none;

  color: rgb(var(--v-theme-on-surface-variant));

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
  width: 72px; /* Fixed width to center 56dp icon container */
  min-width: 72px; /* Touch target minimum */
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

/* Base Icon Container */
.nav-item__icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Vertical Layout - Stacked (icon above text) */
.nav-item--vertical .nav-item__icon-container {
  padding: 4px 16px; /* height = 32px (24px icon + 8px padding) */
  width: 56px; /* Fixed width - MD3 spec */
  height: 32px; /* Fixed height - MD3 spec */
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
  background-color: rgb(var(--v-theme-secondary-container));
}

/* Icon Styling */
.nav-item .v-icon {
  font-size: 24px;
  color: rgb(var(--v-theme-on-surface-variant));
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Active Icon */
.nav-item--active .v-icon {
  color: rgb(var(--v-theme-on-secondary-container));
}

/* Label Styling */
.nav-item__label {
  font-size: 12px;
  font-weight: 500;
  line-height: 16px;
  text-align: center;
  transition: color 0.2s cubic-bezier(0.2, 0, 0, 1);
}

/* Hover State - 8% state layer */
.nav-item--active:hover .nav-item__icon-container {
  background-color: color-mix(in srgb, rgb(var(--v-theme-secondary-container)), rgba(var(--v-theme-on-secondary-container), 0.08));
}

/* Focus State - 10% state layer */
.nav-item--active:focus-visible .nav-item__icon-container {
  background-color: color-mix(in srgb, rgb(var(--v-theme-secondary-container)), rgba(var(--v-theme-on-secondary-container), 0.1));
}

/* Pressed State - 12% state layer */
.nav-item--active:active .nav-item__icon-container {
  background-color: color-mix(in srgb, rgb(var(--v-theme-secondary-container)), rgba(var(--v-theme-on-secondary-container), 0.12));
}

/* Remove default focus outline */
.nav-item:focus-visible {
  outline: none;
}
</style>

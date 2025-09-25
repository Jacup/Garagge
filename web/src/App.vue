<script lang="ts" setup>
import { ref } from 'vue'
import ResponsiveNavigation from './components/navigation/ResponsiveNavigation.vue'
import ApplicationBar from './components/layout/ApplicationBar.vue'

const navigationRef = ref<InstanceType<typeof ResponsiveNavigation>>()

</script>

<template>
  <v-app>
    <ResponsiveNavigation ref="navigationRef" />

    <ApplicationBar/>

    <v-main>
      <v-container class="app-container" fluid>
        <router-view />
      </v-container>
    </v-main>
  </v-app>
</template>

<style>
/* Global app styles using MD3 Expressive spacing system */
.app-container {
  margin: 0 !important;
  padding: var(--md-sys-spacing-xs) !important; /* 4px - consistent with MD3 */
  max-width: none !important; /* Remove default container max-width */
}

/* Override Vuetify grid system for consistent MD3 spacing */
.app-container .v-row {
  margin-left: 0 !important;
  margin-right: 0 !important;
}

.app-container .v-col {
  padding-left: 0 !important;
  padding-right: 0 !important;
}

/* Add gap between columns using MD3 spacing tokens */
.app-container .v-row > .v-col:not(:last-child) {
  margin-right: var(--md-sys-spacing-lg); /* 16px - standard element spacing */
}

.app-container .v-row:not(:last-child) {
  margin-bottom: var(--md-sys-spacing-lg); /* 16px - consistent row spacing */
}

/* Enhanced focus styles for accessibility */
.v-application :focus-visible {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
  border-radius: var(--md-sys-spacing-xs);
}

/* State layer enhancements for interactive elements */
.v-btn:hover::before,
.v-list-item:hover::before,
.v-card:hover::before {
  opacity: 0.08;
  transition: opacity var(--md-sys-motion-duration-short2) var(--md-sys-motion-easing-standard);
}

/* Enhanced elevation transitions */
.v-card,
.v-btn,
.v-sheet {
  transition: box-shadow var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-emphasized);
}

/* Consistent border radius using design tokens */
.rounded-md3-xs { border-radius: var(--md-sys-spacing-xs); }
.rounded-md3-sm { border-radius: var(--md-sys-spacing-sm); }
.rounded-md3-md { border-radius: var(--md-sys-spacing-md); }
.rounded-md3-lg { border-radius: var(--md-sys-spacing-lg); }
.rounded-md3-xl { border-radius: var(--md-sys-spacing-xl); }
.rounded-md3-xxl { border-radius: var(--md-sys-spacing-xxl); }
</style>

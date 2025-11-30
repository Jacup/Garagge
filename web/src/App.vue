<script lang="ts" setup>
import { ref } from 'vue'

import { ResponsiveNavigation, ApplicationBar, FloatingActionButton } from './components/layout'

const navigationRef = ref<InstanceType<typeof ResponsiveNavigation>>()

const handleToggleDrawer = () => {
  navigationRef.value?.toggleDrawer()
}
</script>

<template>
  <v-app>
    <ResponsiveNavigation ref="navigationRef" />

    <ApplicationBar @toggle-drawer="handleToggleDrawer" />

    <v-main>
      <v-container class="app-container" fluid>
        <router-view />
      </v-container>

      <v-footer class="app-footer">
        <div class="footer-wrapper">Created by Jakub Gramburg © 2025</div>
      </v-footer>
    </v-main>

    <FloatingActionButton />
  </v-app>
</template>

<style>
.app-container {
  margin: 0 !important;
  padding: 16px !important;
  max-width: none !important;
}

.app-footer {
  background-color: transparent !important;
  padding: 0 16px 16px 16px;
  /* Set height to allow wrapper to be visible */
  height: 80px !important;
  z-index: 0 !important;
}

.footer-wrapper {
  width: 100%;
  height: 64px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
}

/* Add bottom layout offset for mobile view when bottom navigation is present */
@media (max-width: 959px) {
  .v-main {
    --v-layout-bottom: 64px !important;
  }
}
</style>

<script lang="ts" setup>
import { ref } from 'vue'

import ResponsiveNavigation from './components/layout/ResponsiveNavigation.vue'
import ApplicationBar from './components/layout/applicationBar/ApplicationBar.vue'

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
    </v-main>
  </v-app>
</template>

<style>
.app-container {
  margin: 0 !important;
  padding: 16px !important;
  max-width: none !important;
}

.app-container .v-row {
  margin-left: 0 !important;
  margin-right: 0 !important;
}

.app-container .v-col {
  padding-left: 0 !important;
  padding-right: 0 !important;
}

.app-container .v-row > .v-col:not(:last-child) {
  margin-right: 16px;
}

.app-container .v-row:not(:last-child) {
  margin-bottom: 16px;
}
</style>

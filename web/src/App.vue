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
/* Global app styles - MD3 spacing system */
.app-container {
  margin: 0 !important;
  padding: 4px !important;
  max-width: none !important; /* Remove default container max-width */
}

/* Override Vuetify grid system for consistent 16px spacing */
.app-container .v-row {
  margin-left: 0 !important;
  margin-right: 0 !important;
}

.app-container .v-col {
  padding-left: 0 !important;
  padding-right: 0 !important;
}

/* Add gap between columns using margin instead of padding */
.app-container .v-row > .v-col:not(:last-child) {
  margin-right: 16px;
}

.app-container .v-row:not(:last-child) {
  margin-bottom: 16px;
}
</style>

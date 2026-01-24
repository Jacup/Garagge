<script lang="ts" setup>
import { ref } from 'vue'
import { ResponsiveNavigation, ApplicationBar, FloatingActionButton } from '@/components/layout'

const navigationRef = ref<InstanceType<typeof ResponsiveNavigation>>()

const handleToggleDrawer = () => {
  navigationRef.value?.toggleDrawer()
}
</script>

<template>
  <div>
    <ResponsiveNavigation ref="navigationRef" />
    <ApplicationBar @toggle-drawer="handleToggleDrawer" />

    <v-main>
      <v-container class="app-container" fluid>
        <router-view />
      </v-container>
      <v-footer class="app-footer" />
    </v-main>

    <FloatingActionButton />
  </div>
</template>

<style scoped>
.app-container {
  margin: 0 !important;
  padding: 16px !important;
  max-width: none !important;
}

.app-footer {
  background-color: transparent !important;
  height: 96px !important;
  z-index: 0 !important;
}

@media (max-width: 959px) {
  .v-main {
    --v-layout-bottom: 64px !important;
  }
}
</style>

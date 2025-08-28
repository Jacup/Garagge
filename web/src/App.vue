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
      <v-container>
        <router-view />
      </v-container>
    </v-main>
  </v-app>
</template>

<style>
/* Global app styles can go here */
</style>

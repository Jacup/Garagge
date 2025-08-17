<script lang="ts" setup>
import { ref, computed } from 'vue'
import { useDisplay } from 'vuetify'
import AppNavigation from './components/layout/AppNavigation.vue'

const { name } = useDisplay()
const drawer = ref(false)

const isDesktop = computed(() => ['lg', 'xl', 'xxl'].includes(name.value))
const isTablet = computed(() => name.value === 'md')
const isMobile = computed(() => ['sm', 'xs'].includes(name.value))
</script>

<template>
  <v-app>
    <v-navigation-drawer v-if="isDesktop" permanent :width="240"> </v-navigation-drawer>

    <v-navigation-drawer v-else-if="isTablet" rail permanent :width="72"> </v-navigation-drawer>

    <v-navigation-drawer v-else v-model="drawer" temporary> </v-navigation-drawer>

    <v-app-bar title="Application bar"></v-app-bar>

    <v-main class="d-flex align-center justify-center" height="300">
      <v-container>
        <v-sheet color="surface-light" height="200" rounded="lg" width="100%"></v-sheet>
      </v-container>
    </v-main>
  </v-app>
</template>

<style scoped>
.app-layout {
  display: flex;
  height: 100vh;
  overflow: hidden;
}
</style>

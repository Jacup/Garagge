<script lang="ts" setup>
import { ref, computed } from 'vue'
import { useDisplay } from 'vuetify'
import AppNavigation from './components/layout/AppNavigation.vue'
import AppBar from './components/layout/AppBar.vue'
const { name } = useDisplay()
const drawer = ref(false)

const isDesktop = computed(() => ['lg', 'xl', 'xxl'].includes(name.value))
const isTablet = computed(() => name.value === 'md')
const isMobile = computed(() => ['sm', 'xs'].includes(name.value))
</script>

<template>
  <v-app>
    <v-navigation-drawer v-if="isDesktop" permanent :width="240">
      <AppNavigation @navigate="drawer = false" />
    </v-navigation-drawer>

    <v-navigation-drawer v-else-if="isTablet" rail permanent :width="72">
      <AppNavigation :is-rail="true" @navigate="drawer = false" />
    </v-navigation-drawer>

    <v-navigation-drawer v-else v-model="drawer" temporary>
      <AppNavigation @navigate="drawer = false" />
    </v-navigation-drawer>

    <AppBar :is-mobile="isMobile" @update:drawer="drawer = !drawer" />

    <v-main>
      <v-container>
        <router-view />
      </v-container>
    </v-main>
  </v-app>
</template>

<style scoped>
.app-layout {
  display: flex;
  flex-direction: row;
  height: 100vh;
  overflow: hidden;
}
</style>

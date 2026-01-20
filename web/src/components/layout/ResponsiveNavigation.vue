<script lang="ts" setup>
import { ref } from 'vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'

import DrawerNavigation from './navigation/DrawerNavigation.vue'
import BottomNavigation from './navigation/BottomNavigation.vue'

const { navigationConfig, mode } = useResponsiveLayout()

const drawer = ref(false)

const emit = defineEmits<{
  drawerToggle: [value: boolean]
}>()

const handleNavigate = () => {
  if (navigationConfig.value.temporary) {
    drawer.value = false
  }
  emit('drawerToggle', drawer.value)
}

defineExpose({
  toggleDrawer: () => {
    drawer.value = !drawer.value
    emit('drawerToggle', drawer.value)
  },
})
</script>

<template>
  <v-navigation-drawer v-if="mode === 'desktop'" permanent floating :width="navigationConfig.width">
    <DrawerNavigation @navigate="handleNavigate" />
  </v-navigation-drawer>

  <v-navigation-drawer v-else-if="mode === 'tablet'" permanent floating rail :rail-width="navigationConfig.railWidth">
    <DrawerNavigation is-rail @navigate="handleNavigate" />
  </v-navigation-drawer>

  <template v-else-if="mode === 'mobile'">
    <BottomNavigation />
  </template>
</template>

<style scoped></style>

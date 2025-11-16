<script lang="ts" setup>
import { ref } from 'vue'
import type { FabMenuItem } from '@/composables/useLayoutFab'

/**
 * FAB Menu - v-speed-dial with multiple actions
 */

interface Props {
  icon: string
  text: string
  menuItems: readonly FabMenuItem[]
}

defineProps<Props>()

const speedDialOpen = ref(false)
</script>

<template>
  <v-speed-dial v-model="speedDialOpen" transition="slide-y-reverse-transition">
    <template v-slot:activator="{ props: activatorProps }">
      <v-fab
        v-bind="activatorProps"
        :icon="icon"
        color="tertiary-container"
        size="80"
        :aria-label="`${text} button`"
      />
    </template>

    <v-btn
      v-for="item in menuItems"
      :key="item.key"
      :icon="item.icon"
      :color="item.color || 'secondary'"
      :aria-label="item.text"
      size="large"
      @click="item.action"
    />
  </v-speed-dial>
</template>

<style lang="scss" scoped>
:deep(.v-btn) {
  .v-icon {
    font-size: 28px !important;
  }
}

// Speed dial list (menu items)
:deep(.v-speed-dial__list) {
  flex-direction: column-reverse;
  gap: 8px;
  align-items: flex-end;
}
</style>

<script lang="ts" setup>
import AccountMenu from '@/components/NavBar/AccountMenu.vue'

const props = defineProps<{
  showUserMenu?: boolean
  isRail?: boolean
}>()

const emit = defineEmits(['navigate'])

const mainNav = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard', link: '/' },
  { title: 'Vehicles', icon: 'mdi-car-side', link: '/vehicles' },
]
const systemNav = [
  { title: 'Settings', icon: 'mdi-cog', link: '/settings' },
  { title: 'Server Info', icon: 'mdi-server', link: '/server' },
]
</script>

<template>
  <div class="app-navigation">
    <div class="nav-header">
      <v-list-item>
        <v-list-item-title class="font-weight-medium ms-2">GARAGGE</v-list-item-title>
      </v-list-item>
    </div>

    <div class="nav-menu">
      <v-btn variant="text" icon="mdi-magnify" />

      <!-- Scrollable Menu -->
      <v-list nav density="comfortable">
        <!-- Main Navigation -->
        <v-divider class="my-2" />
        <v-list-item
          v-for="item in mainNav"
          :key="item.title"
          :prepend-icon="item.icon"
          :title="item.title"
          :to="item.link"
          link
          @click="emit('navigate')"
        />

        <!-- System Navigation -->
        <v-divider class="my-2" />
        <v-list-item
          v-for="item in systemNav"
          :key="item.title"
          :prepend-icon="item.icon"
          :title="item.title"
          :to="item.link"
          link
          @click="emit('navigate')"
        />
      </v-list>
    </div>

    <div v-if="showUserMenu" class="nav-footer">
      <AccountMenu :is-rail="isRail" @navigate="emit('navigate')" />
    </div>
  </div>
</template>

<style scoped>
.app-navigation {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.nav-header {
  flex: 0 0 auto;
  padding: 0px 16px;
}
.nav-menu {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

.nav-footer {
  flex: 0 0 auto;
  padding: 8px 16px;
}
</style>

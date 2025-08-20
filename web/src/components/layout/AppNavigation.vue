<script lang="ts" setup>
import SearchBox from '@/components/NavBar/SearchBox.vue'

const { isRail } = defineProps<{
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
      <template v-if="isRail">
        <v-list-item :prepend-icon="'mdi-alpha-g-box'" to="/" variant="plain" :ripple="false" :active="false" @click="emit('navigate')" />
      </template>
      <template v-else>
        <v-list-item :prepend-icon="'mdi-alpha-g-box'" to="/" variant="plain" :ripple="false" :active="false" @click="emit('navigate')">
          <v-list-item-title class="font-weight-medium">GARAGGE</v-list-item-title>
        </v-list-item>
      </template>
    </div>

    <div class="nav-menu">
      <div class="search-container" :class="{ 'px-2': !isRail, 'px-1': isRail }">
        <SearchBox :is-rail="isRail" />
      </div>

      <v-list nav density="comfortable">
        <v-divider class="my-2" />
        <template v-for="item in mainNav" :key="item.title">
          <v-tooltip v-if="isRail" location="end" :text="item.title">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-list-item v-bind="tooltipProps" :prepend-icon="item.icon" :to="item.link" link @click="emit('navigate')" />
            </template>
          </v-tooltip>
          <v-list-item v-else :prepend-icon="item.icon" :title="item.title" :to="item.link" link @click="emit('navigate')" />
        </template>
      </v-list>
    </div>

    <div class="nav-footer">
      <v-list nav density="comfortable">
        <v-divider class="my-2" />
        <template v-for="item in systemNav" :key="item.title">
          <v-tooltip v-if="isRail" location="end" :text="item.title">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-list-item v-bind="tooltipProps" :prepend-icon="item.icon" :to="item.link" link @click="emit('navigate')" />
            </template>
          </v-tooltip>
          <v-list-item v-else :prepend-icon="item.icon" :title="item.title" :to="item.link" link @click="emit('navigate')" />
        </template>
      </v-list>
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
  display: flex;
  flex: 0 0 auto;
  height: 64px;
  align-items: center;
}

.nav-menu {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

.search-container {
  margin-bottom: 8px;
}

.nav-footer {
  flex: 0 0 auto;
}
</style>

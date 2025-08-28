<script lang="ts" setup>
import { MAIN_NAVIGATION_ITEMS, SYSTEM_NAVIGATION_ITEMS } from '@/constants/navigation'

interface Props {
  isRail?: boolean
}

const { isRail = false } = defineProps<Props>()

const emit = defineEmits<{
  navigate: []
}>()
</script>

<template>
  <div class="drawer-navigation">
    <!-- Header with logo -->
    <div class="nav-header">
      <button :class="isRail ? 'logo-btn-rail' : 'logo-btn-expanded'" @click="$router.push('/')">
        <v-icon size="35px">mdi-alpha-g-box</v-icon>
        <span v-if="!isRail">GARAGGE</span>
      </button>
      <v-btn v-if="!isRail" class="status-indicator" variant="plain" size="x-small">
        <v-icon size="20px">mdi-radiobox-marked</v-icon>
      </v-btn>
    </div>

    <!-- Main navigation -->
    <nav class="nav-menu" role="navigation" aria-label="Main navigation">
      <v-list nav density="comfortable" color="primary">
        <template v-for="item in MAIN_NAVIGATION_ITEMS" :key="item.title">
          <v-tooltip v-if="isRail" location="end" :text="item.title">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-list-item v-bind="tooltipProps" :prepend-icon="item.icon" :to="item.link" link @click="emit('navigate')" />
            </template>
          </v-tooltip>
          <v-list-item v-else :prepend-icon="item.icon" :title="item.title" :to="item.link" link @click="emit('navigate')" />
        </template>
      </v-list>
    </nav>

    <!-- System navigation -->
    <footer class="nav-footer" role="contentinfo">
      <v-list nav density="comfortable">
        <v-divider class="my-2" />
        <template v-for="item in SYSTEM_NAVIGATION_ITEMS" :key="item.title">
          <v-tooltip v-if="isRail" location="end" :text="item.title">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-list-item v-bind="tooltipProps" :prepend-icon="item.icon" :to="item.link" link @click="emit('navigate')" />
            </template>
          </v-tooltip>
          <v-list-item v-else :prepend-icon="item.icon" :title="item.title" :to="item.link" link @click="emit('navigate')" />
        </template>
      </v-list>
    </footer>
  </div>
</template>

<style scoped>
.drawer-navigation {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 32px);
  box-sizing: border-box;
  margin: 16px 0 16px 16px;
  overflow: hidden;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
}

.nav-header {
  display: flex;
  flex: 0 0 auto;
  flex-direction: row;
  height: 64px;
  padding: 16px;
  align-items: center;
  justify-content: space-between;
}

.logo-btn-expanded {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  color: rgb(var(--v-theme-on-primary-container));
}

.logo-btn-expanded:focus {
  outline: 2px solid rgb(var(--v-theme-on-primary-container));
  outline-offset: 2px;
}

.logo-btn-expanded .v-icon {
  color: inherit;
}

.logo-btn-expanded span {
  font-family: 'Bitcount Grid Single', monospace;
  font-weight: 600;
  font-size: 1.2rem;
}

.logo-btn-rail {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  color: rgb(var(--v-theme-on-primary-container));
}

.logo-btn-rail:focus {
  outline: 2px solid rgb(var(--v-theme-on-primary-container));
  outline-offset: 2px;
}

.logo-btn-rail .v-icon {
  color: inherit;
}

.status-indicator {
  overflow: visible;
  position: relative;
  width: 36px;
  height: 36px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

.nav-menu {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

.nav-footer {
  flex: 0 0 auto;
}
</style>

<script lang="ts" setup>
import { useTheme } from 'vuetify'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import AccountMenu from '@/components/layout/applicationBar/AccountMenu.vue'
import SearchBox from '@/components/layout/applicationBar/SearchBox.vue'

const theme = useTheme()

const emit = defineEmits<{
  toggleDrawer: []
}>()

const handleSearch = (query: string) => {
  console.log('Search query:', query)
}

const { mode } = useResponsiveLayout()
</script>

<template>
  <!-- Desktop and Tablet - same layout -->
  <v-app-bar v-if="mode === 'desktop' || mode === 'tablet'" flat floating :app="true" class="header" :height="80">
    <div class="navbar-wrapper">
      <div class="search-container">
        <SearchBox @search="handleSearch" />
      </div>

      <v-spacer />
      <v-btn icon="mdi-theme-light-dark" @click="theme.cycle(['abyssDark', 'abyssLight'])" text="Cycle All Themes"></v-btn>
      <div class="account-menu-container">
        <AccountMenu />
      </div>
    </div>
  </v-app-bar>

  <!-- Mobile - with hamburger menu -->
  <v-app-bar v-else-if="mode === 'mobile'" :app="true" class="header-mobile" :height="80">
    <div class="navbar-wrapper-mobile">
      <v-btn icon="mdi-menu" @click="emit('toggleDrawer')"></v-btn>

      <div class="search-container">
        <SearchBox @search="handleSearch" :is-mobile="true" />
      </div>
      <div class="account-menu-container">
        <AccountMenu />
      </div>
    </div>
  </v-app-bar>
</template>

<style scoped>
/* Base app-bar styles */
.app-bar-base {
  padding: 16px 0px 0px 0px;
  background-color: rgba(var(--v-theme-surface), 0.8) !important;
  backdrop-filter: blur(8px);
}

.app-bar-content-base {
  background-color: rgb(var(--v-theme-surface));
  padding: 0 16px;
  height: 64px !important;
  align-items: flex-start !important;
  justify-content: flex-start !important;
}

/* Desktop specific */
.header {
  padding: 16px 0px 0px 0px;
  background-color: rgba(var(--v-theme-surface), 0.8) !important;
  backdrop-filter: blur(8px);
}

.header :deep(.v-toolbar__content) {
  background-color: rgb(var(--v-theme-surface));
  padding: 0 16px;
  height: 64px !important;
  align-items: flex-start !important;
  justify-content: flex-start !important;
}

/* Mobile specific */
.header-mobile {
  padding: 16px 0px 0px 0px;
  background-color: rgba(var(--v-theme-surface), 0.8) !important;
  backdrop-filter: blur(8px);
  height: 80px;
}

.header-mobile :deep(.v-toolbar__content) {
  background-color: rgb(var(--v-theme-surface)) !important;
  padding: 0 16px;
  height: 64px !important;
  align-items: flex-start !important;
  justify-content: flex-start !important;
}

/* Navbar wrapper base */
.navbar-wrapper-base {
  width: 100%;
  height: 64px;
  display: flex;
  flex-direction: row;
  align-items: center;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
}

/* Desktop navbar wrapper */
.navbar-wrapper {
  width: 100%;
  height: 64px;
  display: flex;
  flex-direction: row;
  align-items: center;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
  padding: 0 16px;
}

/* Mobile navbar wrapper */
.navbar-wrapper-mobile {
  width: 100%;
  height: 64px;
  display: flex;
  flex-direction: row;
  align-items: center;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
  padding: 0 4px;
  gap: 8px;
}

/* Shared utility classes */
.search-container {
  flex: 1;
}

.account-menu-container {
  display: flex;
  align-items: center;
  padding-right: 4px;
}
</style>

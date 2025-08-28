<script lang="ts" setup>
import { useTheme } from 'vuetify'
import AccountMenu from '@/components/NavBar/AccountMenu.vue'
import SearchBox from '@/components/NavBar/SearchBox.vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'

const theme = useTheme()

const handleSearch = (query: string) => {
  console.log('Search query:', query)
  // TODO: Implement search functionality
}

const { mode } = useResponsiveLayout()
</script>

<template>
  <v-app-bar v-if="mode === 'desktop'" flat floating class="header" :height="64">
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

  <v-app-bar v-else-if="mode === 'mobile'" class="header-mobile">
    <div class="navbar-wrapper-mobile">
      <v-btn disabled icon="mdi-menu"></v-btn>

      <div class="search-container">
        <SearchBox @search="handleSearch" />
      </div>

      <div class="account-menu-container">
        <AccountMenu />
      </div>
    </div>
  </v-app-bar>
</template>

<style scoped>
.header {
  padding-top: 16px;
  background-color: rgba(var(--v-theme-surface), 0.9) !important;
  backdrop-filter: blur(8px);
}

.header-mobile {
  height: 64px;
}

.header :deep(.v-toolbar__content) {
  background-color: rgb(var(--v-theme-surface));
  padding: 0 20px;
}

.header-mobile :deep(.v-toolbar__content) {
  background-color: rgb(var(--v-theme-surface)) !important;
}

.navbar-wrapper {
  width: 100%;
  height: 64px;
  padding: 0 16px;

  display: flex;
  flex-direction: row;
  align-items: center;

  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 12px;
}

.navbar-wrapper-mobile {
  width: 100%;
  height: 64px;
  padding: 0 4px;

  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 8px;

  background-color: rgba(var(--v-theme-primary), 0.08);
}

.search-container {
  flex: 1;
}

.account-menu-container {
  display: flex;
  align-items: center;
}
</style>

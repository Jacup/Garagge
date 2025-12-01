<script lang="ts" setup>
import { useTheme } from 'vuetify'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useUserStore } from '@/stores/userStore'
import AccountMenu from '@/components/layout/applicationBar/AccountMenu.vue'
import SearchBox from '@/components/layout/applicationBar/SearchBox.vue'
import { computed, watch, onMounted, onUnmounted, ref } from 'vue'

const theme = useTheme()
const userStore = useUserStore()

const systemTheme = ref<'light' | 'dark'>(
  window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
)

const activeTheme = computed(() => {
  if (userStore.settings.theme === 'system') {
    return systemTheme.value
  }
  return userStore.settings.theme
})

watch(
  activeTheme,
  (newTheme) => {
    theme.global.name.value = newTheme === 'dark' ? 'abyssDark' : 'abyssLight'
  },
  { immediate: true }
)

const toggleTheme = () => {
  const currentTheme = userStore.settings.theme
  let newTheme: 'light' | 'dark' | 'system'
  if (currentTheme === 'light') {
    newTheme = 'dark'
  } else if (currentTheme === 'dark') {
    newTheme = 'system'
  } else {
    newTheme = 'light'
  }
  userStore.updateSettings({ theme: newTheme })
}

const themeIcon = computed(() => {
  switch (userStore.settings.theme) {
    case 'light':
      return 'mdi-weather-sunny'
    case 'dark':
      return 'mdi-weather-night'
    case 'system':
      return 'mdi-theme-light-dark'
    default:
      return 'mdi-theme-light-dark'
  }
})

const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
const handleThemeChange = (e: MediaQueryListEvent) => {
  systemTheme.value = e.matches ? 'dark' : 'light'
}
onMounted(() => {
  mediaQuery.addEventListener('change', handleThemeChange)
})
onUnmounted(() => {
  mediaQuery.removeEventListener('change', handleThemeChange)
})

const handleSearch = (query: string) => {
  console.log('Search query:', query)
}

const { mode } = useResponsiveLayout()
</script>

<template>
  <v-app-bar v-if="mode === 'desktop' || mode === 'tablet'" flat floating :app="true" class="header" :height="80">
    <div class="navbar-wrapper">
      <div class="search-container">
        <SearchBox @search="handleSearch" />
      </div>

      <v-spacer />
      <v-btn :icon="themeIcon" @click="toggleTheme" text="Toggle Theme"></v-btn>
      <div class="account-menu-container">
        <AccountMenu />
      </div>
    </div>
  </v-app-bar>

  <v-app-bar v-else-if="mode === 'mobile'" :app="true" class="header-mobile" :height="80">
    <div class="navbar-wrapper-mobile">
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
  margin-left: 56px;
  flex: 1;
}

.account-menu-container {
  display: flex;
  align-items: center;
  padding-right: 4px;
}
</style>

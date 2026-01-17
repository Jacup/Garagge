<script lang="ts" setup>
import { useTheme } from 'vuetify'
import { useRoute } from 'vue-router'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useSettingsStore } from '@/stores/settings'
import AccountMenu from '@/components/layout/applicationBar/AccountMenu.vue'
import SearchBox from '@/components/layout/applicationBar/SearchBox.vue'
import { computed, watch, onMounted, onUnmounted, ref } from 'vue'

const theme = useTheme()
const settingsStore = useSettingsStore()

const systemTheme = ref<'light' | 'dark'>(window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light')

const activeTheme = computed(() => {
  if (settingsStore.currentTheme === 'system') {
    return systemTheme.value
  }
  return settingsStore.currentTheme
})

watch(
  activeTheme,
  (newTheme) => {
    theme.global.name.value = newTheme === 'dark' ? 'abyssDark' : 'abyssLight'
  },
  { immediate: true },
)

const toggleTheme = () => {
  const currentTheme = settingsStore.currentTheme
  let newTheme: 'light' | 'dark' | 'system'
  if (currentTheme === 'light') {
    newTheme = 'dark'
  } else if (currentTheme === 'dark') {
    newTheme = 'system'
  } else {
    newTheme = 'light'
  }
  settingsStore.setTheme(newTheme)
}

const themeIcon = computed(() => {
  switch (settingsStore.currentTheme) {
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

const route = useRoute()
const { mode } = useResponsiveLayout()

const appBarType = computed(() => {
  return (route.meta?.appBar as { type: 'search' | 'context' } | undefined)?.type ?? 'search'
})
</script>

<template>
  <v-app-bar v-if="mode === 'desktop' || mode === 'tablet'" flat floating class="header-desktop" :height="80">
    <div class="navbar-wrapper">
      <div class="searchbar-container">
        <SearchBox @search="handleSearch"/>
      </div>

      <v-spacer />
      <v-btn :icon="themeIcon" @click="toggleTheme" text="Toggle Theme"></v-btn>
      <div class="account-menu-container">
        <AccountMenu />
      </div>
    </div>
  </v-app-bar>

  <v-app-bar v-else-if="mode === 'mobile' && appBarType === 'search'" class="header-mobile px-1" flat>
    <div class="leading-container"></div>
    <div class="searchbar-container">
      <SearchBox @search="handleSearch" :is-mobile="true" />
    </div>
    <div class="trailing-container">
      <AccountMenu />
    </div>
  </v-app-bar>

  <v-app-bar v-else-if="mode === 'mobile' && appBarType === 'context'" class="header-mobile" flat>
    <v-btn icon variant="text">
      <v-icon size="24">mdi-arrow-left</v-icon>
    </v-btn>

    <v-app-bar-title class="ma-0">Title</v-app-bar-title>

    <v-btn icon variant="text">
      <v-icon size="24">mdi-dots-vertical</v-icon>
    </v-btn>
  </v-app-bar>
</template>

<style scoped>
/* Desktop specific */
.header-desktop {
  padding: 16px 0px 0px 0px;
  background-color: rgba(var(--v-theme-surface), 0.8) !important;
  backdrop-filter: blur(8px);
}

.header-desktop :deep(.v-toolbar__content) {
  background-color: rgb(var(--v-theme-surface));
  padding: 0 16px;
  height: 64px !important;
  align-items: flex-start !important;
  justify-content: flex-start !important;
}

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

/* Mobile specific */

.header-mobile :deep(.v-toolbar__content) {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 8px;
}

.leading-container,
.trailing-container {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 100%;
}

.searchbar-container {
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 1;
  height: 100%;
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

.account-menu-container {
  display: flex;
  align-items: center;
  padding-right: 4px;
}
</style>

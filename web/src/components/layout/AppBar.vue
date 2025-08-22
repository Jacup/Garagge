<script lang="ts" setup>
import AccountMenu from '@/components/NavBar/AccountMenu.vue'
import { useUserStore } from '@/stores/userStore'
import { useAppTheme, type ThemeVariant } from '@/composables/useTheme';
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const { isMobile } = defineProps<{
  isMobile: boolean
}>()

const emit = defineEmits<{
  (e: 'update:drawer'): void
}>()

const router = useRouter()
const userStore = useUserStore()

const {
  isDark,
  isHighContrast,
  isSystem,
  currentTheme,
  toggleDark,
  toggleHighContrast,
  setTheme,
  resetToSystem,
  initTheme,
  availableThemes
} = useAppTheme();

// Computed properties for UI
const currentThemeIcon = computed(() => {
  const config = getThemeMenuIcon(currentTheme.value);
  return config.icon;
});

const currentThemeLabel = computed(() => {
  return getThemeLabel(currentTheme.value);
});

const isCurrentTheme = (theme: ThemeVariant): boolean => {
  if (theme === 'system') {
    return isSystem.value;
  }

  // For manual themes, check exact match
  return currentTheme.value === theme;
};

const getThemeLabel = (theme: ThemeVariant): string => {
  const labels: Record<ThemeVariant, string> = {
    'system': 'Follow System',
    'light': 'Light Mode',
    'dark': 'Dark Mode',
    'light-hc': 'Light High Contrast',
    'dark-hc': 'Dark High Contrast'
  };
  return labels[theme];
};

const getThemeMenuIcon = (theme: ThemeVariant) => {
  const configs: Record<ThemeVariant, { icon: string; color: string }> = {
    'system': { icon: 'mdi-cog-sync', color: 'primary' },
    'light': { icon: 'mdi-weather-sunny', color: 'orange' },
    'dark': { icon: 'mdi-weather-night', color: 'blue' },
    'light-hc': { icon: 'mdi-contrast-circle', color: 'orange' },
    'dark-hc': { icon: 'mdi-contrast-box', color: 'blue' }
  };
  return configs[theme];
};

// Initialize theme on mount
onMounted(() => {
  initTheme();
});

const isLoggedIn = computed(() => !!userStore.accessToken)

const goToLogin = () => router.push('/login')
const goToRegister = () => router.push('/register')
</script>

<template>
  <v-app-bar>
    <v-app-bar-nav-icon v-if="isMobile" @click="emit('update:drawer')" />
    <v-app-bar-title>Application bar</v-app-bar-title>
    <v-spacer />

    <template v-slot:append>
      <!-- Single Theme Button with Menu -->
      <v-menu>
        <template #activator="{ props }">
          <v-tooltip bottom>
            <template #activator="{ props: tooltipProps }">
              <v-btn
                v-bind="{ ...props, ...tooltipProps }"
                :icon="currentThemeIcon"
                variant="text"
                size="small"
              />
            </template>
            <span>{{ currentThemeLabel }}</span>
          </v-tooltip>
        </template>

        <v-list>
          <v-list-subheader>Theme Options</v-list-subheader>

          <v-list-item
            v-for="theme in availableThemes"
            :key="theme"
            :active="isCurrentTheme(theme)"
            @click="setTheme(theme)"
          >
            <template #prepend>
              <v-icon :color="getThemeMenuIcon(theme).color">
                {{ getThemeMenuIcon(theme).icon }}
              </v-icon>
            </template>

            <v-list-item-title>
              {{ getThemeLabel(theme) }}
            </v-list-item-title>

            <template #append>
              <v-chip
                v-if="isCurrentTheme(theme)"
                color="primary"
                size="x-small"
                variant="tonal"
              >
                Active
              </v-chip>
            </template>
          </v-list-item>

          <v-divider />

          <!-- Quick Actions -->
          <v-list-item
            v-if="!isSystem"
            @click="toggleDark"
          >
            <template #prepend>
              <v-icon>{{ isDark ? 'mdi-weather-sunny' : 'mdi-weather-night' }}</v-icon>
            </template>
            <v-list-item-title>
              Switch to {{ isDark ? 'Light' : 'Dark' }}
            </v-list-item-title>
          </v-list-item>

          <v-list-item @click="toggleHighContrast">
            <template #prepend>
              <v-icon>{{ isHighContrast ? 'mdi-contrast-circle' : 'mdi-contrast-box' }}</v-icon>
            </template>
            <v-list-item-title>
              {{ isHighContrast ? 'Disable' : 'Enable' }} High Contrast
            </v-list-item-title>
          </v-list-item>

          <v-list-item v-if="!isSystem" @click="resetToSystem">
            <template #prepend>
              <v-icon>mdi-cog-sync</v-icon>
            </template>
            <v-list-item-title>
              Follow System
            </v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>

      <template v-if="isLoggedIn">
        <AccountMenu />
      </template>
      <template v-else>
        <v-btn variant="text" @click="goToLogin">Login</v-btn>
        <v-btn variant="tonal" @click="goToRegister">Register</v-btn>
      </template>
    </template>
  </v-app-bar>
</template>

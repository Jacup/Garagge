import { useTheme } from 'vuetify';
import { computed, watch, ref } from 'vue';
import materialThemes from '@/themes/material-themes.json';

type ThemeVariant = 'system' | 'light' | 'dark' | 'light-hc' | 'dark-hc';
type UserPreference = 'system' | 'light' | 'dark';
type ContrastPreference = 'normal' | 'high';

const STORAGE_KEY = 'garagge-theme-preferences';

// Persistent state
const userPreference = ref<UserPreference>('system');
const contrastPreference = ref<ContrastPreference>('normal');

// Helper: Map preferences to Vuetify theme names
const getVuetifyTheme = (
  userPref: UserPreference,
  contrastPref: ContrastPreference
): ThemeVariant => {
  if (userPref === 'system') {
    // Vuetify 3.9+ handles system automatically, but we need to handle contrast
    if (contrastPref === 'high') {
      // For system + high contrast, we need to use media query to decide
      const systemIsDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      return systemIsDark ? 'dark-hc' : 'light-hc';
    }
    return 'system';
  }

  // Manual theme selection
  if (contrastPref === 'high') {
    return userPref === 'dark' ? 'dark-hc' : 'light-hc';
  }

  return userPref;
};

// Save/load preferences
const savePreferences = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({
    userPreference: userPreference.value,
    contrastPreference: contrastPreference.value
  }));
};

const loadPreferences = () => {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      const { userPreference: savedUser, contrastPreference: savedContrast } = JSON.parse(saved);
      userPreference.value = savedUser || 'system';
      contrastPreference.value = savedContrast || 'normal';
    }
  } catch (error) {
    console.warn('Failed to load theme preferences:', error);
  }
};

export const useAppTheme = () => {
  const vuetifyTheme = useTheme();

  // Get system dark mode preference
  const systemIsDark = ref(window.matchMedia('(prefers-color-scheme: dark)').matches);

  // Listen for system changes
  const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
  mediaQuery.addEventListener('change', (e) => {
    systemIsDark.value = e.matches;
    // If we're in system mode with high contrast, update theme
    if (userPreference.value === 'system' && contrastPreference.value === 'high') {
      const newTheme = getVuetifyTheme(userPreference.value, contrastPreference.value);
      if (vuetifyTheme.global.name.value !== newTheme) {
        vuetifyTheme.global.name.value = newTheme;
      }
    }
  });

  // Computed states
  const currentTheme = computed(() =>
    getVuetifyTheme(userPreference.value, contrastPreference.value)
  );

  const isDark = computed(() => {
    const current = vuetifyTheme.global.current.value;
    return current.dark;
  });

  const isHighContrast = computed(() => contrastPreference.value === 'high');
  const isSystem = computed(() => userPreference.value === 'system' && contrastPreference.value === 'normal');

  // Apply CSS variables for MD3 colors
  const applyCSSVariables = (themeName: ThemeVariant) => {
    if (themeName === 'system') return; // Vuetify handles system internally

    let schemeName: keyof typeof materialThemes.schemes;

    // Map theme names to scheme names
    switch (themeName) {
      case 'light-hc':
        schemeName = 'light-high-contrast';
        break;
      case 'dark-hc':
        schemeName = 'dark-high-contrast';
        break;
      default:
        schemeName = themeName as keyof typeof materialThemes.schemes;
    }

    const scheme = materialThemes.schemes[schemeName];
    if (!scheme) return;

    const root = document.documentElement;

    Object.entries(scheme).forEach(([key, value]) => {
      const cssVarName = key.replace(/([A-Z])/g, '-$1').toLowerCase();
      root.style.setProperty(`--md-sys-color-${cssVarName}`, value);
    });
  };

  // Watch theme changes
  watch(currentTheme, (newTheme) => {
    // Update Vuetify theme
    if (newTheme !== vuetifyTheme.global.name.value) {
      vuetifyTheme.global.name.value = newTheme;
    }

    // Apply CSS variables (skip for 'system' - Vuetify handles it)
    if (newTheme !== 'system') {
      applyCSSVariables(newTheme);
    }

    // Save preferences
    savePreferences();
  }, { immediate: true });

  // Actions
  const setUserPreference = (pref: UserPreference) => {
    userPreference.value = pref;
  };

  const setContrastPreference = (pref: ContrastPreference) => {
    contrastPreference.value = pref;
  };

  const toggleDark = () => {
    if (userPreference.value === 'system') {
      // If currently system, switch to opposite of current system
      userPreference.value = systemIsDark.value ? 'light' : 'dark';
    } else {
      // Toggle between light/dark
      userPreference.value = userPreference.value === 'dark' ? 'light' : 'dark';
    }
  };

  const toggleHighContrast = () => {
    contrastPreference.value = contrastPreference.value === 'high' ? 'normal' : 'high';
  };

  const resetToSystem = () => {
    userPreference.value = 'system';
    contrastPreference.value = 'normal';
  };

  const setTheme = (variant: ThemeVariant) => {
    if (variant === 'system') {
      resetToSystem();
      return;
    }

    // Parse theme variant to preferences
    if (variant === 'light-hc') {
      userPreference.value = 'light';
      contrastPreference.value = 'high';
    } else if (variant === 'dark-hc') {
      userPreference.value = 'dark';
      contrastPreference.value = 'high';
    } else {
      userPreference.value = variant;
      contrastPreference.value = 'normal';
    }
  };

  // Initialize
  const initTheme = () => {
    loadPreferences();
    // Force initial theme application
    const initialTheme = currentTheme.value;
    if (initialTheme !== 'system') {
      applyCSSVariables(initialTheme);
    }
  };

  const availableThemes: ThemeVariant[] = ['system', 'light', 'dark', 'light-hc', 'dark-hc'];

  return {
    // State
    isDark,
    isHighContrast,
    isSystem,
    currentTheme,
    userPreference: computed(() => userPreference.value),
    contrastPreference: computed(() => contrastPreference.value),
    systemIsDark: computed(() => systemIsDark.value),

    // Actions
    toggleDark,
    toggleHighContrast,
    setUserPreference,
    setContrastPreference,
    setTheme,
    resetToSystem,
    initTheme,

    // Data
    availableThemes
  };
};

// Utility functions remain the same but work with RGB values
export function getMaterialColor(colorName: string): string {
  return `rgb(var(--md-sys-color-${colorName}))`;
}

export function getMaterialColors(colorNames: string[]): Record<string, string> {
  return colorNames.reduce((acc, name) => {
    acc[name] = getMaterialColor(name);
    return acc;
  }, {} as Record<string, string>);
}

export type { ThemeVariant, UserPreference, ContrastPreference };

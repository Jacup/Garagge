// Styles
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'

// Composables
import { createVuetify } from 'vuetify'
import { md3 } from 'vuetify/blueprints'

import abyssTheme from '@/theme/md3-abyss.json'

const SETTINGS_STORAGE_KEY = 'user_settings'

// Function to read the initial theme directly from localStorage and resolve 'system'
function getInitialTheme() {
  let storedTheme: 'light' | 'dark' | 'system' = 'system';

  try {
    const settingsStr = localStorage.getItem(SETTINGS_STORAGE_KEY);
    if (settingsStr) {
      const settings = JSON.parse(settingsStr);
      if (['light', 'dark', 'system'].includes(settings.theme)) {
        storedTheme = settings.theme;
      }
    }
  } catch {
    // Fallback to system if parsing fails
    storedTheme = 'system';
  }

  if (storedTheme === 'system') {
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    return prefersDark ? 'abyssDark' : 'abyssLight';
  }

  return storedTheme === 'dark' ? 'abyssDark' : 'abyssLight';
}


function mapThemeKeys<T extends Record<string, string>>(jsonColors: T): Record<string, string> {
  const toKebab = (key: string) =>
    key.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase();

  return Object.fromEntries(
    Object.entries(jsonColors).map(([key, value]) => [toKebab(key), value])
  );
}const abyssDark = {
  dark: true,
  colors: mapThemeKeys(abyssTheme.schemes.dark),
}

const abyssLight = {
  dark: false,
  colors: mapThemeKeys(abyssTheme.schemes.light),
}

export default createVuetify({
  theme: {
    defaultTheme: getInitialTheme(),
    themes: {
      abyssDark: abyssDark,
      abyssLight: abyssLight
    }
  },
  defaults: {
    VBtn: {
      class: 'md3-btn no-uppercase'
    },
  },
  blueprint: md3,
})

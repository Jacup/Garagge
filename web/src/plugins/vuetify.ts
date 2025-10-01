// Styles
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'

// Composables
import { createVuetify } from 'vuetify'
import { md3 } from 'vuetify/blueprints'

import abyssTheme from '@/theme/md3-abyss.json'

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
    defaultTheme: 'abyssLight',
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

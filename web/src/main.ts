import './assets/main.css'
import '@mdi/font/css/materialdesignicons.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

// vuetify
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi'
import { createVuetifyColors } from '@/themes/md3-themes';
import materialThemes from '@/themes/material-themes.json';

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'system',
    themes: {
      // Natywny system theme (Vuetify 3.9+) - automatycznie używa light/dark
      light: {
        dark: false,
        colors: createVuetifyColors(materialThemes.schemes.light)
      },
      dark: {
        dark: true,
        colors: createVuetifyColors(materialThemes.schemes.dark)
      },
      // Motywy high-contrast
      'light-hc': {
        dark: false,
        colors: createVuetifyColors(materialThemes.schemes['light-high-contrast'])
      },
      'dark-hc': {
        dark: true,
        colors: createVuetifyColors(materialThemes.schemes['dark-high-contrast'])
      }
    }
  },
  icons: {
    defaultSet: 'mdi',
    sets: {
      mdi,
    },
  },
})

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(vuetify)

app.mount('#app')

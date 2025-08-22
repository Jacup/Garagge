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
import { md3 } from 'vuetify/blueprints'

const vuetify = createVuetify({
  // blueprint: md3, // Przywrócony - spróbujemy naprawić inaczej
  components,
  directives,
  // defaults: {
  //   VCard: {
  //     color: 'surface-container' // Default dla wszystkich v-card
  //   },
  //   VCardItem: {
  //     color: 'primary' // Default dla card items (title, subtitle)
  //   }
  // },
  theme: {
    defaultTheme: 'dark',
    themes: {
      // Tymczasowo: domyślne Vuetify kolory dla podstawowych motywów
      light: {
        dark: false
        // Brak custom colors - Vuetify użyje domyślnych
      },
      dark: {
        dark: true
        // Brak custom colors - Vuetify użyje domyślnych
      },
      // Zachowujemy nasze MD3 kolory dla high-contrast
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

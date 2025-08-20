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

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'dark',
    themes: {
      dark: {
        dark: true,
        colors: {
          primary: '#00b7ab',      // --color-accent
          secondary: '#f5fe7f',    // --color-highlight
          accent: '#c0f3f4',       // --color-brand-light
          background: '#000000',   // --color-bg
          surface: '#1e1e1e',      // --color-card
          'surface-variant': '#2a2a2a', // --color-card-contrast
          success: '#00b7ab',
          warning: '#f5fe7f',
          error: '#ff5a5f',
        }
      },
      light: {
        dark: false,
        colors: {
          primary: '#00b7ab',
          secondary: '#f5fe7f',
          accent: '#c0f3f4',
          background: '#ffffff',
          surface: '#f5f5f5',
          'surface-variant': '#e0e0e0',
          success: '#00b7ab',
          warning: '#f5fe7f',
          error: '#ff5a5f',
        }
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

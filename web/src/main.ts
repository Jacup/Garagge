import { registerPlugins } from './plugins'

import App from './App.vue'
import router from './router'
import { createApp, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useAuthStore } from './stores/auth'

import 'vuetify/styles'
import './styles/md3-list.css'

const app = createApp(App)

registerPlugins(app)

const authStore = useAuthStore()
const { isAuthenticated } = storeToRefs(authStore)

watch(isAuthenticated, (newVal) => {
  if (!newVal) {
    router.push({ name: 'Login' })
  }
})

app.mount('#app')

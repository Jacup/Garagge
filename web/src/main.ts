import { registerPlugins } from './plugins'

import App from './App.vue'
import { createApp } from 'vue'

// vuetify
import 'vuetify/styles'

const app = createApp(App)

registerPlugins(app)

app.mount('#app')

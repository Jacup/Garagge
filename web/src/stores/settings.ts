import { defineStore } from 'pinia'

interface UserSettings {
  theme: 'light' | 'dark' | 'system'
}

const defaultSettings: UserSettings = {
  theme: 'system',
}

export const useSettingsStore = defineStore('settings', {
  state: (): { settings: UserSettings } => ({
    settings: defaultSettings,
  }),

  getters: {
    currentTheme: (state) => state.settings.theme,
  },


  actions: {
    setTheme(theme: UserSettings['theme']) {
      this.settings.theme = theme
    },
  },

  persist: true,
})

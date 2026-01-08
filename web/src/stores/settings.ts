import { defineStore } from 'pinia'

export type VehicleViewType = 'list' | 'detailed-list' | 'cards'

interface UserSettings {
  theme: 'light' | 'dark' | 'system'
  vehicleViewMode: VehicleViewType
}

const defaultSettings: UserSettings = {
  theme: 'system',
  vehicleViewMode: 'detailed-list',
}

export const useSettingsStore = defineStore('settings', {
  state: (): { settings: UserSettings } => ({
    settings: defaultSettings,
  }),

  getters: {
    currentTheme: (state) => state.settings.theme,
    currentVehicleViewMode: (state) => state.settings.vehicleViewMode,
  },

  actions: {
    setTheme(theme: UserSettings['theme']) {
      this.settings.theme = theme
    },
    setVehicleViewMode(mode: VehicleViewType) {
      this.settings.vehicleViewMode = mode
    },
  },

  persist: true,
})

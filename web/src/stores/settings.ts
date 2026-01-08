import { defineStore } from 'pinia'

export type VehicleViewType = 'list' | 'detailed-list' | 'cards'
export type Theme = 'light' | 'dark' | 'system'

interface AppSettings {
  theme: Theme
  vehicleViewMode: VehicleViewType
}

const defaultSettings: AppSettings = {
  theme: 'system',
  vehicleViewMode: 'detailed-list',
}

export const useSettingsStore = defineStore('settings', {
  state: (): { settings: AppSettings } => ({
    settings: defaultSettings,
  }),

  getters: {
    currentTheme: (state) => state.settings.theme,
    currentVehicleViewMode: (state) => state.settings.vehicleViewMode,
  },

  actions: {
    setTheme(theme: Theme) {
      this.settings.theme = theme
    },
    setVehicleViewMode(mode: VehicleViewType) {
      this.settings.vehicleViewMode = mode
    },
  },

  persist: true,
})

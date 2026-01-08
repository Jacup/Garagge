import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSettingsStore } from '../settings'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

import { createApp } from 'vue'

const app = createApp({})
describe('Settings Store', () => {
  beforeEach(() => {
    const pinia = createPinia().use(piniaPluginPersistedstate)
    app.use(pinia)
    setActivePinia(pinia)
  })

  it('initializes with default settings', () => {
    const store = useSettingsStore()

    expect(store.settings).toEqual({
      theme: 'system',
      vehicleViewMode: 'detailed-list',
    })
    expect(store.settings.theme).toBe('system')
    expect(store.settings.vehicleViewMode).toBe('detailed-list')
  })

  it('updates theme correctly', () => {
    const store = useSettingsStore()

    store.setTheme('dark')
    expect(store.settings.theme).toBe('dark')

    store.setTheme('light')
    expect(store.settings.theme).toBe('light')
  })

  it('updates vehicle view mode correctly', () => {
    const store = useSettingsStore()

    store.setVehicleViewMode('cards')
    expect(store.settings.vehicleViewMode).toBe('cards')

    store.setVehicleViewMode('list')
    expect(store.settings.vehicleViewMode).toBe('list')
  })
})

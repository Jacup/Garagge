import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSettingsStore } from '../settings'

describe('Settings Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
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

    store.settings.theme = 'dark'
    expect(store.settings.theme).toBe('dark')

    store.settings.theme = 'light'
    expect(store.settings.theme).toBe('light')
  })

  it('updates vehicle view mode correctly', () => {
    const store = useSettingsStore()

    store.settings.vehicleViewMode = 'cards'
    expect(store.settings.vehicleViewMode).toBe('cards')

    store.settings.vehicleViewMode = 'list'
    expect(store.settings.vehicleViewMode).toBe('list')
  })
})

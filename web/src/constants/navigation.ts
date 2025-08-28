export interface NavigationItem {
  title: string
  icon: string
  link: string
  value?: string
}

export const MAIN_NAVIGATION_ITEMS: NavigationItem[] = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard', link: '/', value: '/' },
  { title: 'Vehicles', icon: 'mdi-car', link: '/vehicles', value: '/vehicles' },
  { title: 'Components', icon: 'mdi-palette', link: '/components', value: '/components' },
  { title: 'Cards', icon: 'mdi-card', link: '/cards', value: '/cards' },
] as const

export const SYSTEM_NAVIGATION_ITEMS: NavigationItem[] = [
  { title: 'Settings', icon: 'mdi-cog', link: '/settings', value: '/settings' },
  { title: 'Server Info', icon: 'mdi-server', link: '/server', value: '/server' },
] as const

export const ALL_NAVIGATION_ITEMS = [...MAIN_NAVIGATION_ITEMS, ...SYSTEM_NAVIGATION_ITEMS] as const

export interface NavigationItem {
  title: string
  icon: string
  activeIcon?: string
  link: string
  value?: string
}

export const MAIN_NAVIGATION_ITEMS: NavigationItem[] = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard-outline', activeIcon: 'mdi-view-dashboard', link: '/', value: '/' },
  { title: 'Vehicles', icon: 'mdi-car-outline', activeIcon: 'mdi-car', link: '/vehicles', value: '/vehicles' },
  { title: 'Settings', icon: 'mdi-cog-outline', activeIcon: 'mdi-cog', link: '/settings', value: '/settings' },
] as const

export const SYSTEM_NAVIGATION_ITEMS: NavigationItem[] = [
] as const

export const ALL_NAVIGATION_ITEMS = [...MAIN_NAVIGATION_ITEMS, ...SYSTEM_NAVIGATION_ITEMS] as const

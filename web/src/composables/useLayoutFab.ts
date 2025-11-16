import { ref, readonly } from 'vue'

export interface FabMenuItem {
  key: string
  icon: string
  text: string
  color?: string
  action: () => void
}

interface BaseFabConfig {
  icon: string
  text: string
}

interface RegularFabConfig extends BaseFabConfig {
  type: 'regular'
  action: () => void
}

interface MenuFabConfig extends BaseFabConfig {
  type: 'menu'
  menuItems: FabMenuItem[]
}

export type FabConfig = RegularFabConfig | MenuFabConfig

/**
 * Composable for FAB management
 * Views register FAB config, FloatingActionButton component decides which variant to render
 */

const fabConfig = ref<FabConfig | null>(null)

export function useLayoutFab() {
  function registerFab(config: Omit<RegularFabConfig, 'type'>) {
    fabConfig.value = { ...config, type: 'regular' }
  }

  function registerFabMenu(config: Omit<MenuFabConfig, 'type'>) {
    fabConfig.value = { ...config, type: 'menu' }
  }

  function unregisterFab() {
    fabConfig.value = null
  }

  return {
    fabConfig: readonly(fabConfig),
    registerFab,
    registerFabMenu,
    unregisterFab,
  }
}

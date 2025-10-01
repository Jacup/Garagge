import { ref, readonly } from 'vue'

export interface FabConfig {
  icon: string
  text: string
  action: () => void
}

const fabConfig = ref<FabConfig | null>(null)

/**
 * Composable for managing FAB across different layout modes
 * Allows routes to register FAB configuration that will be displayed
 * in appropriate locations based on screen size and MD3 guidelines
 */
export function useLayoutFab() {
  const registerFab = (config: FabConfig) => {
    fabConfig.value = config
  }

  const unregisterFab = () => {
    fabConfig.value = null
  }

  const shouldShowInDrawer = (mode: 'desktop' | 'tablet' | 'mobile') => {
    if (!fabConfig.value) return false
    // FAB shows in drawer for desktop and tablet modes
    return mode === 'desktop' || mode === 'tablet'
  }

  const shouldShowFloating = (mode: 'desktop' | 'tablet' | 'mobile') => {
    if (!fabConfig.value) return false
    // FAB shows floating only for mobile mode
    return mode === 'mobile'
  }

  return {
    fabConfig: readonly(fabConfig),
    registerFab,
    unregisterFab,
    shouldShowInDrawer,
    shouldShowFloating
  }
}

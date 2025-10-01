import { ref, readonly, computed } from 'vue'
import { useResponsiveLayout } from './useResponsiveLayout'

export interface FabConfig {
  icon: string
  text: string
  action: () => void
}

export type FabDisplayMode = 'floating' | 'drawer-extended' | 'drawer-compact' | 'hidden'

const fabConfig = ref<FabConfig | null>(null)

/**
 * Composable for managing FAB across different layout modes
 * Views register FAB configuration, layout system determines display mode
 */
export function useLayoutFab() {
  const { mode } = useResponsiveLayout()

  const registerFab = (config: FabConfig) => {
    fabConfig.value = config
  }

  const unregisterFab = () => {
    fabConfig.value = null
  }

  /**
   * Determines how FAB should be displayed based on layout mode
   * Business logic: Mobile = floating, Desktop/Tablet = in drawer
   */
  const displayMode = computed((): FabDisplayMode => {
    if (!fabConfig.value) return 'hidden'

    switch (mode.value) {
      case 'mobile':
        return 'floating'
      case 'tablet':
        return 'drawer-compact'
      case 'desktop':
        return 'drawer-extended'
      default:
        return 'hidden'
    }
  })

  // Convenience computed properties for components
  const shouldShowFloating = computed(() => displayMode.value === 'floating')
  const shouldShowInDrawer = computed(() =>
    displayMode.value === 'drawer-extended' || displayMode.value === 'drawer-compact'
  )
  const isDrawerExtended = computed(() => displayMode.value === 'drawer-extended')
  const isDrawerCompact = computed(() => displayMode.value === 'drawer-compact')

  return {
    fabConfig: readonly(fabConfig),
    displayMode,
    registerFab,
    unregisterFab,
    shouldShowFloating,
    shouldShowInDrawer,
    isDrawerExtended,
    isDrawerCompact
  }
}

import { computed, readonly, type Ref } from 'vue'
import { useDisplay } from 'vuetify'

export type LayoutMode = 'desktop' | 'tablet' | 'mobile'

export interface ResponsiveLayout {
  mode: Readonly<Ref<LayoutMode>>
  isDesktop: Readonly<Ref<boolean>>
  isTablet: Readonly<Ref<boolean>>
  isMobile: Readonly<Ref<boolean>>
  navigationConfig: Readonly<Ref<NavigationConfig>>
}

export interface NavigationConfig {
  type: 'drawer' | 'rail' | 'bottom'
  width?: number
  railWidth?: number
  permanent: boolean
  floating: boolean
  temporary: boolean
}

/**
 * Composable for managing responsive layout behavior
 * Centralizes breakpoint logic and navigation configuration
 */
export function useResponsiveLayout(): ResponsiveLayout {
  const { name } = useDisplay()

  const isDesktop = computed(() => ['lg', 'xl', 'xxl'].includes(name.value))
  const isTablet = computed(() => name.value === 'md')
  const isMobile = computed(() => ['sm', 'xs'].includes(name.value))

  const mode = computed((): LayoutMode => {
    if (isDesktop.value) return 'desktop'
    if (isTablet.value) return 'tablet'
    return 'mobile'
  })

  const navigationConfig = computed((): NavigationConfig => {
    switch (mode.value) {
      case 'desktop':
        return {
          type: 'drawer',
          width: 260,
          permanent: true,
          floating: true,
          temporary: false,
        }
      case 'tablet':
        return {
          type: 'rail',
          railWidth: 80,
          permanent: true,
          floating: true,
          temporary: false,
        }
      case 'mobile':
      default:
        return {
          type: 'bottom',
          permanent: false,
          floating: false,
          temporary: true,
        }
    }
  })

  return {
    mode: readonly(mode),
    isDesktop: readonly(isDesktop),
    isTablet: readonly(isTablet),
    isMobile: readonly(isMobile),
    navigationConfig: readonly(navigationConfig),
  }
}

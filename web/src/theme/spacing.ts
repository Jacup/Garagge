/**
 * Material Design 3 Expressive Spacing System
 * Consistent spacing tokens following MD3 guidelines
 */

/**
 * MD3 Spacing Scale
 * Based on 4px base unit with consistent increments
 */
export const spacingScale = {
  none: '0px',
  xs: '4px',      // 4px  - 1 unit
  sm: '8px',      // 8px  - 2 units  
  md: '12px',     // 12px - 3 units
  lg: '16px',     // 16px - 4 units
  xl: '20px',     // 20px - 5 units
  xxl: '24px',    // 24px - 6 units
  xxxl: '28px',   // 28px - 7 units
  xxxxl: '32px',  // 32px - 8 units
} as const

/**
 * Extended spacing for larger layouts
 */
export const spacingExtended = {
  ...spacingScale,
  '5xl': '36px',   // 36px - 9 units
  '6xl': '40px',   // 40px - 10 units
  '7xl': '44px',   // 44px - 11 units
  '8xl': '48px',   // 48px - 12 units
  '9xl': '56px',   // 56px - 14 units
  '10xl': '64px',  // 64px - 16 units
  '11xl': '72px',  // 72px - 18 units
  '12xl': '80px',  // 80px - 20 units
} as const

/**
 * Component-specific spacing tokens
 * Following MD3 component specifications
 */
export const componentSpacing = {
  // Buttons
  buttonPaddingHorizontal: spacingScale.lg,   // 16px
  buttonPaddingVertical: spacingScale.sm,     // 8px
  buttonGap: spacingScale.sm,                 // 8px
  
  // Cards
  cardPadding: spacingScale.lg,               // 16px
  cardGap: spacingScale.md,                   // 12px
  cardMargin: spacingScale.sm,                // 8px
  
  // Form controls
  inputPadding: spacingScale.lg,              // 16px
  inputMargin: spacingScale.sm,               // 8px
  inputGap: spacingScale.xs,                  // 4px
  
  // Lists
  listItemPadding: spacingScale.lg,           // 16px
  listItemGap: spacingScale.xs,               // 4px
  listSectionGap: spacingScale.md,            // 12px
  
  // Navigation
  navItemPadding: spacingScale.md,            // 12px
  navItemGap: spacingScale.xs,                // 4px
  navSectionGap: spacingScale.lg,             // 16px
  
  // Layout
  containerPadding: spacingScale.lg,          // 16px
  sectionGap: spacingScale.xxl,               // 24px
  pageMargin: spacingScale.lg,                // 16px
  
  // Dialogs and sheets
  dialogPadding: spacingScale.xxl,            // 24px
  sheetPadding: spacingScale.lg,              // 16px
  
  // App bar
  appBarPadding: spacingScale.lg,             // 16px
  appBarHeight: '64px',
  
  // Bottom navigation
  bottomNavPadding: spacingScale.sm,          // 8px
  bottomNavHeight: '80px',
  
  // Floating elements
  fabMargin: spacingScale.lg,                 // 16px
  tooltipPadding: spacingScale.sm,            // 8px
  
  // Grid and layout
  gridGap: spacingScale.lg,                   // 16px
  columnGap: spacingScale.lg,                 // 16px
  rowGap: spacingScale.lg,                    // 16px
} as const

/**
 * Responsive spacing multipliers
 */
export const responsiveSpacing = {
  mobile: {
    multiplier: 0.8,
    containerPadding: spacingScale.md,        // 12px on mobile
    sectionGap: spacingScale.lg,              // 16px on mobile
  },
  tablet: {
    multiplier: 1,
    containerPadding: spacingScale.lg,        // 16px on tablet
    sectionGap: spacingScale.xl,              // 20px on tablet
  },
  desktop: {
    multiplier: 1.2,
    containerPadding: spacingScale.xl,        // 20px on desktop
    sectionGap: spacingScale.xxl,             // 24px on desktop
  },
} as const

/**
 * Convert spacing tokens to CSS custom properties
 */
export function spacingToCssProperties() {
  const cssProperties: Record<string, string> = {}
  
  // Base spacing scale
  Object.entries(spacingExtended).forEach(([key, value]) => {
    cssProperties[`--md-sys-spacing-${key}`] = value
  })
  
  // Component spacing
  Object.entries(componentSpacing).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    cssProperties[`--md-sys-spacing-${kebabKey}`] = value
  })
  
  return cssProperties
}

/**
 * Generate CSS utility classes for spacing
 */
export function generateSpacingClasses() {
  let css = ''
  
  // Margin classes
  Object.entries(spacingExtended).forEach(([key, value]) => {
    // All margins
    css += `.m-${key} { margin: ${value}; }\n`
    // Directional margins
    css += `.mt-${key} { margin-top: ${value}; }\n`
    css += `.mr-${key} { margin-right: ${value}; }\n`
    css += `.mb-${key} { margin-bottom: ${value}; }\n`
    css += `.ml-${key} { margin-left: ${value}; }\n`
    // Axis margins
    css += `.mx-${key} { margin-left: ${value}; margin-right: ${value}; }\n`
    css += `.my-${key} { margin-top: ${value}; margin-bottom: ${value}; }\n`
  })
  
  // Padding classes
  Object.entries(spacingExtended).forEach(([key, value]) => {
    // All padding
    css += `.p-${key} { padding: ${value}; }\n`
    // Directional padding
    css += `.pt-${key} { padding-top: ${value}; }\n`
    css += `.pr-${key} { padding-right: ${value}; }\n`
    css += `.pb-${key} { padding-bottom: ${value}; }\n`
    css += `.pl-${key} { padding-left: ${value}; }\n`
    // Axis padding
    css += `.px-${key} { padding-left: ${value}; padding-right: ${value}; }\n`
    css += `.py-${key} { padding-top: ${value}; padding-bottom: ${value}; }\n`
  })
  
  // Gap classes
  Object.entries(spacingExtended).forEach(([key, value]) => {
    css += `.gap-${key} { gap: ${value}; }\n`
    css += `.gap-x-${key} { column-gap: ${value}; }\n`
    css += `.gap-y-${key} { row-gap: ${value}; }\n`
  })
  
  return css
}

/**
 * Get responsive spacing value
 */
export function getResponsiveSpacing(baseSpacing: string, breakpoint: keyof typeof responsiveSpacing): string {
  const multiplier = responsiveSpacing[breakpoint].multiplier
  const numValue = parseInt(baseSpacing.replace('px', ''))
  return `${Math.round(numValue * multiplier)}px`
}

/**
 * Spacing utilities for components
 */
export const spacingUtilities = {
  /**
   * Get consistent spacing for a component
   */
  getComponentSpacing: (component: keyof typeof componentSpacing) => {
    return componentSpacing[component] || spacingScale.md
  },
  
  /**
   * Calculate spacing based on design tokens
   */
  calculateSpacing: (units: number) => {
    return `${units * 4}px`
  },
  
  /**
   * Get spacing between related elements
   */
  getRelatedSpacing: () => spacingScale.sm,
  
  /**
   * Get spacing between unrelated elements  
   */
  getUnrelatedSpacing: () => spacingScale.lg,
  
  /**
   * Get spacing for sections
   */
  getSectionSpacing: () => spacingScale.xxl,
}
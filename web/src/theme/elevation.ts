/**
 * Material Design 3 Expressive Elevation System
 * Enhanced shadow tokens for better depth perception
 */

/**
 * MD3 Elevation Levels
 * Following Material Design 3 elevation specification
 */
export const elevationLevels = {
  level0: 0,
  level1: 1,
  level2: 3,
  level3: 6,
  level4: 8,
  level5: 12,
} as const

/**
 * MD3 Shadow Tokens
 * Using multiple shadow layers for realistic depth
 */
export const elevationShadows = {
  level0: 'none',
  
  level1: `
    0px 1px 2px 0px rgba(0, 0, 0, 0.3),
    0px 1px 3px 1px rgba(0, 0, 0, 0.15)
  `,
  
  level2: `
    0px 1px 2px 0px rgba(0, 0, 0, 0.3),
    0px 2px 6px 2px rgba(0, 0, 0, 0.15)
  `,
  
  level3: `
    0px 1px 3px 0px rgba(0, 0, 0, 0.3),
    0px 4px 8px 3px rgba(0, 0, 0, 0.15)
  `,
  
  level4: `
    0px 2px 3px 0px rgba(0, 0, 0, 0.3),
    0px 6px 10px 4px rgba(0, 0, 0, 0.15)
  `,
  
  level5: `
    0px 4px 4px 0px rgba(0, 0, 0, 0.3),
    0px 8px 12px 6px rgba(0, 0, 0, 0.15)
  `,
} as const

/**
 * Component-specific elevation mappings
 * Based on Material Design 3 component guidelines
 */
export const componentElevations = {
  // Surfaces
  surface: elevationLevels.level0,
  surfaceContainer: elevationLevels.level0,
  surfaceContainerLow: elevationLevels.level1,
  surfaceContainerHigh: elevationLevels.level1,
  surfaceContainerHighest: elevationLevels.level2,
  
  // Navigation
  navigationBar: elevationLevels.level2,
  navigationRail: elevationLevels.level0,
  navigationDrawer: elevationLevels.level1,
  appBar: elevationLevels.level0,
  
  // Buttons
  button: elevationLevels.level0,
  buttonElevated: elevationLevels.level1,
  fab: elevationLevels.level3,
  fabPressed: elevationLevels.level1,
  
  // Cards
  card: elevationLevels.level1,
  cardElevated: elevationLevels.level1,
  cardFilled: elevationLevels.level0,
  cardOutlined: elevationLevels.level0,
  
  // Sheets and overlays
  bottomSheet: elevationLevels.level1,
  dialog: elevationLevels.level3,
  fullscreenDialog: elevationLevels.level0,
  sideSheet: elevationLevels.level1,
  
  // Menus and tooltips
  menu: elevationLevels.level2,
  tooltip: elevationLevels.level2,
  
  // Other components
  chip: elevationLevels.level0,
  chipElevated: elevationLevels.level1,
  searchBar: elevationLevels.level2,
  snackbar: elevationLevels.level3,
} as const

/**
 * Dynamic elevation states
 * For interactive components with state changes
 */
export const elevationStates = {
  hover: {
    level0: elevationLevels.level1,
    level1: elevationLevels.level2,
    level2: elevationLevels.level3,
    level3: elevationLevels.level4,
    level4: elevationLevels.level5,
    level5: elevationLevels.level5, // Max elevation
  },
  
  pressed: {
    level0: elevationLevels.level0,
    level1: elevationLevels.level1,
    level2: elevationLevels.level1,
    level3: elevationLevels.level2,
    level4: elevationLevels.level3,
    level5: elevationLevels.level4,
  },
  
  dragged: {
    level0: elevationLevels.level4,
    level1: elevationLevels.level5,
    level2: elevationLevels.level5,
    level3: elevationLevels.level5,
    level4: elevationLevels.level5,
    level5: elevationLevels.level5,
  },
} as const

/**
 * Convert elevation tokens to CSS custom properties
 */
export function elevationToCssProperties() {
  const cssProperties: Record<string, string> = {}
  
  // Elevation levels
  Object.entries(elevationLevels).forEach(([key, value]) => {
    cssProperties[`--md-sys-elevation-${key}`] = value.toString()
  })
  
  // Shadow tokens
  Object.entries(elevationShadows).forEach(([key, value]) => {
    cssProperties[`--md-sys-elevation-shadow-${key}`] = value.replace(/\s+/g, ' ').trim()
  })
  
  // Component elevations
  Object.entries(componentElevations).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    cssProperties[`--md-sys-elevation-${kebabKey}`] = value.toString()
  })
  
  return cssProperties
}

/**
 * Generate CSS utility classes for elevation
 */
export function generateElevationClasses() {
  let css = ''
  
  // Elevation level classes
  Object.entries(elevationShadows).forEach(([key, shadow]) => {
    if (!shadow || shadow === 'none') {
      css += `.elevation-${key} { box-shadow: none; }\n`
    } else {
      const cleanShadow = shadow.replace(/\s+/g, ' ').trim()
      css += `.elevation-${key} { box-shadow: ${cleanShadow}; }\n`
    }
  })
  
  // Hover elevation classes
  Object.entries(elevationStates.hover).forEach(([level, targetLevel]) => {
    const targetShadow = elevationShadows[`level${targetLevel}` as keyof typeof elevationShadows]
    if (targetShadow && targetShadow !== 'none') {
      const cleanShadow = targetShadow.replace(/\s+/g, ' ').trim()
      css += `.elevation-${level}:hover { box-shadow: ${cleanShadow}; }\n`
    }
  })
  
  return css
}

/**
 * Get shadow for specific elevation level
 */
export function getElevationShadow(level: keyof typeof elevationLevels): string {
  return elevationShadows[level] || elevationShadows.level0
}

/**
 * Get component elevation level
 */
export function getComponentElevation(component: keyof typeof componentElevations): number {
  return componentElevations[component] || elevationLevels.level0
}
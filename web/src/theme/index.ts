/**
 * Material Design 3 Expressive Theme System
 * Complete integration of typography, motion, elevation, and spacing
 */

import { md3Typography, typographyToCssProperties, generateTypographyClasses } from './typography'
import { motionToCssProperties, generateMotionClasses } from './motion'
import { elevationToCssProperties, generateElevationClasses } from './elevation'
import { spacingToCssProperties, generateSpacingClasses } from './spacing'

/**
 * Complete MD3 Expressive theme configuration
 */
export const md3ExpressiveTheme = {
  typography: md3Typography,
  motion: {
    duration: {
      short1: '50ms',
      short2: '100ms',
      short3: '150ms',
      short4: '200ms',
      medium1: '250ms',
      medium2: '300ms',
      medium3: '350ms',
      medium4: '400ms',
      long1: '450ms',
      long2: '500ms',
      long3: '550ms',
      long4: '600ms',
    },
    easing: {
      standard: 'cubic-bezier(0.2, 0, 0, 1)',
      emphasized: 'cubic-bezier(0.2, 0, 0, 1)',
      expressive: 'cubic-bezier(0.4, 0, 0.2, 1)',
    }
  },
  elevation: {
    level0: 0,
    level1: 1,
    level2: 3,
    level3: 6,
    level4: 8,
    level5: 12,
  },
  spacing: {
    none: '0px',
    xs: '4px',
    sm: '8px',
    md: '12px',
    lg: '16px',
    xl: '20px',
    xxl: '24px',
    xxxl: '28px',
    xxxxl: '32px',
  }
}

/**
 * Generate all CSS custom properties for the theme
 */
export function generateThemeCssProperties() {
  return {
    ...typographyToCssProperties(md3Typography),
    ...motionToCssProperties(),
    ...elevationToCssProperties(),
    ...spacingToCssProperties(),
  }
}

/**
 * Generate all utility CSS classes
 */
export function generateThemeUtilityClasses() {
  return `
/* Typography Classes */
${generateTypographyClasses(md3Typography)}

/* Motion Classes */
${generateMotionClasses()}

/* Elevation Classes */
${generateElevationClasses()}

/* Spacing Classes */
${generateSpacingClasses()}

/* Additional Utility Classes */
.surface-container-lowest { background-color: rgb(var(--v-theme-surface-container-lowest)); }
.surface-container-low { background-color: rgb(var(--v-theme-surface-container-low)); }
.surface-container { background-color: rgb(var(--v-theme-surface-container)); }
.surface-container-high { background-color: rgb(var(--v-theme-surface-container-high)); }
.surface-container-highest { background-color: rgb(var(--v-theme-surface-container-highest)); }

/* Interaction State Classes */
.interactive {
  transition: var(--md-sys-motion-transition-hover);
  cursor: pointer;
}

.interactive:hover {
  background-color: rgba(var(--v-theme-on-surface), 0.08);
}

.interactive:focus {
  background-color: rgba(var(--v-theme-on-surface), 0.12);
  outline: 2px solid rgb(var(--v-theme-outline));
  outline-offset: 2px;
}

.interactive:active {
  background-color: rgba(var(--v-theme-on-surface), 0.16);
}

/* State Layer Classes */
.state-layer {
  position: relative;
  overflow: hidden;
}

.state-layer::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: currentColor;
  opacity: 0;
  transition: opacity var(--md-sys-motion-duration-short2) var(--md-sys-motion-easing-standard);
  pointer-events: none;
}

.state-layer:hover::before {
  opacity: 0.08;
}

.state-layer:focus::before {
  opacity: 0.12;
}

.state-layer:active::before {
  opacity: 0.16;
}

/* Accessibility */
.focus-visible {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
}

/* Screen reader only */
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* High contrast support */
@media (prefers-contrast: high) {
  .interactive {
    border: 1px solid currentColor;
  }
  
  .elevation-level1,
  .elevation-level2,
  .elevation-level3,
  .elevation-level4,
  .elevation-level5 {
    border: 1px solid rgba(var(--v-theme-outline), 0.5);
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}

/* Color scheme support */
@media (prefers-color-scheme: dark) {
  :root {
    color-scheme: dark;
  }
}

@media (prefers-color-scheme: light) {
  :root {
    color-scheme: light;
  }
}
`
}

/**
 * Inject theme CSS properties into document
 */
export function injectThemeCssProperties() {
  const properties = generateThemeCssProperties()
  const root = document.documentElement
  
  Object.entries(properties).forEach(([property, value]) => {
    root.style.setProperty(property, value)
  })
}

/**
 * Create theme style element
 */
export function createThemeStyleElement() {
  const style = document.createElement('style')
  style.id = 'md3-expressive-theme'
  style.textContent = `
:root {
${Object.entries(generateThemeCssProperties())
  .map(([prop, value]) => `  ${prop}: ${value};`)
  .join('\n')}
}

${generateThemeUtilityClasses()}
`
  return style
}

/**
 * Apply theme to document
 */
export function applyThemeToDocument() {
  // Remove existing theme if present
  const existingTheme = document.getElementById('md3-expressive-theme')
  if (existingTheme) {
    existingTheme.remove()
  }
  
  // Add new theme
  const themeStyle = createThemeStyleElement()
  document.head.appendChild(themeStyle)
}

// Export all theme tokens for use in components
export * from './typography'
export * from './motion'
export * from './elevation'
export * from './spacing'
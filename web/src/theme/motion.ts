/**
 * Material Design 3 Expressive Motion System
 * Enhanced easing functions and duration tokens for better UX
 */

/**
 * MD3 Motion Duration Tokens
 * Based on the official Material Design 3 motion specification
 */
export const motionDuration = {
  // Short durations for simple transitions
  short1: '50ms',
  short2: '100ms',
  short3: '150ms',
  short4: '200ms',

  // Medium durations for standard transitions
  medium1: '250ms',
  medium2: '300ms',
  medium3: '350ms',
  medium4: '400ms',

  // Long durations for complex animations
  long1: '450ms',
  long2: '500ms',
  long3: '550ms',
  long4: '600ms',

  // Extra long for emphasis
  extraLong1: '700ms',
  extraLong2: '800ms',
  extraLong3: '900ms',
  extraLong4: '1000ms',
} as const

/**
 * MD3 Motion Easing Functions
 * Enhanced with expressive curves for better feel
 */
export const motionEasing = {
  // Standard easing - for most transitions
  standard: 'cubic-bezier(0.2, 0, 0, 1)',
  standardAccelerate: 'cubic-bezier(0.3, 0, 1, 1)',
  standardDecelerate: 'cubic-bezier(0, 0, 0, 1)',

  // Emphasized easing - for important state changes
  emphasized: 'cubic-bezier(0.2, 0, 0, 1)',
  emphasizedAccelerate: 'cubic-bezier(0.3, 0, 0.8, 0.15)',
  emphasizedDecelerate: 'cubic-bezier(0.05, 0.7, 0.1, 1)',

  // Legacy support
  legacy: 'cubic-bezier(0.4, 0, 0.2, 1)',
  legacyAccelerate: 'cubic-bezier(0.4, 0, 1, 1)',
  legacyDecelerate: 'cubic-bezier(0, 0, 0.2, 1)',

  // Expressive easing - for delightful interactions
  expressive: 'cubic-bezier(0.4, 0, 0.2, 1)',
  expressiveAccelerate: 'cubic-bezier(0.4, 0, 1, 1)',
  expressiveDecelerate: 'cubic-bezier(0, 0, 0.2, 1)',
} as const

/**
 * Common transition combinations
 */
export const motionTransitions = {
  // Standard transitions
  fadeIn: `opacity ${motionDuration.short4} ${motionEasing.standard}`,
  fadeOut: `opacity ${motionDuration.short3} ${motionEasing.standard}`,
  
  // Slide transitions
  slideUp: `transform ${motionDuration.medium2} ${motionEasing.emphasized}`,
  slideDown: `transform ${motionDuration.medium2} ${motionEasing.emphasized}`,
  slideLeft: `transform ${motionDuration.medium2} ${motionEasing.emphasized}`,
  slideRight: `transform ${motionDuration.medium2} ${motionEasing.emphasized}`,
  
  // Scale transitions
  scaleIn: `transform ${motionDuration.short4} ${motionEasing.emphasized}`,
  scaleOut: `transform ${motionDuration.short3} ${motionEasing.emphasized}`,
  
  // Color transitions
  colorChange: `color ${motionDuration.short4} ${motionEasing.standard}, background-color ${motionDuration.short4} ${motionEasing.standard}`,
  
  // Layout transitions
  layout: `all ${motionDuration.medium2} ${motionEasing.standard}`,
  
  // Interactive states
  hover: `all ${motionDuration.short2} ${motionEasing.standard}`,
  focus: `all ${motionDuration.short3} ${motionEasing.standard}`,
  pressed: `all ${motionDuration.short1} ${motionEasing.standard}`,
} as const

/**
 * Convert motion tokens to CSS custom properties
 */
export function motionToCssProperties() {
  const cssProperties: Record<string, string> = {}
  
  // Duration tokens
  Object.entries(motionDuration).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    cssProperties[`--md-sys-motion-duration-${kebabKey}`] = value
  })
  
  // Easing tokens
  Object.entries(motionEasing).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    cssProperties[`--md-sys-motion-easing-${kebabKey}`] = value
  })
  
  // Transition tokens
  Object.entries(motionTransitions).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    cssProperties[`--md-sys-motion-transition-${kebabKey}`] = value
  })
  
  return cssProperties
}

/**
 * Generate CSS utility classes for motion
 */
export function generateMotionClasses() {
  let css = ''
  
  // Duration classes
  Object.entries(motionDuration).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    css += `.duration-${kebabKey} { transition-duration: ${value}; }\n`
  })
  
  // Easing classes
  Object.entries(motionEasing).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    css += `.ease-${kebabKey} { transition-timing-function: ${value}; }\n`
  })
  
  // Transition classes
  Object.entries(motionTransitions).forEach(([key, value]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    css += `.transition-${kebabKey} { transition: ${value}; }\n`
  })
  
  return css
}
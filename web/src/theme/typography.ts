/**
 * Material Design 3 Expressive Typography System
 * Based on the updated MD3 type scale with enhanced expressiveness
 */

export interface TypographyToken {
  fontFamily: string
  fontSize: string
  fontWeight: number
  lineHeight: string
  letterSpacing: string
}

/**
 * MD3 Expressive Typography Scale
 * Enhanced with better contrast and readability
 */
export const md3Typography = {
  // Display styles - for large, impactful text
  displayLarge: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '57px',
    fontWeight: 400,
    lineHeight: '64px',
    letterSpacing: '-0.25px',
  } as TypographyToken,

  displayMedium: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '45px',
    fontWeight: 400,
    lineHeight: '52px',
    letterSpacing: '0px',
  } as TypographyToken,

  displaySmall: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '36px',
    fontWeight: 400,
    lineHeight: '44px',
    letterSpacing: '0px',
  } as TypographyToken,

  // Headline styles - for headings and important text
  headlineLarge: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '32px',
    fontWeight: 400,
    lineHeight: '40px',
    letterSpacing: '0px',
  } as TypographyToken,

  headlineMedium: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '28px',
    fontWeight: 400,
    lineHeight: '36px',
    letterSpacing: '0px',
  } as TypographyToken,

  headlineSmall: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '24px',
    fontWeight: 400,
    lineHeight: '32px',
    letterSpacing: '0px',
  } as TypographyToken,

  // Title styles - for sections and cards
  titleLarge: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '22px',
    fontWeight: 400,
    lineHeight: '28px',
    letterSpacing: '0px',
  } as TypographyToken,

  titleMedium: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '16px',
    fontWeight: 500,
    lineHeight: '24px',
    letterSpacing: '0.15px',
  } as TypographyToken,

  titleSmall: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '14px',
    fontWeight: 500,
    lineHeight: '20px',
    letterSpacing: '0.1px',
  } as TypographyToken,

  // Body styles - for main content
  bodyLarge: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '16px',
    fontWeight: 400,
    lineHeight: '24px',
    letterSpacing: '0.5px',
  } as TypographyToken,

  bodyMedium: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '14px',
    fontWeight: 400,
    lineHeight: '20px',
    letterSpacing: '0.25px',
  } as TypographyToken,

  bodySmall: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '12px',
    fontWeight: 400,
    lineHeight: '16px',
    letterSpacing: '0.4px',
  } as TypographyToken,

  // Label styles - for UI elements
  labelLarge: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '14px',
    fontWeight: 500,
    lineHeight: '20px',
    letterSpacing: '0.1px',
  } as TypographyToken,

  labelMedium: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '12px',
    fontWeight: 500,
    lineHeight: '16px',
    letterSpacing: '0.5px',
  } as TypographyToken,

  labelSmall: {
    fontFamily: 'Roboto, sans-serif',
    fontSize: '11px',
    fontWeight: 500,
    lineHeight: '16px',
    letterSpacing: '0.5px',
  } as TypographyToken,
}

/**
 * Converts typography tokens to CSS custom properties
 */
export function typographyToCssProperties(typography: Record<string, TypographyToken>) {
  const cssProperties: Record<string, string> = {}
  
  Object.entries(typography).forEach(([key, token]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    cssProperties[`--md-sys-typescale-${kebabKey}-font-family`] = token.fontFamily
    cssProperties[`--md-sys-typescale-${kebabKey}-font-size`] = token.fontSize
    cssProperties[`--md-sys-typescale-${kebabKey}-font-weight`] = token.fontWeight.toString()
    cssProperties[`--md-sys-typescale-${kebabKey}-line-height`] = token.lineHeight
    cssProperties[`--md-sys-typescale-${kebabKey}-letter-spacing`] = token.letterSpacing
  })
  
  return cssProperties
}

/**
 * Generate CSS utility classes for typography
 */
export function generateTypographyClasses(typography: Record<string, TypographyToken>) {
  let css = ''
  
  Object.entries(typography).forEach(([key, token]) => {
    const kebabKey = key.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase()
    css += `
.text-${kebabKey} {
  font-family: ${token.fontFamily};
  font-size: ${token.fontSize};
  font-weight: ${token.fontWeight};
  line-height: ${token.lineHeight};
  letter-spacing: ${token.letterSpacing};
}
`
  })
  
  return css
}
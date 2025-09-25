// Styles
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'

// Composables
import { createVuetify, type ThemeDefinition } from 'vuetify'
import { md3 } from 'vuetify/blueprints'

// MD3 Expressive theme integration
import { applyThemeToDocument } from '@/theme'
import abyssTheme from '@/theme/md3-abyss.json'

function mapThemeKeys<T extends Record<string, string>>(jsonColors: T): Record<string, string> {
  const toKebab = (key: string) =>
    key.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase();

  return Object.fromEntries(
    Object.entries(jsonColors).map(([key, value]) => [toKebab(key), value])
  );
}

// Enhanced themes with MD3 Expressive improvements
const abyssDark: ThemeDefinition = {
  dark: true,
  colors: {
    ...mapThemeKeys(abyssTheme.schemes.dark),
    // Add any additional color overrides for better expressiveness
  },
  variables: {
    // Enhanced border radius for more expressive feel
    'border-radius-root': '12px',
    'border-radius-card': '16px',
    'border-radius-button': '20px',
    'border-radius-fab': '16px',
    
    // Enhanced shadow settings
    'shadow-key-umbra-opacity': '0.3',
    'shadow-key-penumbra-opacity': '0.15',
    
    // Enhanced motion settings
    'motion-standard': '300ms cubic-bezier(0.2, 0, 0, 1)',
    'motion-emphasized': '300ms cubic-bezier(0.2, 0, 0, 1)',
    'motion-expressive': '400ms cubic-bezier(0.4, 0, 0.2, 1)',
  }
}

const abyssLight: ThemeDefinition = {
  dark: false,
  colors: {
    ...mapThemeKeys(abyssTheme.schemes.light),
    // Add any additional color overrides for better expressiveness
  },
  variables: {
    // Enhanced border radius for more expressive feel
    'border-radius-root': '12px',
    'border-radius-card': '16px', 
    'border-radius-button': '20px',
    'border-radius-fab': '16px',
    
    // Enhanced shadow settings
    'shadow-key-umbra-opacity': '0.3',
    'shadow-key-penumbra-opacity': '0.15',
    
    // Enhanced motion settings
    'motion-standard': '300ms cubic-bezier(0.2, 0, 0, 1)',
    'motion-emphasized': '300ms cubic-bezier(0.2, 0, 0, 1)',
    'motion-expressive': '400ms cubic-bezier(0.4, 0, 0.2, 1)',
  }
}

// Apply MD3 Expressive theme CSS properties
applyThemeToDocument()

export default createVuetify({
  theme: {
    defaultTheme: 'abyssLight',
    themes: {
      abyssDark: abyssDark,
      abyssLight: abyssLight
    }
  },
  blueprint: md3,
  defaults: {
    // Enhanced defaults for MD3 Expressive
    VBtn: {
      style: 'transition: all 200ms cubic-bezier(0.2, 0, 0, 1); border-radius: 20px;',
      elevation: 0,
    },
    VCard: {
      style: 'transition: all 300ms cubic-bezier(0.2, 0, 0, 1);',
      elevation: 1,
      rounded: 'lg',
    },
    VSheet: {
      rounded: 'lg',
    },
    VDialog: {
      elevation: 6,
      rounded: 'xl',
    },
    VTextField: {
      variant: 'outlined',
      density: 'comfortable',
      hideDetails: 'auto',
    },
    VSelect: {
      variant: 'outlined', 
      density: 'comfortable',
      hideDetails: 'auto',
    },
    VAutocomplete: {
      variant: 'outlined',
      density: 'comfortable', 
      hideDetails: 'auto',
    },
    VTextarea: {
      variant: 'outlined',
      density: 'comfortable',
      hideDetails: 'auto',
    },
    VNavigationDrawer: {
      elevation: 1,
    },
    VAppBar: {
      elevation: 0,
    },
    VBottomNavigation: {
      elevation: 2,
    },
    VFab: {
      elevation: 3,
      rounded: 'lg',
    },
    VSnackbar: {
      elevation: 3,
      rounded: 'lg',
    },
    VMenu: {
      elevation: 2,
      rounded: 'lg',
    },
    VTooltip: {
      elevation: 2,
    },
  }
})

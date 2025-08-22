import themesData from './material-themes.json';

type MaterialScheme = {
  [key: string]: string;
};

export const md3Themes = themesData.schemes;

// Convert MD3 colors to Vuetify format (keeping HEX format for theme definition)
export const createVuetifyColors = (scheme: MaterialScheme) => {
  const colors: Record<string, string> = {};

  // Map wszystkie MD3 kolory (pozostaw jako HEX - Vuetify sam konwertuje na RGB w CSS variables)
  Object.entries(scheme).forEach(([key, value]) => {
    // Convert camelCase to kebab-case
    const kebabKey = key.replace(/([A-Z])/g, '-$1').toLowerCase();
    colors[kebabKey] = value; // Zostaw jako HEX!
  });

  // Dodaj aliasy dla standardowych kolorów Vuetify
  return {
    ...colors,
    // Standard Vuetify colors (mapped from MD3)
    primary: colors.primary,
    secondary: colors.secondary,
    tertiary: colors.tertiary,
    accent: colors.tertiary,
    error: colors.error,
    info: colors.primary,
    success: colors.tertiary,
    warning: colors.error,
    background: colors.background,
    surface: colors.surface
  };
};

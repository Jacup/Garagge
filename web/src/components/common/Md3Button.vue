<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  variant?: 'filled' | 'elevated' | 'tonal' | 'outlined' | 'text'
  size?: 'small' | 'default' | 'large'
  icon?: string | boolean
  loading?: boolean
  disabled?: boolean
  block?: boolean
  rounded?: boolean
  color?: string
  prependIcon?: string
  appendIcon?: string
  expressive?: boolean // New prop for extra expressive styling
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'filled',
  size: 'default',
  icon: false,
  loading: false,
  disabled: false,
  block: false,
  rounded: false,
  expressive: true,
})

const emit = defineEmits<{
  click: [event: Event]
}>()

// Enhanced styling for MD3 Expressive
const buttonClass = computed(() => {
  const classes = ['md3-expressive-btn']
  
  if (props.expressive) {
    classes.push('md3-expressive-btn--expressive')
  }
  
  if (props.size !== 'default') {
    classes.push(`md3-expressive-btn--${props.size}`)
  }
  
  return classes.join(' ')
})

// Map our variants to Vuetify variants
const vuetifyVariant = computed(() => {
  switch (props.variant) {
    case 'filled':
      return 'flat'
    case 'elevated':
      return 'elevated'
    case 'tonal':
      return 'tonal'
    case 'outlined':
      return 'outlined'
    case 'text':
      return 'text'
    default:
      return 'flat'
  }
})

// Enhanced elevation for different states
const elevation = computed(() => {
  if (props.disabled) return 0
  if (props.variant === 'elevated') return 1
  if (props.variant === 'filled' && props.expressive) return 1
  return 0
})

const handleClick = (event: Event) => {
  if (!props.disabled && !props.loading) {
    emit('click', event)
  }
}
</script>

<template>
  <v-btn
    :class="buttonClass"
    :variant="vuetifyVariant"
    :size="size"
    :loading="loading"
    :disabled="disabled"
    :block="block"
    :rounded="rounded"
    :color="color"
    :prepend-icon="prependIcon"
    :append-icon="appendIcon"
    :elevation="elevation"
    @click="handleClick"
  >
    <template v-if="icon && typeof icon === 'string'">
      <v-icon>{{ icon }}</v-icon>
    </template>
    
    <slot v-if="!icon || typeof icon === 'boolean'" />
    
    <template #prepend v-if="$slots.prepend">
      <slot name="prepend" />
    </template>
    
    <template #append v-if="$slots.append">
      <slot name="append" />
    </template>
  </v-btn>
</template>

<style scoped>
/* MD3 Expressive Button Enhancements */
.md3-expressive-btn {
  /* Enhanced border radius for more expressive feel */
  border-radius: var(--md-sys-spacing-xl) !important;
  
  /* Improved typography for better readability */
  font-family: var(--md-sys-typescale-label-large-font-family);
  font-size: var(--md-sys-typescale-label-large-font-size);
  font-weight: var(--md-sys-typescale-label-large-font-weight);
  line-height: var(--md-sys-typescale-label-large-line-height);
  letter-spacing: var(--md-sys-typescale-label-large-letter-spacing);
  
  /* Enhanced transitions for better feel */
  transition: 
    background-color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    box-shadow var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-emphasized),
    transform var(--md-sys-motion-duration-short2) var(--md-sys-motion-easing-standard),
    color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard);
  
  /* Better padding for touch targets */
  padding-left: var(--md-sys-spacing-xxl) !important;
  padding-right: var(--md-sys-spacing-xxl) !important;
  min-height: 40px;
}

/* Expressive variant with enhanced interactions */
.md3-expressive-btn--expressive:hover {
  transform: translateY(-1px);
}

.md3-expressive-btn--expressive:active {
  transform: translateY(0px);
}

/* Size variants */
.md3-expressive-btn--small {
  font-size: var(--md-sys-typescale-label-medium-font-size);
  padding-left: var(--md-sys-spacing-lg) !important;
  padding-right: var(--md-sys-spacing-lg) !important;
  min-height: 32px;
}

.md3-expressive-btn--large {
  font-size: var(--md-sys-typescale-label-large-font-size);
  padding-left: var(--md-sys-spacing-xxxxl) !important;
  padding-right: var(--md-sys-spacing-xxxxl) !important;
  min-height: 48px;
}

/* Enhanced focus states for accessibility */
.md3-expressive-btn:focus-visible {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
}

/* Enhanced state layers */
.md3-expressive-btn .v-btn__overlay {
  transition: opacity var(--md-sys-motion-duration-short2) var(--md-sys-motion-easing-standard);
}

.md3-expressive-btn:hover .v-btn__overlay {
  opacity: 0.08;
}

.md3-expressive-btn:focus .v-btn__overlay {
  opacity: 0.12;
}

.md3-expressive-btn:active .v-btn__overlay {
  opacity: 0.16;
}

/* Disabled state improvements */
.md3-expressive-btn:disabled {
  opacity: 0.38;
  transform: none !important;
}

/* Loading state improvements */
.md3-expressive-btn .v-progress-circular {
  color: inherit;
}

/* Icon enhancements */
.md3-expressive-btn .v-icon {
  transition: transform var(--md-sys-motion-duration-short2) var(--md-sys-motion-easing-standard);
}

.md3-expressive-btn--expressive:hover .v-icon {
  transform: scale(1.05);
}

/* Ripple enhancements for expressive variant */
.md3-expressive-btn--expressive .v-ripple__container {
  border-radius: var(--md-sys-spacing-xl);
}

/* High contrast mode support */
@media (prefers-contrast: high) {
  .md3-expressive-btn {
    border: 1px solid currentColor;
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .md3-expressive-btn,
  .md3-expressive-btn .v-btn__overlay,
  .md3-expressive-btn .v-icon {
    transition: none !important;
  }
  
  .md3-expressive-btn--expressive:hover,
  .md3-expressive-btn--expressive:active {
    transform: none !important;
  }
}
</style>
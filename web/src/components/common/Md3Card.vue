<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  variant?: 'elevated' | 'filled' | 'outlined'
  interactive?: boolean
  loading?: boolean
  disabled?: boolean
  color?: string
  elevation?: number | string
  rounded?: string | number | boolean
  expressive?: boolean // Enhanced MD3 Expressive styling
  hoverElevation?: boolean // Enable elevation change on hover
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'elevated',
  interactive: false,
  loading: false,
  disabled: false,
  rounded: 'lg',
  expressive: true,
  hoverElevation: true,
})

const emit = defineEmits<{
  click: [event: Event]
}>()

// Calculate elevation based on variant and state
const cardElevation = computed(() => {
  if (props.disabled) return 0
  
  if (props.elevation !== undefined) {
    return props.elevation
  }
  
  switch (props.variant) {
    case 'elevated':
      return 1
    case 'filled':
      return 0
    case 'outlined':
      return 0
    default:
      return 1
  }
})

// Enhanced class computation
const cardClass = computed(() => {
  const classes = ['md3-expressive-card']
  
  if (props.expressive) {
    classes.push('md3-expressive-card--expressive')
  }
  
  if (props.interactive) {
    classes.push('md3-expressive-card--interactive')
  }
  
  if (props.hoverElevation && props.variant === 'elevated') {
    classes.push('md3-expressive-card--hover-elevation')
  }
  
  if (props.loading) {
    classes.push('md3-expressive-card--loading')
  }
  
  if (props.disabled) {
    classes.push('md3-expressive-card--disabled')
  }
  
  return classes.join(' ')
})

const handleClick = (event: Event) => {
  if (!props.disabled && !props.loading && props.interactive) {
    emit('click', event)
  }
}
</script>

<template>
  <v-card
    :class="cardClass"
    :color="color"
    :elevation="cardElevation"
    :rounded="rounded"
    :disabled="disabled"
    :loading="loading"
    @click="handleClick"
  >
    <!-- Loading overlay -->
    <v-overlay
      v-if="loading"
      :model-value="loading"
      contained
      class="d-flex align-center justify-center"
      scrim="transparent"
    >
      <v-progress-circular
        indeterminate
        size="40"
        color="primary"
      />
    </v-overlay>

    <!-- Card header -->
    <template v-if="$slots.header">
      <div class="md3-card-header">
        <slot name="header" />
      </div>
    </template>

    <!-- Card media/image -->
    <template v-if="$slots.media">
      <div class="md3-card-media">
        <slot name="media" />
      </div>
    </template>

    <!-- Card content -->
    <v-card-text v-if="$slots.default" class="md3-card-content">
      <slot />
    </v-card-text>

    <!-- Card actions -->
    <template v-if="$slots.actions">
      <v-card-actions class="md3-card-actions">
        <slot name="actions" />
      </v-card-actions>
    </template>
  </v-card>
</template>

<style scoped>
/* MD3 Expressive Card Base Styles */
.md3-expressive-card {
  /* Enhanced border radius for more expressive feel */
  border-radius: var(--md-sys-spacing-lg) !important;
  
  /* Improved transitions for better UX */
  transition: 
    box-shadow var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-emphasized),
    transform var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    background-color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard);
  
  /* Better spacing using design tokens */
  overflow: hidden;
  position: relative;
}

/* Expressive enhancements */
.md3-expressive-card--expressive {
  /* Subtle scale on hover for interactive cards */
  transform-origin: center;
}

/* Interactive card enhancements */
.md3-expressive-card--interactive {
  cursor: pointer;
  user-select: none;
}

.md3-expressive-card--interactive:hover {
  /* Enhanced state layer */
  background-color: rgba(var(--v-theme-on-surface), 0.05);
}

.md3-expressive-card--interactive:focus {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
}

.md3-expressive-card--interactive:active {
  transform: scale(0.98);
}

/* Hover elevation enhancement */
.md3-expressive-card--hover-elevation:hover {
  box-shadow: 
    0px 2px 4px 0px rgba(0, 0, 0, 0.3),
    0px 4px 8px 2px rgba(0, 0, 0, 0.15) !important;
}

/* Loading state */
.md3-expressive-card--loading {
  pointer-events: none;
  opacity: 0.7;
}

/* Disabled state */
.md3-expressive-card--disabled {
  opacity: 0.38;
  pointer-events: none;
}

/* Card section styles */
.md3-card-header {
  padding: var(--md-sys-spacing-xxl) var(--md-sys-spacing-xxl) var(--md-sys-spacing-lg);
}

.md3-card-media {
  position: relative;
  overflow: hidden;
}

.md3-card-content {
  padding: 0 var(--md-sys-spacing-xxl) var(--md-sys-spacing-lg) !important;
}

/* First content section gets top padding */
.md3-card-content:first-child {
  padding-top: var(--md-sys-spacing-xxl) !important;
}

.md3-card-actions {
  padding: var(--md-sys-spacing-sm) var(--md-sys-spacing-lg) var(--md-sys-spacing-lg) !important;
  gap: var(--md-sys-spacing-sm);
}

/* Enhanced typography in cards */
.md3-expressive-card :deep(.v-card-title) {
  font-family: var(--md-sys-typescale-headline-small-font-family);
  font-size: var(--md-sys-typescale-headline-small-font-size);
  font-weight: var(--md-sys-typescale-headline-small-font-weight);
  line-height: var(--md-sys-typescale-headline-small-line-height);
  letter-spacing: var(--md-sys-typescale-headline-small-letter-spacing);
  color: rgb(var(--v-theme-on-surface));
}

.md3-expressive-card :deep(.v-card-subtitle) {
  font-family: var(--md-sys-typescale-title-medium-font-family);
  font-size: var(--md-sys-typescale-title-medium-font-size);
  font-weight: var(--md-sys-typescale-title-medium-font-weight);
  line-height: var(--md-sys-typescale-title-medium-line-height);
  letter-spacing: var(--md-sys-typescale-title-medium-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

.md3-expressive-card :deep(.v-card-text) {
  font-family: var(--md-sys-typescale-body-medium-font-family);
  font-size: var(--md-sys-typescale-body-medium-font-size);
  font-weight: var(--md-sys-typescale-body-medium-font-weight);
  line-height: var(--md-sys-typescale-body-medium-line-height);
  letter-spacing: var(--md-sys-typescale-body-medium-letter-spacing);
  color: rgb(var(--v-theme-on-surface-variant));
}

/* Image enhancements */
.md3-card-media :deep(img) {
  width: 100%;
  height: auto;
  display: block;
  transition: transform var(--md-sys-motion-duration-medium2) var(--md-sys-motion-easing-standard);
}

.md3-expressive-card--interactive:hover .md3-card-media :deep(img) {
  transform: scale(1.02);
}

/* Action button improvements */
.md3-card-actions :deep(.v-btn) {
  border-radius: var(--md-sys-spacing-xl);
}

/* Responsive improvements */
@media (max-width: 600px) {
  .md3-card-header,
  .md3-card-content {
    padding-left: var(--md-sys-spacing-lg) !important;
    padding-right: var(--md-sys-spacing-lg) !important;
  }
  
  .md3-card-actions {
    padding-left: var(--md-sys-spacing-md) !important;
    padding-right: var(--md-sys-spacing-md) !important;
  }
}

/* High contrast mode support */
@media (prefers-contrast: high) {
  .md3-expressive-card {
    border: 1px solid rgba(var(--v-theme-outline), 0.5);
  }
  
  .md3-expressive-card--interactive:hover {
    border-color: rgb(var(--v-theme-outline));
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .md3-expressive-card,
  .md3-expressive-card :deep(img) {
    transition: none !important;
  }
  
  .md3-expressive-card--interactive:active,
  .md3-expressive-card--interactive:hover .md3-card-media :deep(img) {
    transform: none !important;
  }
}

/* Dark theme adjustments */
.v-theme--dark .md3-expressive-card--interactive:hover {
  background-color: rgba(var(--v-theme-on-surface), 0.08);
}
</style>
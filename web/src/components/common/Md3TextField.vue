<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue?: string | number
  label?: string
  placeholder?: string
  hint?: string
  errorMessages?: string | string[]
  type?: string
  variant?: 'outlined' | 'filled' | 'underlined' | 'solo'
  density?: 'default' | 'comfortable' | 'compact'
  disabled?: boolean
  readonly?: boolean
  clearable?: boolean
  prependIcon?: string
  appendIcon?: string
  prependInnerIcon?: string
  appendInnerIcon?: string
  loading?: boolean
  expressive?: boolean // Enhanced MD3 Expressive styling
  focused?: boolean
  rules?: ((value: any) => boolean | string)[]
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  variant: 'outlined',
  density: 'comfortable',
  disabled: false,
  readonly: false,
  clearable: false,
  loading: false,
  expressive: true,
  focused: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number]
  focus: [event: FocusEvent]
  blur: [event: FocusEvent]
  click: [event: Event]
}>()

// Enhanced styling classes
const inputClass = computed(() => {
  const classes = ['md3-expressive-input']
  
  if (props.expressive) {
    classes.push('md3-expressive-input--expressive')
  }
  
  if (props.variant) {
    classes.push(`md3-expressive-input--${props.variant}`)
  }
  
  return classes.join(' ')
})

// Enhanced border radius based on variant
const borderRadius = computed(() => {
  switch (props.variant) {
    case 'outlined':
      return 'var(--md-sys-spacing-md)'
    case 'filled':
      return 'var(--md-sys-spacing-md) var(--md-sys-spacing-md) 0 0'
    case 'solo':
      return 'var(--md-sys-spacing-lg)'
    default:
      return 'var(--md-sys-spacing-md)'
  }
})

const handleInput = (value: string | number) => {
  emit('update:modelValue', value)
}

const handleFocus = (event: FocusEvent) => {
  emit('focus', event)
}

const handleBlur = (event: FocusEvent) => {
  emit('blur', event)
}

const handleClick = (event: Event) => {
  emit('click', event)
}
</script>

<template>
  <v-text-field
    :class="inputClass"
    :model-value="modelValue"
    :label="label"
    :placeholder="placeholder"
    :hint="hint"
    :error-messages="errorMessages"
    :type="type"
    :variant="variant"
    :density="density"
    :disabled="disabled"
    :readonly="readonly"
    :clearable="clearable"
    :prepend-icon="prependIcon"
    :append-icon="appendIcon"
    :prepend-inner-icon="prependInnerIcon"
    :append-inner-icon="appendInnerIcon"
    :loading="loading"
    :focused="focused"
    :rules="rules"
    @update:model-value="handleInput"
    @focus="handleFocus"
    @blur="handleBlur"
    @click="handleClick"
  >
    <template v-if="$slots.prepend" #prepend>
      <slot name="prepend" />
    </template>
    
    <template v-if="$slots.append" #append>
      <slot name="append" />
    </template>
    
    <template v-if="$slots.prependInner" #prepend-inner>
      <slot name="prepend-inner" />
    </template>
    
    <template v-if="$slots.appendInner" #append-inner>
      <slot name="append-inner" />
    </template>
    
    <template v-if="$slots.details" #details>
      <slot name="details" />
    </template>
  </v-text-field>
</template>

<style scoped>
/* MD3 Expressive Input Base Styles */
.md3-expressive-input {
  /* Enhanced typography for better readability */
  --v-field-font-family: var(--md-sys-typescale-body-large-font-family);
  --v-field-font-size: var(--md-sys-typescale-body-large-font-size);
  --v-field-font-weight: var(--md-sys-typescale-body-large-font-weight);
  --v-field-line-height: var(--md-sys-typescale-body-large-line-height);
  --v-field-letter-spacing: var(--md-sys-typescale-body-large-letter-spacing);
}

.md3-expressive-input :deep(.v-field) {
  /* Enhanced border radius */
  border-radius: v-bind(borderRadius);
  
  /* Improved transitions for better UX */
  transition: 
    border-color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    background-color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    box-shadow var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard);
}

/* Expressive enhancements */
.md3-expressive-input--expressive :deep(.v-field) {
  /* Enhanced padding for better touch targets */
  --v-field-padding-start: var(--md-sys-spacing-lg);
  --v-field-padding-end: var(--md-sys-spacing-lg);
  --v-field-padding-top: var(--md-sys-spacing-md);
  --v-field-padding-bottom: var(--md-sys-spacing-md);
}

/* Outlined variant enhancements */
.md3-expressive-input--outlined :deep(.v-field__outline) {
  --v-field-border-width: 1px;
  --v-field-border-opacity: 0.42;
}

.md3-expressive-input--outlined:hover :deep(.v-field__outline) {
  --v-field-border-opacity: 0.87;
}

.md3-expressive-input--outlined :deep(.v-field--focused .v-field__outline) {
  --v-field-border-width: 2px;
  --v-field-border-opacity: 1;
  border-color: rgb(var(--v-theme-primary));
}

/* Filled variant enhancements */
.md3-expressive-input--filled :deep(.v-field) {
  background-color: rgba(var(--v-theme-on-surface), 0.04);
}

.md3-expressive-input--filled:hover :deep(.v-field) {
  background-color: rgba(var(--v-theme-on-surface), 0.08);
}

.md3-expressive-input--filled :deep(.v-field--focused) {
  background-color: rgba(var(--v-theme-on-surface), 0.12);
}

/* Solo variant enhancements */
.md3-expressive-input--solo :deep(.v-field) {
  background-color: rgb(var(--v-theme-surface-container));
  box-shadow: var(--md-sys-elevation-shadow-level1);
}

.md3-expressive-input--solo:hover :deep(.v-field) {
  box-shadow: var(--md-sys-elevation-shadow-level2);
}

/* Label enhancements */
.md3-expressive-input :deep(.v-label) {
  font-family: var(--md-sys-typescale-body-large-font-family);
  font-size: var(--md-sys-typescale-body-large-font-size);
  font-weight: var(--md-sys-typescale-body-large-font-weight);
  letter-spacing: var(--md-sys-typescale-body-large-letter-spacing);
  color: rgba(var(--v-theme-on-surface-variant), 0.87);
  transition: 
    color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard),
    transform var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard);
}

.md3-expressive-input :deep(.v-field--focused .v-label) {
  color: rgb(var(--v-theme-primary));
}

/* Input text enhancements */
.md3-expressive-input :deep(.v-field__input) {
  color: rgb(var(--v-theme-on-surface));
  caret-color: rgb(var(--v-theme-primary));
}

.md3-expressive-input :deep(.v-field__input::placeholder) {
  color: rgba(var(--v-theme-on-surface-variant), 0.6);
}

/* Icon enhancements */
.md3-expressive-input :deep(.v-field__prepend-inner),
.md3-expressive-input :deep(.v-field__append-inner) {
  align-items: center;
  padding-top: 0;
}

.md3-expressive-input :deep(.v-icon) {
  color: rgba(var(--v-theme-on-surface-variant), 0.87);
  transition: color var(--md-sys-motion-duration-short4) var(--md-sys-motion-easing-standard);
}

.md3-expressive-input :deep(.v-field--focused .v-icon) {
  color: rgb(var(--v-theme-primary));
}

/* Error state enhancements */
.md3-expressive-input :deep(.v-field--error) {
  caret-color: rgb(var(--v-theme-error));
}

.md3-expressive-input :deep(.v-field--error .v-field__outline) {
  border-color: rgb(var(--v-theme-error));
}

.md3-expressive-input :deep(.v-field--error .v-label),
.md3-expressive-input :deep(.v-field--error .v-icon) {
  color: rgb(var(--v-theme-error));
}

/* Messages enhancements */
.md3-expressive-input :deep(.v-messages) {
  font-family: var(--md-sys-typescale-body-small-font-family);
  font-size: var(--md-sys-typescale-body-small-font-size);
  font-weight: var(--md-sys-typescale-body-small-font-weight);
  letter-spacing: var(--md-sys-typescale-body-small-letter-spacing);
  margin-top: var(--md-sys-spacing-xs);
}

.md3-expressive-input :deep(.v-messages__message) {
  color: rgba(var(--v-theme-on-surface-variant), 0.87);
}

.md3-expressive-input :deep(.v-field--error .v-messages__message) {
  color: rgb(var(--v-theme-error));
}

/* Disabled state */
.md3-expressive-input :deep(.v-field--disabled) {
  opacity: 0.38;
}

.md3-expressive-input :deep(.v-field--disabled .v-label),
.md3-expressive-input :deep(.v-field--disabled .v-field__input),
.md3-expressive-input :deep(.v-field--disabled .v-icon) {
  color: rgba(var(--v-theme-on-surface), 0.38);
}

/* Loading state enhancements */
.md3-expressive-input :deep(.v-progress-linear) {
  border-radius: 0 0 v-bind(borderRadius) v-bind(borderRadius);
}

/* Clearable button enhancement */
.md3-expressive-input :deep(.v-field__clearable) {
  margin-inline-end: var(--md-sys-spacing-xs);
}

/* Responsive improvements */
@media (max-width: 600px) {
  .md3-expressive-input--expressive :deep(.v-field) {
    --v-field-padding-start: var(--md-sys-spacing-md);
    --v-field-padding-end: var(--md-sys-spacing-md);
  }
}

/* High contrast mode support */
@media (prefers-contrast: high) {
  .md3-expressive-input :deep(.v-field__outline) {
    --v-field-border-width: 2px;
    --v-field-border-opacity: 1;
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .md3-expressive-input :deep(.v-field),
  .md3-expressive-input :deep(.v-label),
  .md3-expressive-input :deep(.v-icon) {
    transition: none !important;
  }
}

/* Dark theme adjustments */
.v-theme--dark .md3-expressive-input--filled :deep(.v-field) {
  background-color: rgba(var(--v-theme-on-surface), 0.06);
}

.v-theme--dark .md3-expressive-input--filled:hover :deep(.v-field) {
  background-color: rgba(var(--v-theme-on-surface), 0.1);
}

.v-theme--dark .md3-expressive-input--filled :deep(.v-field--focused) {
  background-color: rgba(var(--v-theme-on-surface), 0.16);
}
</style>
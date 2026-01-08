<script lang="ts" setup generic="T extends string | number">
interface ButtonOption {
  value: T
  icon?: string
  selectedIcon?: string
  text?: string
  tooltip?: string
}

interface Props {
  modelValue: T
  options: ButtonOption[]
  mandatory?: boolean
  class?: string
}

interface Emits {
  (e: 'update:modelValue', value: T): void
}

const props = defineProps<Props>()
defineEmits<Emits>()

// Function to get the correct icon based on selection state
function getIcon(option: ButtonOption) {
  const isSelected = props.modelValue === option.value
  return isSelected && option.selectedIcon ? option.selectedIcon : option.icon
}
</script>

<template>
  <v-btn-toggle
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :mandatory="mandatory"
    variant="flat"
    color="secondary"
    :class="['connected-button-group']"
  >
    <v-tooltip
      v-for="option in options"
      :key="option.value"
      :text="option.tooltip"
      location="top"
      :disabled="!option.tooltip"
      class="tooltip-plain"
    >
      <template #activator="{ props }">
        <v-btn class="button"
          v-bind="props"
          :value="option.value"
          :icon="getIcon(option)"
          :text="option.text"
          size="small"
        />
      </template>
    </v-tooltip>
  </v-btn-toggle>
</template>

<style scoped>
.connected-button-group {
  gap: 2px;
}

.v-btn-group .v-btn {
  border-radius: 8px;
}

.connected-button-group :deep(.v-btn--active) {
  border-radius: 9999px !important;
}

.button {
  background-color: rgb(var(--v-theme-secondary-container));
}
</style>

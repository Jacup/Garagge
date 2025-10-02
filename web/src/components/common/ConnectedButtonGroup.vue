<script lang="ts" setup generic="T extends string | number">
interface ButtonOption {
  value: T
  icon?: string
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

defineProps<Props>()
defineEmits<Emits>()
</script>

<template>
  <v-btn-toggle
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :mandatory="mandatory"
    variant="flat"
    color="secondary"
    :class="['connected-button-group', $props.class]"
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
        <v-btn
          v-bind="props"
          :value="option.value"
          :icon="option.icon"
          :text="option.text"
        />
      </template>
    </v-tooltip>
  </v-btn-toggle>
</template>

<style scoped>
.connected-button-group {
  gap: 2px;
  border-radius: 50px;
  overflow: hidden;
}

.connected-button-group :deep(.v-btn) {
  border-radius: 0;
  min-width: 40px;
}

.connected-button-group :deep(.v-btn:first-child) {
  border-radius: 50px 8px 8px 50px;
}

.connected-button-group :deep(.v-btn:last-child) {
  border-radius: 8px 50px 50px 8px;
}

.connected-button-group :deep(.v-btn:not(:first-child):not(:last-child)) {
  border-radius: 8px;
}

.connected-button-group :deep(.v-btn--active) {
  border-radius: 50px !important;
}
</style>

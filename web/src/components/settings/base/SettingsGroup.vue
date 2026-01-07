<script lang="ts" setup>
defineProps<{
  value: string
  title: string
  subtitle?: string
  icon: string
}>()
</script>

<template>
  <v-list-group :value="value" class="settings-group-wrapper">
    <template v-slot:activator="{ props, isOpen }">
      <v-list-item
        v-bind="props"
        lines="two"
        :title="title"
        :subtitle="subtitle"
        :prepend-icon="icon"
        class="group-activator"
        :class="{ 'is-open': isOpen }"
      />
    </template>

    <slot></slot>
  </v-list-group>
</template>

<style scoped>
.settings-group-wrapper {
  margin-bottom: 16px;
}

.group-activator {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 12px !important;
  transition:
    border-radius 0.2s ease-in-out,
    margin-bottom 0.2s;
}

.group-activator.is-open {
  border-bottom-left-radius: 4px !important;
  border-bottom-right-radius: 4px !important;
  margin-bottom: 2px !important;
}

:deep(.inner-item) {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 4px !important;
  margin-bottom: 2px !important;
  padding-inline-start: 32px !important;
}

:deep(.v-list-group__items .inner-item:first-child) {
  border-top-left-radius: 4px !important;
  border-top-right-radius: 4px !important;
}

:deep(.v-list-group__items .inner-item:last-child) {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
  margin-bottom: 0 !important;
}
</style>

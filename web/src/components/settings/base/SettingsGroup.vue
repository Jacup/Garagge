<script lang="ts" setup>
defineProps<{
  value: string
  title: string
  subtitle?: string
  icon: string
}>()
</script>

<template>
  <v-list-group :value="value" class="md3-list-group">
    <template v-slot:activator="{ props, isOpen }">
      <v-list-item
        v-bind="props"
        lines="two"
        :title="title"
        :subtitle="subtitle"
        :prepend-icon="icon"
        class="md3-list-group-activator"
        :class="{ 'is-open': isOpen }"
      />
    </template>

    <slot></slot>
  </v-list-group>
</template>

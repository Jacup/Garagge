<script lang="ts" setup>
defineProps<{
  title: string
}>()
</script>

<template>
  <v-card class="mb-6" variant="elevated" color="surface-container">
    <v-card-title class="d-flex align-center">
      <span class="text-h5">{{ title }}</span>
      <v-spacer />
    </v-card-title>
    <v-card-text class="pt-0">
      <slot />
    </v-card-text>
  </v-card>
</template>

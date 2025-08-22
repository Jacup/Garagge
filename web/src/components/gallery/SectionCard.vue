<script lang="ts" setup>
defineProps<{
  title: string
}>()
</script>

<template>
  <v-card class="mb-6" variant="outlined">
    <v-card-title class="d-flex align-center">
      <span class="text-h5">{{ title }}</span>
      <v-spacer />
    </v-card-title>
    <v-card-text class="pt-0">
      <slot />
    </v-card-text>
  </v-card>
</template>

<style scoped>
.v-card-title {
  background-color: rgb(var(--md-sys-color-surface-container));
  color: rgb(var(--md-sys-color-on-surface));
}
</style>

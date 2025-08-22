<script lang="ts" setup>
defineProps<{
  title: string
}>()
</script>

<template>
  <v-card variant="tonal" class="h-100">
    <v-card-title class="text-subtitle-1">{{ title }}</v-card-title>
    <v-card-text>
      <slot />
    </v-card-text>
  </v-card>
</template>

<style scoped>
.v-card {
  border: 1px solid rgb(var(--md-sys-color-outline-variant));
}
</style>

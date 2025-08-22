<script lang="ts" setup>
defineProps<{
  title: string
}>()
</script>

<template>
  <v-card variant="outlined" class="h-100">
    <v-card-title class="text-subtitle-1">{{ title }}</v-card-title>
    <v-card-text>
      <slot />
    </v-card-text>
  </v-card>
</template>

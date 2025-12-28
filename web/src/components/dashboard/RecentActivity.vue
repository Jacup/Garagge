<script lang="ts" setup>
import type { Ref } from 'vue'
defineProps<{
  recentActivity: Ref<
    Array<{
      title: string
      subtitle: string
      date: string
      icon: string
      color: string
    }>
  >
}>()
</script>

<template>
  <v-card class="card-background" variant="flat" rounded="md-16px">
    <v-card-title> Recent Activity </v-card-title>
    <v-card-text>
      <v-timeline direction="vertical" side="end">
        <v-timeline-item v-for="(item, index) in recentActivity" :key="index">
          <template v-slot:icon>
            <v-avatar size="40" :color="item.color" variant="tonal">
              <v-icon :icon="item.icon" />
            </v-avatar>
          </template>
          <div class="text-body-2 font-weight-medium">{{ item.title }}</div>
          <div class="text-caption text-medium-emphasis">{{ item.subtitle }}</div>
        </v-timeline-item>
      </v-timeline>
    </v-card-text>
  </v-card>
</template>

<style scoped>
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>

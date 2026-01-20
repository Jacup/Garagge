<script setup lang="ts">
interface Props {
  title: string
  titlePrependIcon: string
  subtitle?: string
  value: string | number
  valueAppend?: string
  trendIcon?: string
  trend?: string
}

defineProps<Props>()
</script>
<template>
  <v-card class="energy-stat-card" variant="flat" :prepend-icon="titlePrependIcon" :title="title" :subtitle="subtitle">
    <v-card-text>
      <div class="d-flex align-center mb-1">
        <v-icon v-if="trendIcon" :icon="trendIcon" size="18" color="outline" class="me-1" />
        <span class="text-caption text-medium-emphasis">
          <span v-if="trend" class="me-1">{{ trend }}</span>
          {{ subtitle }}
        </span>
      </div>

      <div class="d-flex align-baseline">
        <span class="text-h4 font-weight-bold">
          {{ value }}
        </span>
        <span v-if="valueAppend" class="text-h6 ms-1 font-weight-medium text-medium-emphasis">
          {{ valueAppend }}
        </span>
      </div>
    </v-card-text>
  </v-card>
</template>

<style lang="scss" scoped>
.energy-stat-card {
  .color-on-surface {
    color: rgb(var(--v-theme-on-surface));
  }
}
</style>

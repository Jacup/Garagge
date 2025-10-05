<script setup lang="ts">
interface Props {
  icon: string
  label: string
  value: string | number | null | undefined
  iconSize?: string | number
}

withDefaults(defineProps<Props>(), {
  iconSize: 20,
})
</script>

<template>
  <div class="details-item">
    <div class="details-item-icon">
      <v-icon :icon="icon" :size="iconSize" />
    </div>
    <div class="details-item-data">
      <span class="details-item-label">{{ label }}</span>
      <span class="details-item-value">{{ value || 'N/A' }}</span>
    </div>
  </div>
</template>

<style scoped lang="scss">
.details-item {
  display: flex;
  flex-direction: row;
  gap: 8px;
  align-items: center;
}

.details-item-data {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.details-item-label {
  font-weight: 400;
  color: rgba(var(--v-theme-on-surface-variant), 0.8);
}

.details-item-label {
  font-weight: 400;
}
</style>

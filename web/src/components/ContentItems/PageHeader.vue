<script lang="ts" setup>
defineProps<{
  title?: string
}>()
</script>

<template>
  <div class="page-header-container">
    <div class="title">
      <span class="page-header">{{ title }}</span>
    </div>
    <div class="header-actions">
      <slot name="actions" />
    </div>
  </div>
</template>

<style scoped>
.page-header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.title {
  flex: 1;
}

.page-header {
  display: flex;
  font-optical-sizing: auto;
  font-weight: 500;
  font-style: normal;
  font-size: 2rem;
  color: #fff;
}

.header-actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
</style>

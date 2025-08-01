<script lang="ts" setup>
defineProps<{
  title?: string
}>()
</script>

<template>
  <div class="title">
    <span class="page-header">{{ title }}</span>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  font-optical-sizing: auto;
  font-weight: 500;
  font-style: normal;
  font-size: 2rem;
  color: #fff;
}
</style>

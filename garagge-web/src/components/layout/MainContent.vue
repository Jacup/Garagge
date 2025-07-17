<script lang="ts" setup>
// Placeholder na RouterView albo content
</script>

<template>
  <main class="main-content">
    <!-- Tutaj będzie dashboard -->
    <RouterView />
  </main>
</template>

<style scoped>
.main-content {
  flex: 1;
  height: 100%;
  overflow-y: auto;
  background-color: var(--color-background);
  color: var(--color-text);
  padding: 2rem;
}
</style>

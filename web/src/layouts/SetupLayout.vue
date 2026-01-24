<script lang="ts" setup></script>

<template>
  <v-main class="setup-layout">
    <div class="container">
      <router-view />
    </div>
  </v-main>
</template>

<style scoped>
.setup-layout {
  display: flex;
  align-items: center;
  justify-content: center;
}

.container {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  padding: 32px;
  border-radius: 28px;
  min-width: 400px;
  max-width: 600px;
  min-height: 280px;
  max-width: 600px;
}
</style>

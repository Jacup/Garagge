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
  padding: 16px;
}

.container {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  padding: 24px;
  border-radius: 28px;
  width: 100%;
  max-width: 560px;
}

@media (max-width: 600px) {
  .container {
    display: flex;
    flex-direction: column;
    justify-content: center;
  }
}
</style>

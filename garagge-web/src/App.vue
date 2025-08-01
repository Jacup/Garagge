<script lang="ts" setup>
import MainContent from './components/layout/MainContent.vue'
import NavBar from './components/layout/NavBar.vue'
</script>

<template>
  <div class="app-layout">
    <NavBar />
    <MainContent />
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  height: 100vh;
  overflow: hidden;
}
</style>

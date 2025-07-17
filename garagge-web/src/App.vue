<script lang="ts" setup>
import Sidebar from './components/layout/SideBar.vue'
import MainContent from './components/layout/MainContent.vue'
</script>

<template>
  <div class="app-layout">
    <Sidebar />
    <MainContent />
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  height: 100vh; /* ważne: pełna wysokość */
  overflow: hidden; /* nie pozwala wyciekać scrollom na X */
}
</style>

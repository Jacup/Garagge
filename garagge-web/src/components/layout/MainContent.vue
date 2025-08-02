<script lang="ts" setup>
import PageHeader from '@/components/ContentItems/PageHeader.vue'
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const pageTitle = computed(() => {
  const titleMap: Record<string, string> = {
    Dashboard: 'Dashboard',
    Vehicles: 'Vehicles',
    Login: 'Login',
    Register: 'Register',
  }

  return titleMap[route.name as string] || 'Page'
})
</script>

<template>
  <main class="main-content">
    <div class="page-header-section">
      <PageHeader :title="pageTitle" />
    </div>

    <div class="page-body">
      <RouterView />
    </div>
  </main>
</template>

<style scoped>
.main-content {
  flex: 1;
  height: 100%;
  display: flex;
  flex-direction: column;
  background-color: var(--color-background);
  color: var(--color-text);
}

.page-header-section {
  padding: 2rem 2rem 1rem 2rem;
  border-bottom: 1px solid var(--color-border, rgba(255, 255, 255, 0.1));
}

.page-body {
  flex: 1;
  overflow-y: auto;
  padding: 2rem;
}
</style>

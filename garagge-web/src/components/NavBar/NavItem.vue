<script lang="ts" setup>
import { RouterLink } from 'vue-router'

defineProps<{
  icon?: string
  text: string
  path: string
}>()
</script>

<template>
  <RouterLink :to="path" class="nav-item" active-class="router-link-active">
    <div v-if="icon" class="nav-icon">
      <span class="material-icons-outlined">{{ icon }}</span>
    </div>
    <span class="nav-text">{{ text }}</span>
  </RouterLink>
</template>

<style scoped>
.nav-item {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: left;
  padding: 0.5rem;
  gap: 0.5rem;
  border-radius: 12px;
  color: var(--color-text-muted);
}

.nav-item.router-link-active,
.nav-item.router-link-active:hover {
  background-color: var(--color-accent);
  color: var(--color-text);
  opacity: 1;
}

.nav-item:hover {
  background-color: var(--color-card-contrast);
  color: var(--color-text);
}

.nav-icon {
  align-items: center;
  justify-content: center;
  height: 24px;
}
</style>

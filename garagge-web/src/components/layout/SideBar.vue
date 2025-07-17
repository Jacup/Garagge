<script lang="ts" setup>
import { useUserStore } from '@/stores/userStore'
import { RouterLink, useRouter } from 'vue-router'
import { computed } from 'vue'

const userStore = useUserStore()
const isLoggedIn = computed(() => !!userStore.accessToken)
const router = useRouter()

const handleLogout = () => {
  userStore.clearToken() // Wywołaj akcję czyszczącą store i localStorage
  router.push('/') // Przekieruj na stronę logowania
}
</script>

<template>
  <aside class="sidebar">
    <!-- TOP: Logo / Brand -->
    <div class="sidebar-header">
      <span class="logo">MyApp</span>
    </div>

    <!-- NAV: Główna nawigacja -->
    <nav class="sidebar-nav">
      <RouterLink to="/" class="nav-item">Dashboard</RouterLink>
    </nav>

    <!-- BOTTOM: User info or auth buttons -->
    <div class="sidebar-footer">
      <div v-if="isLoggedIn" class="user-info">
        <div class="avatar"></div>
        <span class="username">{{ userStore.firstName }} {{ userStore.lastName }}</span>
        <button class="auth-buttons" @click="handleLogout">Wyloguj</button>
      </div>
      <div v-else class="auth-buttons">
        <RouterLink to="/login" class="auth-btn">Zaloguj</RouterLink>
        <RouterLink to="/register" class="auth-btn">Zarejestruj</RouterLink>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 240px;
  height: 100%;
  background-color: var(--color-sidebar);
  color: var(--color-text);
  display: flex;
  flex-direction: column;
  justify-content: space-between; /* header/top nav/footer */
  padding: 1.5rem 1rem;
}

.sidebar-header {
  font-size: 1.5rem;
  font-weight: bold;
  color: #fff;
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  margin-top: 2rem;
}

.nav-item {
  color: #cbd5e1; /* slate-300 */
  text-decoration: none;
  margin-bottom: 1rem;
}

.nav-item:hover {
  color: #fff;
}

.sidebar-footer {
  border-top: 1px solid #334155; /* slate-700 */
  padding-top: 1rem;
  display: flex;
  align-items: center;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #cbd5e1;
}

.avatar {
  width: 32px;
  height: 32px;
  background-color: #64748b; /* slate-500 */
  border-radius: 50%;
}

.auth-buttons {
  display: flex;
  gap: 0.5rem;
}

.auth-btn {
  background: #334155;
  color: #cbd5e1;
  border: none;
  border-radius: 4px;
  padding: 0.5rem 1rem;
  text-decoration: none;
  cursor: pointer;
  transition: background 0.2s;
}

.auth-btn:hover {
  background: #475569;
  color: #fff;
}
</style>

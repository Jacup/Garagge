<script lang="ts" setup>
import { useUserStore } from '@/stores/userStore'
import { ref, computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'

const userStore = useUserStore()
const router = useRouter()
const isLoggedIn = computed(() => !!userStore.accessToken)
const isOpen = ref(false)

const handleLogout = () => {
  userStore.clearToken()
  router.push('/')
}

const toggleMenu = () => {
  isOpen.value = !isOpen.value
}
</script>

<template>
  <div v-if="isLoggedIn" class="auth-info" @click.stop="toggleMenu">
    <!-- TODO: delete IF statement - navbar wont be visible if not logged in. -->

    <div class="avatar"></div>
    <div class="user-details">
      <span class="username">{{ userStore.firstName }} {{ userStore.lastName }}</span>
      <span class="userrole">Admin</span>
    </div>
    <span class="expand">⋮</span>

    <div v-if="isOpen" class="auth-dropdown">
      <RouterLink to="/profile" class="dropdown-item">Profil</RouterLink>
      <RouterLink to="/" class="dropdown-item" @click.prevent="handleLogout">Wyloguj</RouterLink>
    </div>
  </div>
  <div v-else class="auth-buttons">
    <RouterLink to="/login" class="auth-btn">Zaloguj</RouterLink>
    <RouterLink to="/register" class="auth-btn">Zarejestruj</RouterLink>
  </div>
</template>

<style scoped>
.auth-info {
  margin-top: auto;
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 100%;
  gap: 0.5rem;
  padding: 0.5rem;
  background-color: var(--color-card-contrast);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  cursor: pointer;
}

.avatar {
  width: 48px;
  height: 48px;
  background-color: #81868d;
  object-fit: cover;
  border-radius: 50%;
}

.user-details {
  display: flex;
  flex-direction: column;
}

.username {
  color: var(--color-text);
  font-weight: 600;
}

.userrole {
  color: var(--color-text-muted);
  font-size: 0.8rem;
}

.expand {
  margin-left: auto;
  margin-right: 0.5rem;
  color: var(--color-text-muted);
  font-size: 1.5rem;
}

.auth-buttons {
  display: flex;
  flex-direction: row;
  align-items: center;
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

.auth-info {
  position: relative; /* Ważne! */
}

.auth-dropdown {
  position: absolute;
  bottom: 100%;
  right: 0;
  width: 100%;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 0.5rem 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  z-index: 1000;
  margin-bottom: 0.5rem;
}

.dropdown-item {
  padding: 0.5rem 1rem;
  color: var(--color-text);
  text-decoration: none;
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
}

.dropdown-item:hover {
  background: var(--color-card-contrast);
}
</style>

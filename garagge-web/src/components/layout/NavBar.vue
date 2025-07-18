<script lang="ts" setup>
import AccountMenu from '../NavBar/AccountMenu.vue'
import NavigationMenu from '../NavBar/NavigationMenu.vue'
</script>

<template>
  <aside class="navbar">
    <!-- TOP: Logo / Brand -->
    <div class="navbar-header">
      <span class="logo">MyApp</span>
    </div>
    <NavigationMenu />
    <AccountMenu />
  </aside>
</template>

<style scoped>
.navbar {
  background: var(--color-card);
  color: var(--color-text);
  width: 255px;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 1rem;
}

.navbar-header {
  font-size: 1.5rem;
  font-weight: bold;
  color: #fff;
}

.navbar-nav {
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
</style>

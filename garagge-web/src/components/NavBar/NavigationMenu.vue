<script lang="ts"></script>

<template>
  <nav class="navbar-nav">
    <RouterLink to="/" class="nav-item">Dashboard</RouterLink>
  </nav>
</template>

<style scoped>
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

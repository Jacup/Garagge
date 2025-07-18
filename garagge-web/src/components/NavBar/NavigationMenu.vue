<script lang="ts" setup>
import NavItem from './NavItem.vue'
</script>

<template>
  <nav class="navbar-nav">
    <NavItem icon="dashboard" text="Dashboard" path="/" />
    <NavItem icon="directions_car" text="Vehicles" path="/vehicles" />
  </nav>
  <hr class="nav-separator" />
  <nav class="navbar-nav">
    <NavItem icon="help" text="Help" path="/help" />
    <NavItem icon="report" text="Report" path="/report" />
    <NavItem icon="settings" text="Settings" path="/settings" />
  </nav>
</template>

<style scoped>
.navbar-nav {
  display: flex;
  flex-direction: column;
  width: 100%;
  gap: 0.5rem;
}

.nav-separator {
  border: none;
  border-top: 1px solid var(--color-border);
}
</style>

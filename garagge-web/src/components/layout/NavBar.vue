<script lang="ts" setup>
import TitleBar from '../NavBar/TitleBar.vue'
import SearchBox from '../NavBar/SearchBox.vue'
import AccountMenu from '../NavBar/AccountMenu.vue'
import NavigationMenu from '../NavBar/NavigationMenu.vue'
</script>

<template>
  <aside class="navbar">
    <TitleBar />
    <SearchBox />
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
  gap: 1rem;
}
</style>

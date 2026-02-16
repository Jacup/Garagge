<script lang="ts" setup>
import SettingsGroup from '@/components/settings/base/SettingsGroup.vue'
</script>

<template>
  <div class="settings-container">
    <SettingsGroup value="app-settings" title="App settings" icon="mdi-cog-outline"> </SettingsGroup>
  </div>
</template>

<style scoped>
.settings-container {
  display: flex;
  flex-direction: column;
}
</style>

<script lang="ts" setup>
import ServicesGroup from '@/components/settings/administration/ServicesGroup.vue'
import SettingsGroup from '@/components/settings/base/SettingsGroup.vue'
</script>

<template>
  <div class="settings-container">
    <SettingsGroup value="users-settings" title="Users settings" subtitle="Manage users" icon="mdi-account-cog-outline"> </SettingsGroup>

    <SettingsGroup
      value="services-settings"
      title="Services settings"
      subtitle="Manage services configuration"
      icon="mdi-wrench-cog-outline"
    >
      <ServicesGroup />
    </SettingsGroup>
  </div>
</template>

<style scoped>
.settings-container {
  display: flex;
  flex-direction: column;
}
</style>

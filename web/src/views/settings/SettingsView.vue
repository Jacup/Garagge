<script lang="ts" setup>
import SettingsGroup from '@/components/settings/base/SettingsGroup.vue'
import AppSettings from '@/components/settings/sections/AppSettings.vue'
import UserSettings from '@/components/settings/sections/UserSettings.vue'
import SessionSettings from '@/components/settings/sections/SessionsSettings.vue'
</script>

<template>
  <v-list lines="two" class="settings-container">
    <SettingsGroup value="app-settings" title="App settings" icon="mdi-cog-outline">
      <AppSettings />
    </SettingsGroup>

    <SettingsGroup value="user-settings" title="User settings" subtitle="Manage your account" icon="mdi-account-outline">
      <UserSettings />
    </SettingsGroup>

    <SettingsGroup value="your-devices" title="Your devices" subtitle="Manage your active sessions" icon="mdi-monitor-cellphone">
      <SessionSettings />
    </SettingsGroup>
  </v-list>
</template>

<style scoped>
.settings-container {
  padding: 0;
  margin: 0 auto;
}
</style>

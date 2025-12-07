<script lang="ts" setup>
import { computed, ref } from 'vue'
import { useSettingsStore } from '@/stores/settings'

const settingsStore = useSettingsStore()

const theme = computed({
  get: () => settingsStore.settings.theme,
  set: (value) => settingsStore.setTheme(value),
})

const selectedLanguage = ref('pl')

const languages = [
  { title: 'Polski', value: 'pl' },
  { title: 'English', value: 'en' },
]

const sessions = ref([
  {
    id: 1,
    isCurrent: true,
    os: 'windows',
    browser: 'Chrome',
    deviceName: 'Chrome on Windows',
    lastSeen: 'Now',
    location: 'Gdynia, PL',
    ip: '192.168.1.1',
  },
  {
    id: 2,
    isCurrent: false,
    os: 'android',
    browser: 'Mobile App',
    deviceName: 'Pixel 7 Pro',
    lastSeen: '2 hours ago',
    location: 'Warsaw, PL',
    ip: '10.0.0.15',
  },
  {
    id: 3,
    isCurrent: false,
    os: 'mac',
    browser: 'Safari',
    deviceName: 'MacBook Air',
    lastSeen: '5 days ago',
    location: '',
    ip: '172.16.0.5',
  },
  {
    id: 4,
    isCurrent: false,
    os: 'Unknown OS',
    browser: 'Unknown Browser',
    deviceName: 'Unknown Device',
    lastSeen: '5 days ago',
    location: 'Gdynia, PL',
    ip: '172.16.0.5',
  },
])

const getDeviceIcon = (os: string) => {
  switch (os.toLowerCase()) {
    case 'windows':
      return 'mdi-microsoft-windows'
    case 'mac':
      return 'mdi-apple'
    case 'android':
      return 'mdi-android'
    case 'ios':
      return 'mdi-apple-ios'
    case 'linux':
      return 'mdi-linux'
    default:
      return 'mdi-laptop'
  }
}

const logoutSession = (id: number) => {
  console.log('Logging out session', id)
  // Logic to remove session
}

const logoutOtherSessions = () => {
  console.log('Logging out all other sessions')
}
</script>

<template>
  <v-list lines="two" class="settings-list">
    <v-list-group value="app-settings" class="settings-group">
      <template v-slot:activator="{ props, isOpen }">
        <v-list-item
          v-bind="props"
          title="App settings"
          prepend-icon="mdi-cog-outline"
          class="group-activator"
          :class="{ 'is-open': isOpen }"
        />
      </template>

      <v-list-item title="App Theme" subtitle="Select your preference" class="inner-item">
        <template #prepend>
          <v-icon
            :icon="theme === 'light' ? 'mdi-white-balance-sunny' : theme === 'dark' ? 'mdi-moon-waning-crescent' : 'mdi-brightness-auto'"
            class="ml-2 mr-4 text-medium-emphasis"
          ></v-icon>
        </template>
        <template #append>
          <v-btn-toggle v-model="theme" mandatory color="primary" variant="outlined" rounded="pill">
            <v-btn value="light" icon="mdi-white-balance-sunny"></v-btn>
            <v-btn value="dark" icon="mdi-moon-waning-crescent"></v-btn>
            <v-btn value="system" icon="mdi-brightness-auto"></v-btn>
          </v-btn-toggle>
        </template>
      </v-list-item>

      <v-list-item title="Language" class="inner-item">
        <template #prepend>
          <v-icon icon="mdi-translate" class="ml-2 mr-4 text-medium-emphasis"></v-icon>
        </template>
        <template #append>
          <div style="min-width: 120px">
            <v-select
              v-model="selectedLanguage"
              :items="languages"
              variant="outlined"
              density="compact"
              disabled
              hide-details
              single-line
            ></v-select>
          </div>
        </template>
      </v-list-item>
    </v-list-group>

    <v-list-group value="user-settings" class="settings-group">
      <template v-slot:activator="{ props, isOpen }">
        <v-list-item
          v-bind="props"
          lines="two"
          title="User settings"
          prepend-icon="mdi-account-outline"
          subtitle="Manage your account"
          class="group-activator"
          :class="{ 'is-open': isOpen }"
        />
      </template>
      <v-list-item title="Profile" class="list-item inner-item">
        <template #append><v-icon icon="mdi-chevron-right"></v-icon></template>
      </v-list-item>
    </v-list-group>

    <v-list-group value="your-devices" class="settings-group">
      <template v-slot:activator="{ props, isOpen }">
        <v-list-item
          v-bind="props"
          title="Your devices"
          subtitle="Manage your active sessions"
          prepend-icon="mdi-monitor-cellphone"
          class="group-activator"
          :class="{ 'is-open': isOpen }"
        />
      </template>

      <v-list-item v-for="session in sessions" :key="session.id" class="list-item inner-item device-item" lines="two">
        <template #prepend>
          <div class="d-flex align-center justify-center" style="width: 40px">
            <v-icon :icon="getDeviceIcon(session.os)" :color="session.isCurrent ? 'primary' : 'medium-emphasis'"></v-icon>
          </div>
        </template>

        <v-list-item-title>
          {{ session.deviceName }}
          <v-chip v-if="session.isCurrent" size="x-small" color="primary" variant="outlined" class="suggestion-chip ml-2">
            Current Device
          </v-chip>
        </v-list-item-title>

        <v-list-item-subtitle class="text-caption mt-1">
          <span v-if="session.isCurrent" class="text-primary">Active now</span>
          <span v-else>{{ [session.browser, session.location, session.lastSeen].filter((i) => i).join(' • ') }}</span>
        </v-list-item-subtitle>

        <template #append>
          <v-btn
            v-if="!session.isCurrent"
            icon="mdi-logout"
            variant="text"
            color="error"
            density="comfortable"
            @click.stop="logoutSession(session.id)"
            v-tooltip:start="'Logout'"
          ></v-btn>
        </template>
      </v-list-item>

      <v-list-item class="inner-item">
        <template #append>
          <v-btn variant="flat" color="error" prepend-icon="mdi-shield-remove-outline" @click="logoutOtherSessions">
            Sign out all sessions
          </v-btn>
        </template>
      </v-list-item>
    </v-list-group>
  </v-list>
</template>

<style scoped>
.settings-list {
  padding: 0;
}
.settings-group {
  margin-bottom: 16px;
}

.group-activator,
:deep(.inner-item) {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.group-activator {
  border-radius: 12px !important;
  transition:
    border-radius 0.2s ease-in-out,
    margin-bottom 0.2s;
}

.v-list-group--open .group-activator {
  border-bottom-left-radius: 2px !important;
  border-bottom-right-radius: 2px !important;
  margin-bottom: 2px !important;
}

:deep(.inner-item) {
  margin-bottom: 2px !important;
  border-radius: 2px !important;
  padding-inline-start: 32px !important;
}

:deep(.v-list-group__items .inner-item:first-child) {
  border-top-left-radius: 2px !important;
  border-top-right-radius: 2px !important;
}

:deep(.v-list-group__items .inner-item:last-child) {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
}

:deep(.inner-item .v-list-item__prepend) {
  justify-content: center;
}

:deep(.device-item .v-list-item__prepend) {
  margin-right: 16px;
}

:deep(.device-item):has(.v-chip) {
  background-color: rgba(var(--v-theme-primary), 0.12) !important;
}

.suggestion-chip {
  border-color: rgb(var(--v-theme-outline)) !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-radius: 8px !important;
  font-weight: 500;
}
</style>

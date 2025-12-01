<script lang="ts" setup>
import { computed, ref } from 'vue'
import { useUserStore } from '@/stores/userStore'

const userStore = useUserStore()

const theme = computed({
  get: () => userStore.settings.theme,
  set: (value) => userStore.updateSettings({ theme: value }),
})

// Stan ustawień
const selectedLanguage = ref('pl')

// Opcje języków (mock)
const languages = [
  { title: 'Polski', value: 'pl' },
  { title: 'English', value: 'en' },
]
</script>

<template>
  <v-list lines="two" class="settings-list">
    <v-list-group value="app-settings" class="settings-group mb-4">
      <template v-slot:activator="{ props, isOpen }">
        <v-list-item
          v-bind="props"
          title="App settings"
          prepend-icon="mdi-cog-outline"
          class="group-activator"
          :class="{ 'is-open': isOpen }"
        />
      </template>

      <v-list-item title="App Theme" subtitle="Select your preference" class="list-item inner-item">
        <template #prepend>
          <v-icon
            :icon="
              theme === 'light'
                ? 'mdi-white-balance-sunny'
                : theme === 'dark'
                  ? 'mdi-moon-waning-crescent'
                  : 'mdi-brightness-auto'
            "
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
  </v-list>
</template>

<style scoped>
.settings-list {
  padding: 0;
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
</style>

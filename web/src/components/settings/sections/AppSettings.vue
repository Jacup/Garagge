<script lang="ts" setup>
import { computed, ref } from 'vue'
import { useSettingsStore } from '@/stores/settings'

const settingsStore = useSettingsStore()

const theme = computed({
  get: () => settingsStore.settings.theme,
  set: (value) => settingsStore.setTheme(value),
})

const selectedLanguage = ref('en')

const languages = [
  { title: 'English', value: 'en' },
  { title: 'Polski', value: 'pl' },
]
</script>

<template>
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
</template>

<script lang="ts" setup>
import { computed } from 'vue'

// Props
const { isMobile } = defineProps<{
  isMobile?: boolean
}>()

// Emits
const emit = defineEmits<{
  open: []
  voiceSearch: []
}>()

const isMac = computed(() => {
  return typeof navigator !== 'undefined' && /Mac|iPod|iPhone|iPad/.test(navigator.platform)
})

const shortcutKey = computed(() => {
  return isMac.value ? '⌘' : 'Ctrl'
})

function handleVoiceSearch() {
  emit('voiceSearch')
}
</script>

<template>
  <div class="search-container" :class="{ 'search-container--mobile': isMobile }">
    <v-btn
      variant="flat"
      rounded="pill"
      height="48px"
      class="search-button"
      :class="{ 'search-button--mobile': isMobile }"
      @click="emit('open')"
    >
      <template #prepend>
        <v-icon>mdi-magnify</v-icon>
      </template>

      Search

      <template #append>
        <div v-if="!isMobile" class="hotkey-container">
          <kbd class="hotkey-key">{{ shortcutKey }}</kbd>
          <kbd class="hotkey-key">K</kbd>
        </div>
        <v-btn
          v-else
          icon="mdi-microphone"
          variant="text"
          size="small"
          @click.stop="handleVoiceSearch"
          class="voice-search-btn"
          aria-label="Voice search"
        />
      </template>
    </v-btn>
  </div>
</template>

<style scoped>
.search-container {
  width: 100%;
  max-width: 312px;
  container-type: inline-size;
}

@media (min-width: 768px) {
  .search-container:not(.search-container--mobile) {
    width: min(100%, max(312px, 312px + (100vw - 312px - 200px) * 0.5));
    max-width: 800px;
  }
}

.search-container--mobile {
  width: 100%;
  max-width: none;
  flex: 1;
}

.search-button {
  width: 100%;
  text-transform: none;
  background-color: rgb(var(--v-theme-surface-container));
  justify-content: space-between;
}

.search-button--mobile {
  background-color: rgb(var(--v-theme-surface-container));
}

.search-button :deep(.v-btn__content) {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.search-button :deep(.v-btn__prepend) {
  margin-inline-end: 0;
}

.search-button :deep(.v-btn__append) {
  margin-inline-start: 0;
}

.voice-search-btn {
  opacity: 0.7;
  transition: opacity 0.2s ease;
}

.voice-search-btn:hover {
  opacity: 1;
}

.hotkey-container {
  display: flex;
  gap: 2px;
}

.hotkey-key {
  background-color: rgb(var(--v-theme-surface-container-high));
  border: 1px solid rgb(var(--v-theme-outline-variant));
  border-radius: 4px;
  padding: 2px 6px;
  font-size: 0.75rem;
  font-family: monospace;
  color: rgb(var(--v-theme-on-surface-variant));
  min-width: 20px;
  text-align: center;
  line-height: 1.2;
}
</style>

<script lang="ts" setup>
import { computed } from 'vue'

const { isMobile } = defineProps<{
  isMobile?: boolean
}>()

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
  <div class="search-button-container">
    <v-btn
      variant="flat"
      rounded="pill"
      height="56px"
      class="search-button"
      @click="emit('open')"
    >
      Search

      <template #append>
        <div v-if="!isMobile" class="hotkey-container">
          <kbd class="hotkey-key">{{ shortcutKey }} + K</kbd>
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
.search-button-container {
  width: min(100%, max(312px, 50%));
}

.search-button {
  width: 100%;
  text-transform: none;
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  justify-content: space-between;
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

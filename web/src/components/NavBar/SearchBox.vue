<script lang="ts" setup>
import { ref, computed, watch } from 'vue'
import { useHotkey } from 'vuetify'

const { isRail } = defineProps<{
  isRail?: boolean
}>()

const emit = defineEmits<{
  search: [query: string]
}>()

const searchText = ref('')
const isOverlayOpen = ref(false)

const isMac = computed(() => {
  return typeof navigator !== 'undefined' && /Mac|iPod|iPhone|iPad/.test(navigator.platform)
})

const shortcutKey = computed(() => {
  return isMac.value ? '⌘' : 'Ctrl'
})

function openSearchOverlay() {
  isOverlayOpen.value = true
}

function closeSearchOverlay() {
  isOverlayOpen.value = false
  searchText.value = ''
}

// Simple debounce for search
let searchTimeout: ReturnType<typeof setTimeout>

function searchWithDebounce(query: string) {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    emit('search', query)
  }, 300)
}

// Watch for search text changes
watch(searchText, (newValue) => {
  if (newValue.trim()) {
    searchWithDebounce(newValue)
  }
})

useHotkey('cmd+k', (event) => {
  event.preventDefault()
  event.stopPropagation()
  event.stopImmediatePropagation()

  openSearchOverlay()

  return false
})

useHotkey('escape', () => {
  if (isOverlayOpen.value) {
    closeSearchOverlay()
  }
})
</script>
<template>
  <v-btn v-if="isRail" variant="text" icon="mdi-magnify" class="search-button-rail" @click="openSearchOverlay" rounded="4" />

  <v-btn v-else variant="outlined" prepend-icon="mdi-magnify" class="search-button" @click="openSearchOverlay" block rounded="4">
    Search...
    <template #append>
      <div class="hotkey-container">
        <kbd class="hotkey-key">{{ shortcutKey }}</kbd>
        <kbd class="hotkey-key">K</kbd>
      </div>
    </template>
  </v-btn>

  <v-overlay
    v-model="isOverlayOpen"
    class="search-overlay"
    role="dialog"
    aria-labelledby="search-modal-title"
    aria-modal="true"
  >
    <div class="search-modal">
      <h2 id="search-modal-title" class="sr-only">Search</h2>
      <v-text-field
        v-model="searchText"
        prepend-inner-icon="mdi-magnify"
        placeholder="Search..."
        variant="solo"
        density="comfortable"
        hide-details
        autofocus
        single-line
        class="search-input-modal"
        aria-label="Search input"
      />

      <div class="search-results">
        <div class="search-empty">
          <v-icon size="48" color="disabled">mdi-magnify</v-icon>
          <p class="text-disabled mt-2">Start typing to search...</p>
        </div>
      </div>
    </div>
  </v-overlay>
</template>

<style scoped>
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.search-button {
  text-transform: none;
}

.search-button :deep(.v-btn__content) {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-button :deep(.v-btn__append) {
  margin-left: auto;
  margin-inline-start: auto;
}

.hotkey-container {
  display: flex;
  gap: 2px;
}

.hotkey-key {
  background-color: rgba(var(--v-theme-on-surface), 0.1);
  border: 1px solid rgba(var(--v-theme-on-surface), 0.2);
  border-radius: 4px;
  padding: 2px 6px;
  font-size: 0.75rem;
  font-family: monospace;
  color: rgba(var(--v-theme-on-surface), 0.7);
  min-width: 20px;
  text-align: center;
  line-height: 1.2;
}

.search-overlay {
  backdrop-filter: blur(0px);
  -webkit-backdrop-filter: blur(0px);
  background-color: rgba(0, 0, 0, 0);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease-out;
}

.search-overlay.v-overlay--active {
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  background-color: rgba(0, 0, 0, 0.3);
}

.search-modal {
  width: 90vw;
  max-width: 600px;
  max-height: 70vh;
  background: rgb(var(--v-theme-surface));
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  margin: auto;
  opacity: 0;
  transform: scale(0.96) translateY(-8px);
  transition: all 0.15s ease-out;
}

.search-overlay.v-overlay--active .search-modal {
  opacity: 1;
  transform: scale(1) translateY(0);
}

.search-input-modal {
  border-bottom: 1px solid rgba(var(--v-theme-outline), 0.2);
}

.search-input-modal :deep(.v-field) {
  box-shadow: none !important;
  border-radius: 0 !important;
}

.search-results {
  flex: 1;
  padding: 24px;
  min-height: 200px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.search-empty {
  text-align: center;
  opacity: 0.6;
}

.search-overlay :deep(.v-overlay__scrim) {
  transition: all 0.15s ease-out;
}
</style>

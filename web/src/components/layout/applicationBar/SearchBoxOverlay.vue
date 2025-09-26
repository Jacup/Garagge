<script lang="ts" setup>
import { ref, watch } from 'vue'

// Props
const { isOpen } = defineProps<{
  isOpen: boolean
}>()

// Emits
const emit = defineEmits<{
  close: []
  search: [query: string]
  'update:isOpen': [value: boolean]
}>()

// Reactive state
const searchText = ref('')

// Functions
function closeOverlay() {
  emit('update:isOpen', false)
  emit('close')
  searchText.value = ''
}

// Search debounce
let searchTimeout: ReturnType<typeof setTimeout>

function searchWithDebounce(query: string) {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    emit('search', query)
  }, 300)
}

// Watchers
watch(searchText, (newValue) => {
  if (newValue.trim()) {
    searchWithDebounce(newValue)
  }
})

// Expose the close function for parent components
defineExpose({
  close: closeOverlay,
})
</script>

<template>
  <v-overlay
    :model-value="isOpen"
    @update:model-value="emit('update:isOpen', $event)"
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

.search-overlay {
  backdrop-filter: blur(0px);
  -webkit-backdrop-filter: blur(0px);
  background-color: rgba(0, 0, 0, 0);
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding-top: 10vh;
  transition: all 0.15s ease-out;
}

@media (max-width: 768px) {
  .search-overlay {
    align-items: flex-start;
    padding-top: 8vh;
  }
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
  margin: 0;
  opacity: 0;
  transform: scale(0.96) translateY(-8px);
  transition: all 0.15s ease-out;
}

@media (max-width: 768px) {
  .search-modal {
    width: 95vw;
    max-height: 60vh;
  }
}

.search-overlay.v-overlay--active .search-modal {
  opacity: 1;
  transform: scale(1) translateY(0);
}

.search-input-modal {
  border-bottom: 1px solid rgb(var(--v-theme-outline-variant));
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

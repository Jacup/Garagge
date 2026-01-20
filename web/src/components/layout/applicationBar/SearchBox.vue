<script lang="ts" setup>
import { ref } from 'vue'
import { useHotkey } from 'vuetify'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'

import SearchBoxActivator from './SearchBoxActivator.vue'
import SearchBoxOverlay from './SearchBoxOverlay.vue'

// Emits
const emit = defineEmits<{
  search: [query: string]
}>()

const { isMobile } = useResponsiveLayout()

// Reactive state
const isOverlayOpen = ref(false)

// Functions
function openSearchOverlay() {
  isOverlayOpen.value = true
}

function closeSearchOverlay() {
  isOverlayOpen.value = false
}

function handleVoiceSearch() {
  console.log('Voice search triggered (placeholder)')
}

function handleSearch(query: string) {
  emit('search', query)
}

// Hotkeys (desktop only)
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
  <SearchBoxActivator :is-mobile="isMobile" @open="openSearchOverlay" @voice-search="handleVoiceSearch" />

  <SearchBoxOverlay v-model:is-open="isOverlayOpen" @close="closeSearchOverlay" @search="handleSearch" />
</template>

<style scoped></style>

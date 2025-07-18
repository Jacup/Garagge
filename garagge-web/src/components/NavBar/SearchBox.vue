<script lang="ts" setup>
import { ref } from 'vue'

const searchText = ref('')
const inputRef = ref<HTMLInputElement | null>(null)

function searchBoxEmpty() {
  return !searchText.value || searchText.value.trim().length === 0
}

function clearInput() {
  searchText.value = ''
  inputRef.value?.focus()
}
</script>

<template>
  <div class="search-box">
    <div class="search-icon">
      <span class="material-icons">search</span>
    </div>
    <div class="search-input">
      <input ref="inputRef" type="text" placeholder="Search..." v-model="searchText" aria-label="Search" />
    </div>
    <div class="search-clear-wrapper">
      <button v-if="!searchBoxEmpty()" class="search-clear" type="button" @click="clearInput" aria-label="Clear search">X</button>
    </div>
  </div>
</template>

<style scoped>
.search-box {
  width: 100%;
  height: 40px;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 0.5rem;
  gap: 0.5rem;
  display: flex;
  flex-direction: row;
}

.search-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: default;
}

.material-icons {
  font-family: 'Material Icons';
  font-size: 24px;
  line-height: 1;
  vertical-align: middle;
  color: var(--color-text-muted);
}

.search-input {
  flex: 1;
  height: 100%;
  display: flex;
  align-items: center;
}

.search-input input {
  width: 100%;
  height: 100%;
  background: transparent;
  border: none;
  outline: none;
  color: inherit;
  font-size: 1rem;
  padding: 0;
}

.search-clear-wrapper {
  display: flex;
  justify-content: center;
  margin-left: auto;
  height: 100%;
  aspect-ratio: 1 / 1;
}

.search-clear {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  height: 100%;
  width: 100%;
  font-size: 1rem;
  cursor: pointer;
}

.search-clear:hover {
  background: var(--color-card-contrast);
  border-radius: 4px;
}
</style>

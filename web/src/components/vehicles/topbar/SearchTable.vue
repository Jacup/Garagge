<script lang="ts" setup>
import { defineProps, defineEmits, computed } from 'vue'
const props = defineProps<{ modelValue: string }>()
const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()
const search = computed({
  get: () => props.modelValue,
  set: (val: string) => emit('update:modelValue', val),
})
</script>

<template>
  <div class="search-container">
    <v-text-field
      v-model="search"
      label="Search vehicles"
      class="search-field"
      prepend-inner-icon="mdi-magnify"
      density="compact"
      variant="outlined"
      hide-details
      single-line
      :clearable="true"
    >
    </v-text-field>
  </div>
</template>

<style scoped>
.search-container {
  display: flex;
  justify-content: flex-start;
  width: 30%;
  min-width: 200px;
  background-color: transparent;
}

.search-field {
  width: 100%;
  max-width: 400px;
}
</style>

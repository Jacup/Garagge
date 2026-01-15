<script lang="ts" setup>
import { computed } from 'vue'
import { useFormatting } from '@/composables/useFormatting'

const props = defineProps<{
  createdDate: string
  updatedDate: string
  id: string
}>()

const { formatDateTime } = useFormatting()

const wasUpdated = computed(() => {
  return props.createdDate !== props.updatedDate
})
</script>

<template>
  <v-card variant="text">
    <v-card-text>
      <div class="text-center text-caption text-medium-emphasis d-flex flex-column gap-2">
        <div class="d-flex justify-center align-center">
          <v-icon icon="mdi-clock-plus-outline" size="small" class="mr-1 opacity-70" />
          <span>Created: {{ formatDateTime(props.createdDate!) }}</span>
        </div>

        <div v-if="wasUpdated" class="d-flex justify-center align-center mt-1 text-medium-emphasis">
          <v-icon icon="mdi-clock-edit-outline" size="small" class="mr-1 opacity-70" />
          <span>Updated: {{ formatDateTime(props.updatedDate!) }}</span>
        </div>

        <div class="font-mono opacity-60 mt-2" style="font-size: 0.7rem">ID: {{ props.id }}</div>
      </div>
    </v-card-text>
  </v-card>
</template>

<style scoped></style>

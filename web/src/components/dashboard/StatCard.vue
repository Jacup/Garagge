<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title: string
  titleHelper?: string
  value: string | number
  valueHelper?: string

  trendValue?: string // Tekst w chipie, np. "+12%", "-5%", "Active"
  trendMode?: 'good' | 'bad' | 'neutral' // Określa kolor. Domyślnie 'neutral'

  icon: string
  accentColor?: string // np. 'primary', 'blue', 'orange'
}

const props = withDefaults(defineProps<Props>(), {
  titleHelper: '',
  valueHelper: '',
  trendMode: 'neutral', // Domyślnie neutralny (np. szary/niebieski)
  accentColor: 'primary',
})

const trendIcon = computed(() => {
  if (!props.trendValue) return null
  if (props.trendValue.includes('+')) return 'mdi-arrow-up'
  if (props.trendValue.includes('-')) return 'mdi-arrow-down'
  return null // Brak strzałki dla zwykłego tekstu (np. "Active")
})

// LOGIKA KOLORU CHIPA:
const chipColor = computed(() => {
  switch (props.trendMode) {
    case 'good':
      return 'success'
    case 'bad':
      return 'error'
    default:
      return 'secondary' // Lub 'surface-variant' dla neutralnego
  }
})
const textColorClass = computed(() => `text-on-${props.accentColor}-container`)
const containerColor = computed(() => `${props.accentColor}-container`)
</script>

<template>
  <v-card
    class="overflow-hidden position-relative h-100 d-flex flex-column"
    :color="accentColor"
    rounded="xl"
    variant="tonal"
    min-height="160"
  >
    <div class="position-absolute" style="right: -20px; bottom: -20px; opacity: 0.08; pointer-events: none">
      <v-icon :icon="icon" size="180" :color="accentColor"></v-icon>
    </div>

    <div class="pa-5 z-index-1 d-flex flex-column">
      <div class="mb-1">
        <div class="text-body-2 font-weight-bold text-uppercase letter-spacing-1">
          {{ title }}
        </div>
        <div class="text-caption font-weight-medium opacity-60">
          {{ titleHelper }}
        </div>
      </div>

      <v-spacer></v-spacer>

      <div class="mt-2">
        <div class="text-h3 font-weight-black text-high-emphasis">
          {{ value }}
        </div>

        <div class="d-flex align-center mt-3">
          <v-chip v-if="trendValue" size="x-small" :color="chipColor" variant="flat" class="font-weight-bold">
            <v-icon v-if="trendIcon" start size="small" :icon="trendIcon"></v-icon>
            {{ trendValue }}
          </v-chip>
          <span v-if="valueHelper" class="text-caption font-weight-bold opacity-60 text-truncate" :class="textColorClass">
            {{valueHelper }}
          </span>
        </div>
      </div>
    </div>
  </v-card>
</template>

<style scoped>
.letter-spacing-1 {
  letter-spacing: 1.5px !important;
}
.leading-tight {
  line-height: 1 !important;
}
.z-index-1 {
  z-index: 1;
  position: relative;
}
</style>

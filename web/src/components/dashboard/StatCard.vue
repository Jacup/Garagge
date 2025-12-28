<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title: string
  subtitle?: string

  value: string | number

  chipValue?: string
  chipColor?: string
  chipAppendText?: string
  chipPrefixArrow?: 'up' | 'down' | 'neutral'

  icon: string
  accentColor: string
}

const props = withDefaults(defineProps<Props>(), {
  subtitle: '',
  chipValue: '',
  chipAppendText: '',
  chipPrefixArrow: 'neutral',
})

const chipIcon = computed(() => {
  const iconMap: Record<string, string | null> = {
    up: 'mdi-arrow-up',
    down: 'mdi-arrow-down',
    neutral: null,
  }
  return iconMap[props.chipPrefixArrow] ?? null
})

const textColorClass = computed(() => `text-on-${props.accentColor}-container`)
</script>

<template>
  <v-card
    class="overflow-hidden position-relative h-100 d-flex flex-column"
    :color="accentColor"
    rounded="xl"
    variant="tonal"
    role="article"
    :aria-label="`${title} ${subtitle ? '- ' + subtitle : ''}`"
  >
    <div class="card-overlay">
      <v-icon :icon="icon" size="180" :color="accentColor"></v-icon>
    </div>

    <div class="card-container pa-5">
      <div class="card-header">
        <div class="card-header-title text-body-2 text-uppercase font-weight-bold">
          {{ title }}
        </div>
        <div class="card-header-subtitle text-caption font-weight-medium opacity-60">
          {{ subtitle }}
        </div>
      </div>

      <div>
        <div class="stat-card-value text-h4 font-weight-black text-high-emphasis">
          {{ value }}
        </div>

        <div class="stat-card-chips d-flex align-center mt-1">
          <v-chip v-if="chipValue" size="x-small" :color="chipColor" variant="flat" class="suggestion-chip">
            <v-icon v-if="chipIcon" start size="small" :icon="chipIcon"></v-icon>
            {{ chipValue }}
          </v-chip>
          <span v-if="chipAppendText" class="text-caption font-weight-bold opacity-60 text-truncate ml-1" :class="textColorClass">
            {{ chipAppendText }}
          </span>
        </div>
      </div>
    </div>
  </v-card>
</template>

<style scoped>
.card-overlay {
  position: absolute;
  right: -20px;
  bottom: -20px;
  opacity: 0.08;
  pointer-events: none;
}

.card-container {
  z-index: 1;
  position: relative;
  display: flex;
  flex-direction: column;
}

.card-header {
  margin-bottom: 12px;
}

.card-header-title {
  letter-spacing: 1.5px;
  min-height: 20px;
  align-items: center;
}

.card-header-subtitle {
  min-height: 20px;
  align-items: center;
}

.stat-card-value {
  min-height: 40px;
}

.stat-card-chips {
  min-height: 20px;
}

.suggestion-chip {
  border-radius: 8px !important;
  font-weight: 500;
}
</style>

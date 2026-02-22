<script setup lang="ts">
import { computed } from 'vue'
import type { StatMetricDto } from '@/api/generated/apiV1.schemas'
import { NullableOfContextTrend, NullableOfTrendMode } from '@/api/generated/apiV1.schemas'

interface Props {
  title: string
  metric: StatMetricDto | null
  icon: string
  accentColor: string
}

const props = defineProps<Props>()

const chipValue = computed(() => props.metric?.contextValue || '')
const chipAppendText = computed(() => props.metric?.contextAppendText || '')

const chipPrefixArrow = computed((): 'up' | 'down' | 'neutral' => {
  switch (props.metric?.contextTrend) {
    case NullableOfContextTrend.Up:
      return 'up'
    case NullableOfContextTrend.Down:
      return 'down'
    case NullableOfContextTrend.None:
    default:
      return 'neutral'
  }
})

const chipColor = computed(() => {
  switch (props.metric?.contextTrendMode) {
    case NullableOfTrendMode.Good:
      return 'success'
    case NullableOfTrendMode.Bad:
      return 'error'
    case NullableOfTrendMode.Neutral:
    default:
      return 'surface'
  }
})

const chipIcon = computed(() => {
  const iconMap: Record<string, string | null> = {
    up: 'mdi-arrow-up',
    down: 'mdi-arrow-down',
    neutral: null,
  }
  return iconMap[chipPrefixArrow.value] ?? null
})

const textColorClass = computed(() => `text-on-${props.accentColor}-container`)
</script>

<template>
  <v-card
    class="overflow-hidden position-relative h-100 d-flex flex-column"
    :color="accentColor"
    rounded="md-16px"
    variant="tonal"
    role="article"
    :aria-label="`${metric?.value ?? 'N/A'} ${metric?.subtitle ? '- ' + metric.subtitle : ''}`"
  >
    <div class="card-overlay">
      <v-icon :icon="icon" size="180" :color="accentColor"></v-icon>
    </div>

    <template #title>
      {{ title }}
    </template>

    <template #subtitle>
      {{ metric?.subtitle || '\u00A0' }}
    </template>

    <template #text>
      <div class="stat-card-value text-h5 font-weight-black text-high-emphasis">
        {{ metric?.value ?? 'N/A' }}
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
    </template>
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

<script lang="ts" setup>
export type ChartType = 'price' | 'consumption' | 'refuel-cost'

const model = defineModel<ChartType>({ default: 'price' })

const options: { value: ChartType; label: string }[] = [
  { value: 'price', label: 'Fuel price' },
  { value: 'consumption', label: 'Consumption' },
  { value: 'refuel-cost', label: 'Refuel cost' },
]
</script>

<template>
  <v-chip-group v-model="model" filter mandatory variant="outlined" selected-class="filter-chip-selected">
    <v-chip v-for="option in options" :key="option.value" :value="option.value" class="filter-chip mr-0 ml-2">
      {{ option.label }}
    </v-chip>
  </v-chip-group>
</template>

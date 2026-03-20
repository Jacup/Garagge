<script lang="ts" setup>
import { computed } from 'vue'
import type { EnergyType } from '@/api/generated/apiV1.schemas'

interface Props {
  allowedEnergyTypes: EnergyType[] | undefined
  modelValue: EnergyType[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: EnergyType[]]
}>()

const model = computed({
  get: () => props.modelValue,
  set: (val: EnergyType[] | null) => emit('update:modelValue', val ?? []),
})
</script>

<template>
  <v-chip-group v-model="model" filter multiple variant="outlined" selected-class="filter-chip-selected" class="py-0">
    <v-chip v-for="type in allowedEnergyTypes" :key="type" :value="type" class="filter-chip mr-0 ml-2">
      {{ type }}
    </v-chip>
  </v-chip-group>
</template>

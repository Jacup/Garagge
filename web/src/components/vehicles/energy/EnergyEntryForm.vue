<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { formattingUtils } from '@/utils/formattingUtils'
import { EnergyType, EnergyUnit } from '@/api/generated/apiV1.schemas'
import type { EnergyEntryCreateRequest } from '@/api/generated/apiV1.schemas'

export type EnergyFormData = Partial<EnergyEntryCreateRequest>

interface Props {
  modelValue: EnergyFormData
  allowedEnergyTypes?: EnergyType[]
  apiErrors?: Array<{ description: string }>
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: EnergyFormData]
}>()

const { isMobile } = useResponsiveLayout()
const { formatEnergyUnit } = formattingUtils()

const formRef = ref<{ validate: () => Promise<{ valid: boolean }> } | null>(null)

const rules = {
  required: (v: unknown) => !!v || 'Field is required',
  positive: (v: number) => v >= 0 || 'Must be positive',
  greaterThanZero: (v: number) => v > 0 || 'Must be greater than zero',
}

defineExpose({
  validate: async () => {
    if (!formRef.value) {
      console.warn('Form ref not available')
      return false
    }
    const result = await formRef.value.validate()
    return result.valid
  },
})

const localModel = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
})

const energyOptions = computed(() => {
  const units =
    localModel.value.type === EnergyType.Electric ? [EnergyUnit.kWh] : Object.values(EnergyUnit).filter((u) => u !== EnergyUnit.kWh)

  return units.map((unit) => ({
    title: formatEnergyUnit(unit),
    value: unit,
  }))
})

const calculatedPricePerUnit = computed(() => {
  const cost = Number(localModel.value.cost)
  const volume = Number(localModel.value.volume)
  if (cost > 0 && volume > 0) return (cost / volume).toFixed(2)
  return '0.00'
})

watch(
  () => localModel.value.type,
  (newType) => {
    if (newType === EnergyType.Electric) {
      localModel.value.energyUnit = EnergyUnit.kWh
    } else if (localModel.value.energyUnit === EnergyUnit.kWh) {
      localModel.value.energyUnit = EnergyUnit.Liter
    }
  },
)
</script>

<template>
  <v-form ref="formRef" @submit.prevent>
    <v-container class="pa-0 mt-2">
      <div v-if="apiErrors && apiErrors.length > 0" class="mb-4">
        <v-alert
          v-for="(error, idx) in apiErrors"
          :key="idx"
          type="error"
          variant="tonal"
          density="compact"
          class="mb-2"
          icon="mdi-alert-circle"
        >
          {{ error.description }}
        </v-alert>
      </div>

      <v-row :dense="isMobile" class="mb-3">
        <v-col cols="12" md="6">
          <v-text-field
            v-model="localModel.date"
            label="Date"
            type="date"
            variant="outlined"
            density="comfortable"
            :rules="[rules.required]"
          />
        </v-col>
        <v-col cols="12" md="6">
          <v-number-input
            v-model="localModel.mileage"
            label="Mileage"
            suffix="km"
            variant="outlined"
            control-variant="hidden"
            density="comfortable"
            :rules="[rules.required, rules.positive]"
          />
        </v-col>
      </v-row>

      <v-row dense>
        <v-col cols="12" md="6">
          <v-chip-group v-model="localModel.type" filter mandatory variant="outlined" class="my-2" selected-class="filter-chip-selected">
            <v-chip v-for="type in allowedEnergyTypes" :key="type" :value="type" class="filter-chip">
              {{ type }}
            </v-chip>
          </v-chip-group>
        </v-col>
      </v-row>

      <v-row :dense="isMobile">
        <v-col cols="8" sm="8">
          <v-number-input
            v-model="localModel.volume"
            label="Volume"
            variant="outlined"
            density="comfortable"
            inset
            :precision="2"
            :step="0.01"
            :rules="[rules.required, rules.greaterThanZero]"
          />
        </v-col>

        <v-col cols="4" sm="4">
          <v-select
            v-model="localModel.energyUnit"
            :items="energyOptions"
            :readonly="localModel.type === EnergyType.Electric"
            label="Unit"
            variant="outlined"
            density="comfortable"
            hide-details
          />
        </v-col>
      </v-row>

      <v-row :dense="isMobile">
        <v-col cols="12" md="6">
          <v-text-field
            v-model="localModel.cost"
            label="Total Cost"
            type="number"
            step="0.01"
            min="0"
            variant="outlined"
            density="comfortable"
            prepend-inner-icon="mdi-cash"
            :rules="[rules.required, rules.positive]"
          />
        </v-col>

        <v-col cols="12">
          <v-text-field
            :model-value="calculatedPricePerUnit"
            label="Price per unit"
            readonly
            variant="plain"
            density="comfortable"
            :suffix="`PLN / ${localModel.energyUnit == null ? '-' : formatEnergyUnit(localModel.energyUnit)}`"
            prepend-inner-icon="mdi-tag-outline"
            class="mt-1"
          />
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<style scoped>
.filter-chip {
  background-color: transparent !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-color: rgb(var(--v-theme-outline)) !important;
  border-radius: 8px !important;
}

.filter-chip-selected {
  background-color: rgb(var(--v-theme-secondary-container)) !important;
  border-width: 0 !important;
  color: rgb(var(--v-theme-on-secondary-container)) !important;
  border-radius: 8px !important;
}

.filter-chip :deep(.v-icon) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.filter-chip-selected :deep(.v-icon) {
  color: rgb(var(--v-theme-on-secondary-container)) !important;
}
</style>

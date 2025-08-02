<script lang="ts" setup>
import type { Vehicle } from '@/types/vehicle'

defineProps<{
  vehicle: Vehicle
}>()

defineEmits<{
  save: []
  'update:vehicle': [vehicle: Vehicle]
}>()

const rules = {
  required: (value: string | number | null) => !!value || 'This field is required.',
  counter: (value: string) => value.length <= 64 || 'Max 64 characters',
  yearMin: (value: number | null) => (value && value >= 1886) || 'Year must be >= 1886',
  yearMax: (value: number | null) => (value && value <= new Date().getFullYear()) || `Year cannot be in the future`,
}
</script>

<template>
  <v-card class="information-card">
    <v-card-title class="information-title">Vehicle Information</v-card-title>

    <v-form class="information-form" id="addVehicleForm">
      <v-text-field
        :model-value="vehicle.brand"
        @update:model-value="$emit('update:vehicle', { ...vehicle, brand: $event })"
        :rules="[rules.required, rules.counter]"
        maxlength="64"
        counter
        variant="outlined"
        label="Brand"
        placeholder="Toyota"
        required
      ></v-text-field>

      <v-text-field
        :model-value="vehicle.model"
        @update:model-value="$emit('update:vehicle', { ...vehicle, model: $event })"
        :rules="[rules.required, rules.counter]"
        maxlength="64"
        counter
        variant="outlined"
        label="Model"
        placeholder="Corolla"
        required
      ></v-text-field>

      <v-text-field
        :model-value="vehicle.manufacturedYear"
        @update:model-value="$emit('update:vehicle', { ...vehicle, manufacturedYear: Number($event) })"
        :rules="[rules.required, rules.yearMin, rules.yearMax]"
        type="number"
        variant="outlined"
        label="Manufactured Year"
        placeholder="2020"
        :min="1886"
        :max="2100"
        required
      ></v-text-field>
    </v-form>
  </v-card>
</template>

<style scoped>
.information-card {
  flex: 2;
  display: flex;
  flex-direction: column;
  justify-content: center;
  max-width: 550px;
}

.information-title {
  font-weight: bold;
  padding: 20px;
}

.information-form {
  display: flex;
  flex-direction: column;
  width: 100%;
  padding: 0 20px 20px 20px;
  gap: 12px;
}
</style>

<script lang="ts" setup>
import type { CreateVehicleCommand, EngineType, NullableOfVehicleType } from '@/api/generated/apiV1.schemas'
import { EngineType as EngineTypeEnum, NullableOfVehicleType as VehicleTypeEnum } from '@/api/generated/apiV1.schemas'

defineProps<{
  vehicle: CreateVehicleCommand
}>()

defineEmits<{
  save: []
  'update:vehicle': [vehicle: CreateVehicleCommand]
}>()

const rules = {
  required: (value: string | number | null) => !!value || 'This field is required.',
  counter: (value: string) => value.length <= 64 || 'Max 64 characters',
  yearMin: (value: number | null) => (value && value >= 1886) || 'Year must be >= 1886',
  yearMax: (value: number | null) => (value && value <= new Date().getFullYear()) || `Year cannot be in the future`,
  vinLength: (value: string | null) => value === null || value === '' || value.length === 17 || 'VIN must be exactly 17 characters',
}

const engineTypeOptions: { label: string; value: EngineType }[] = [
  { label: 'Fuel', value: EngineTypeEnum.Fuel },
  { label: 'Hybrid', value: EngineTypeEnum.Hybrid },
  { label: 'Plug-in Hybrid', value: EngineTypeEnum.PlugInHybrid },
  { label: 'Electric', value: EngineTypeEnum.Electric },
  { label: 'Hydrogen', value: EngineTypeEnum.Hydrogen },
]

const typeOptions: { label: string; value: NullableOfVehicleType }[] = [
  { label: 'Bus', value: VehicleTypeEnum.Bus },
  { label: 'Car', value: VehicleTypeEnum.Car },
  { label: 'Motorbike', value: VehicleTypeEnum.Motorbike },
  { label: 'Truck', value: VehicleTypeEnum.Truck },
]
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

      <v-select
        :model-value="vehicle.engineType"
        @update:model-value="$emit('update:vehicle', { ...vehicle, engineType: $event })"
        :items="engineTypeOptions"
        :rules="[rules.required]"
        item-title="label"
        item-value="value"
        clearable
        label="Power Type"
        variant="outlined"
        required
      ></v-select>

      <v-select
        :model-value="vehicle.type"
        @update:model-value="$emit('update:vehicle', { ...vehicle, type: $event })"
        :items="typeOptions"
        item-title="label"
        item-value="value"
        clearable
        label="Vehicle Type"
        variant="outlined"
      ></v-select>

      <v-text-field
        :model-value="vehicle.vin ?? ''"
        @update:model-value="$emit('update:vehicle', { ...vehicle, vin: $event || null })"
        :rules="[rules.vinLength]"
        variant="outlined"
        label="VIN"
        placeholder="WVWZZZ6RZHU000000"
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

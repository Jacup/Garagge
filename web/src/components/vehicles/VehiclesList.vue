<script lang="ts" setup>
import type { VehicleDto } from '@/api/generated/apiV1.schemas'

interface Props {
  items: VehicleDto[]
  showDetails: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  select: [id: string | undefined]
}>()

const handleRowClick = (item: VehicleDto) => {
  emit('select', item.id)
}
</script>

<template>
  <v-list lines="two" class="list-container">
    <v-list-item v-for="record in items" :key="record.id" class="list-item" @click="handleRowClick(record)">
      <template #prepend>
        <v-avatar color="primary-container">
          <v-icon icon="mdi-car" color="on-primary-container" />
        </v-avatar>
      </template>
      <template #title> {{ record.brand }} {{ record.model }} </template>
      <template #subtitle v-if="showDetails"> {{ record.manufacturedYear }} </template>
    </v-list-item>
  </v-list>
</template>

<style scoped>
.list-container {
  padding-top: 0px;
  padding-bottom: 0px;
  }
.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
  border-radius: 2px !important;
}

.list-item:first-child {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}

.list-item:last-child {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
  margin-bottom: 0 !important;
}

.trailing-supporting-text {
  color: rgb(var(--v-theme-on-surface-variant));
  font-size: 11px;
  font-weight: 500;
  line-height: 16px;
  letter-spacing: 0.5px;
}
</style>

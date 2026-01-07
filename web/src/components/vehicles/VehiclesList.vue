<script lang="ts" setup>
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { getVehicleIcon } from '@/utils/vehicleUtils'
import SwipeableItem from '@/components/common/SwipeableItem.vue'

interface Props {
  items: VehicleDto[]
  showDetails: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  (e: 'select', id: string | undefined): void
  (e: 'delete', id: string | undefined): void
}>()

const handleRowClick = (item: VehicleDto) => {
  emit('select', item.id)
}

const handleDelete = (item: VehicleDto) => {
  emit('delete', item.id)
}
</script>

<template>
  <v-list lines="two" class="list-container" bg-color="transparent">
    <transition-group name="list" tag="div">
      <div
        v-for="(record, index) in items"
        :key="record.id"
        class="list-item-wrapper"
        :class="{
          'is-first': index === 0,
          'is-last': index === items.length - 1
        }"
      >
        <SwipeableItem @delete="handleDelete(record)" @click="handleRowClick(record)">
          <v-list-item class="list-item" link>
            <template #prepend>
              <v-avatar color="primary" variant="tonal" class="mr-3">
                <v-icon :icon="getVehicleIcon(record.type)" color="primary" />
              </v-avatar>
            </template>
            <template #title> {{ record.brand }} {{ record.model }} </template>
            <template #subtitle v-if="showDetails"> {{ record.manufacturedYear }} </template>
          </v-list-item>
        </SwipeableItem>
      </div>
    </transition-group>
  </v-list>
</template>

<style scoped lang="scss">
.list-container {
  padding-top: 0px;
  padding-bottom: 0px;
  position: relative;
}

.list-item-wrapper {
  margin-bottom: 2px;
  transition: all 0.4s cubic-bezier(0.55, 0, 0.1, 1);
}

.list-item-wrapper:last-child {
  margin-bottom: 0;
}

.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 4px !important;
}

.is-first .list-item {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}
.is-first :deep(.swipe-content) {
  border-top-left-radius: 12px;
  border-top-right-radius: 12px;
}

.is-last .list-item {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
}
.is-last :deep(.swipe-content) {
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
}

.list-item-wrapper :deep(.swipe-content) {
  border-radius: 4px;
}

.list-leave-active {
  position: absolute;
  width: 100%;
  z-index: 0;
  transition: all 0.2s ease;
}

.list-leave-to {
  opacity: 0;
  transform: translateX(-100%);
}

.list-move {
  transition: transform 0.4s cubic-bezier(0.55, 0, 0.1, 1);
}
</style>

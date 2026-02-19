<script lang="ts" setup>
import { computed } from 'vue'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { vehicleUtils } from '@/utils/vehicleUtils'
import InteractiveItem from '@/components/common/InteractiveItem.vue'

interface Props {
  items: VehicleDto[]
  showDetails: boolean
  modelValue?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => [],
})

const emit = defineEmits<{
  (e: 'select', id: string | undefined): void
  (e: 'delete', id: string | undefined): void
  (e: 'update:modelValue', ids: string[]): void
}>()

const { getVehicleIcon } = vehicleUtils()

const selectedIds = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
})

const isSelected = (id: string | undefined) => {
  return id ? selectedIds.value.includes(id) : false
}

const toggleSelection = (id: string | undefined) => {
  if (!id) return

  const currentIds = [...selectedIds.value]
  const index = currentIds.indexOf(id)

  if (index === -1) {
    currentIds.push(id)
  } else {
    currentIds.splice(index, 1)
  }

  selectedIds.value = currentIds
}

const handleSelectedUpdate = (item: VehicleDto, value: boolean) => {
  toggleSelection(item.id)
}

const handleRowClick = (item: VehicleDto) => {
  emit('select', item.id)
}

const handleLongPress = (item: VehicleDto) => {
  toggleSelection(item.id)
}

const handleDelete = (item: VehicleDto) => {
  emit('delete', item.id)
}
</script>

<template>
  <v-list :lines="showDetails ? 'two' : 'one'" class="material-list" rounded>
    <transition-group name="list" tag="div">
      <div
        v-for="(record, index) in items"
        :key="record.id"
        class="list-item-wrapper"
        :class="{
          'material-list__item--selected': isSelected(record.id),
        }"
      >
        <InteractiveItem
          :selected="isSelected(record.id)"
          @update:selected="handleSelectedUpdate(record, $event)"
          @delete="handleDelete(record)"
          @click="handleRowClick(record)"
          @long-press="handleLongPress(record)"
        >
          <template #default="{ selected, onIndicatorClick }">
            <v-list-item link :active="selected">
              <template #prepend>
                <div class="avatar-flip-container mr-3" @click.stop="onIndicatorClick">
                  <div class="avatar-flipper" :class="{ flipped: selected }">
                    <div class="avatar-front">
                      <v-avatar color="primary" variant="tonal">
                        <v-icon :icon="getVehicleIcon(record.type)" color="primary" />
                      </v-avatar>
                    </div>
                    <div class="avatar-back">
                      <v-avatar color="secondary" variant="flat">
                        <v-icon icon="mdi-check" color="on-secondary" />
                      </v-avatar>
                    </div>
                  </div>
                </div>
              </template>

              <template #title>{{ record.brand }} {{ record.model }}</template>
              <template v-if="showDetails" #subtitle>{{ record.manufacturedYear }}</template>
            </v-list-item>
          </template>
        </InteractiveItem>
      </div>
    </transition-group>
  </v-list>
</template>

<style scoped lang="scss">
.avatar-flip-container {
  width: 40px;
  height: 40px;
  perspective: 1000px;
  cursor: pointer;
}

.avatar-flipper {
  position: relative;
  width: 100%;
  height: 100%;
  text-align: center;
  transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  transform-style: preserve-3d;
}

.avatar-flipper.flipped {
  transform: rotateY(180deg);
}

.avatar-front,
.avatar-back {
  position: absolute;
  width: 100%;
  height: 100%;
  -webkit-backface-visibility: hidden;
  backface-visibility: hidden;
  top: 0;
  left: 0;
  border-radius: 50%;
}

.avatar-front {
  transform: rotateY(0deg);
  z-index: 2;
}

.avatar-back {
  transform: rotateY(180deg);
  z-index: 1;
}

.list-leave-active {
  position: absolute;
  width: 100%;
  z-index: 0;
  transition: all 0.2s ease;
}

.list-leave-to {
  opacity: 0;
}

.list-move {
  transition: transform 0.4s cubic-bezier(0.55, 0, 0.1, 1);
}
</style>

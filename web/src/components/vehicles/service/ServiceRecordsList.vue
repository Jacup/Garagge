<script lang="ts" setup>
import { computed } from 'vue'
import { useFormatting } from '@/composables/useFormatting'
import { serviceUtils } from '@/utils/serviceUtils'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import InteractiveItem from '@/components/common/InteractiveItem.vue'

interface Props {
  items: ServiceRecordDto[]
  modelValue?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => [],
})

const emit = defineEmits<{
  (e: 'select', id: string): void
  (e: 'delete', id: string): void
  (e: 'update:modelValue', ids: string[]): void
}>()

const { formatDate, formatCurrency } = useFormatting()
const { getIconForServiceType } = serviceUtils()

const selectedIds = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
})

const isSelected = (id: string | undefined): boolean => {
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

const handleSelectedUpdate = (item: ServiceRecordDto) => {
  toggleSelection(item.id)
}

const handleRowClick = (item: ServiceRecordDto) => {
  emit('select', item.id)
}

const handleLongPress = (item: ServiceRecordDto) => {
  toggleSelection(item.id)
}

const handleDelete = (item: ServiceRecordDto) => {
  emit('delete', item.id)
}
</script>

<template>
  <v-list lines="two" class="material-list" rounded>
    <transition-group name="list" tag="div">
      <div
        v-for="record in items"
        :key="record.id"
        class="list-item-wrapper"
        :class="{
          'material-list__item--selected': isSelected(record.id),
        }"
      >
        <InteractiveItem
          :selected="isSelected(record.id)"
          @update:selected="handleSelectedUpdate(record)"
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
                      <v-badge color="info" :model-value="!!record.serviceItems?.length" :content="record.serviceItems?.length">
                        <v-avatar color="primary" variant="tonal">
                          <v-icon :icon="getIconForServiceType(record.type)" color="primary" />
                        </v-avatar>
                      </v-badge>
                    </div>
                    <div class="avatar-back">
                      <v-avatar color="secondary" variant="flat">
                        <v-icon icon="mdi-check" color="on-secondary" />
                      </v-avatar>
                    </div>
                  </div>
                </div>
              </template>

              <template #title>{{ record.type }}</template>

              <template #subtitle>
                {{ formatDate(record.serviceDate) }}
                <span v-if="record.mileage"> • {{ record.mileage }} km</span>
              </template>

              <template #append>
                <div v-if="record.totalCost" class="trailing-supporting-text">
                  {{ formatCurrency(record.totalCost) }}
                </div>
              </template>
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

.trailing-supporting-text {
  color: rgb(var(--v-theme-on-surface-variant));
  font-size: 11px;
  font-weight: 500;
  line-height: 16px;
  letter-spacing: 0.5px;
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

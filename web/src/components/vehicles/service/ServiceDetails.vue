<script setup lang="ts">
import { computed } from 'vue'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import { useFormatting } from '@/composables/useFormatting'
import { useServiceItemState } from '@/composables/vehicles/useServiceItemState'

const props = defineProps<{
  record: ServiceRecordDto | null
}>()

const { formatCurrency, formatDate, formatMileage, formatDateTime } = useFormatting()

const itemState = useServiceItemState()
const itemCount = computed(() => props.record?.serviceItems.length ?? 0)

const wasUpdated = computed(() => {
  if (!props.record) return false
  return props.record.createdDate !== props.record.updatedDate
})
</script>

<template>
  <div class="d-flex flex-column h-100 overflow-y-auto bg-surface font-responsive scroll-container px-4">
    <template v-if="!record">
      <div class="pa-6">
        <v-skeleton-loader type="avatar, text, heading, text, divider, list-item-three-line@3" />
      </div>
    </template>

    <template v-else>
      <div class="hero-section bg-primary-container text-on-primary-container pt-8 pb-10 text-center rounded-xl mt-4 mb-6 elevation-2">
        <h1 class="text-h3 font-weight-black mb-2">
          {{ formatCurrency(record.totalCost) }}
        </h1>

        <h2 class="text-h6 opacity-90 font-weight-medium mb-6">
          {{ record.title }}
        </h2>

        <div class="d-flex justify-center gap-3 flex-wrap">
          <v-chip prepend-icon="mdi-calendar-blank" variant="outlined" class="suggestion-chip">
            <v-tooltip activator="parent" location="bottom" open-delay="200" close-delay="1000">Service date</v-tooltip>
            {{ formatDate(record.serviceDate) }}
          </v-chip>
          <v-chip v-if="record.mileage" prepend-icon="mdi-speedometer" variant="outlined" class="suggestion-chip">
            <v-tooltip activator="parent" location="bottom" open-delay="200" close-delay="1000">Mileage</v-tooltip>
            {{ formatMileage(record.mileage) }}
          </v-chip>
        </div>
      </div>

      <div class="pb-6 mx-auto" style="max-width: 900px; width: 100%">
        <v-alert v-if="record.notes" icon="mdi-note-text-outline" color="tertiary-container" rounded="md-medium" class="mb-6" title="Notes">
          <div class="text-body-2 text-medium-emphasis" style="white-space: pre-line">
            {{ record.notes }}
          </div>
        </v-alert>

        <div class="mb-2 d-flex align-center">
          <h3 class="ml-2 text-h6 font-weight-bold">Items Breakdown</h3>
          <v-spacer />
          <v-chip class="suggestion-chip mr-2" variant="flat" size="small" color="surface-variant">{{ itemCount }} items</v-chip>
          <v-btn v-if="record?.id" size="small" prepend-icon="mdi-plus" @click="itemState.create(record.id)"> Add Item </v-btn>
        </div>

        <v-list density="comfortable" class="pa-0 mb-4">
          <template v-for="item in record.serviceItems" :key="item.id">
            <v-list-item class="list-item" lines="two">
              <template #prepend>
                <v-avatar color="secondary-container" class="font-weight-bold text-body-2 mr-2"> {{ item.quantity }}x </v-avatar>
              </template>

              <v-list-item-title class="font-weight-medium text-body-1 mb-1">
                {{ item.name }}
              </v-list-item-title>

              <v-list-item-subtitle v-if="item.partNumber" class="d-flex flex-wrap align-center text-caption">
                <v-chip class="suggestion-chip" size="small" density="compact" variant="outlined"> PN: {{ item.partNumber }} </v-chip>
              </v-list-item-subtitle>

              <template #append>
                <div class="d-flex align-center">
                  <div class="text-body-1 font-weight-black text-high-emphasis mx-2">
                    {{ formatCurrency(item.totalPrice) }}
                  </div>

                  <v-menu v-if="record?.id" location="bottom end">
                    <template v-slot:activator="{ props: menuProps }">
                      <v-btn icon="mdi-dots-vertical" variant="text" density="comfortable" color="medium-emphasis" v-bind="menuProps" />
                    </template>

                    <v-list density="compact" width="150" class="menu-container pa-1" rounded="md-16px">
                      <v-list-item class="menu-item" rounded="md-medium" prepend-icon="mdi-pencil-outline" title="Edit" @click="itemState.edit(item, record.id)"></v-list-item>

                      <v-list-item
                        class="menu-item text-error"
                        rounded="md-medium"
                        prepend-icon="mdi-delete-outline"
                        title="Delete"
                        @click="itemState.openDeleteDialog(item, record.id)"
                      ></v-list-item>
                    </v-list>
                  </v-menu>
                </div>
              </template>
            </v-list-item>
          </template>
        </v-list>

        <div class="text-center text-caption text-medium-emphasis d-flex flex-column gap-2 pb-4">
          <div class="audit-info">
            <div class="d-flex justify-center align-center">
              <v-icon icon="mdi-clock-plus-outline" size="small" class="mr-1 opacity-70" />
              <span>Created: {{ formatDateTime(record.createdDate) }}</span>
            </div>

            <div v-if="wasUpdated" class="d-flex justify-center align-center mt-1 text-medium-emphasis">
              <v-icon icon="mdi-clock-edit-outline" size="small" class="mr-1 opacity-70" />
              <span>Updated: {{ formatDateTime(record.updatedDate) }}</span>
            </div>
          </div>

          <div class="font-mono text-disabled opacity-70 mt-2" style="font-size: 0.7rem">ID: {{ record.id }}</div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.menu-container {
  background-color: rgba(var(--v-theme-surface-container-low)) !important;
}

.menu-item {
  color: rgba(var(--v-theme-on-surface));
}

.font-mono {
  font-family: 'Roboto Mono', monospace;
  letter-spacing: -0.5px;
}

.gap-3 {
  gap: 12px;
}

.hero-section {
  box-shadow: 0 4px 20px -8px rgba(var(--v-shadow-key-umbra-color), 0.3);
}
.suggestion-chip {
  border-color: rgb(var(--v-theme-outline)) !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-radius: 8px !important;
  font-weight: 500;
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

.scroll-container {
  scrollbar-width: none;
  overflow-y: auto;
}

.scroll-container::-webkit-scrollbar {
  display: none;
}
</style>

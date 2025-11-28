<script setup lang="ts">
import { computed } from 'vue'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import ServiceDetails from './ServiceDetails.vue'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useServiceDetailsState } from '@/composables/vehicle/useServiceDetailsState'

const props = defineProps<{
  modelValue: boolean
  record: ServiceRecordDto | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const { close: closeServiceDetailsSheet } = useServiceDetailsState()
const { isMobile } = useResponsiveLayout()

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

</script>

<template>
  <v-dialog v-if="isMobile" v-model="isOpen" fullscreen transition="dialog-bottom-transition" scrollable>
    <v-card rounded="0" class="d-flex flex-column h-100">
      <v-toolbar class="mobile-toolbar">
        <v-btn icon="mdi-close" @click="closeServiceDetailsSheet()" />
        <v-toolbar-title class="text-body-1 font-weight-medium">Service Details</v-toolbar-title>
      </v-toolbar>

      <ServiceDetails :record="record" />
    </v-card>
  </v-dialog>

  <v-navigation-drawer v-else v-model="isOpen" location="right" width="500" floating>
    <div class="d-flex flex-column h-100 pt-4 px-4">
      <v-toolbar density="compact" rounded="pill" class="web-toolbar">
        <span class="text-subtitle-1 font-weight-medium ml-4">Service Details</span>
        <v-spacer />
        <v-btn icon="mdi-close" variant="text" @click="closeServiceDetailsSheet()" />
      </v-toolbar>

      <div class="flex-grow-1 overflow-y-auto">
        <ServiceDetails :record="record" />
      </div>
    </div>
  </v-navigation-drawer>
</template>

<style scoped>
.mobile-toolbar {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.web-toolbar {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>

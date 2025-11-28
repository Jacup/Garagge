<script setup lang="ts">
import { computed } from 'vue'
import { useDisplay } from 'vuetify'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'
import ServiceDetails from './ServiceDetails.vue';

const props = defineProps<{
  modelValue: boolean
  record: ServiceRecordDto | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const { mobile } = useDisplay()

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

const close = () => {
  isOpen.value = false
}
</script>

<template>
  <v-dialog v-if="mobile" v-model="isOpen" fullscreen transition="dialog-bottom-transition" scrollable>
    <v-card rounded="0" class="d-flex flex-column h-100">
      <v-toolbar class="toolbar">
        <v-btn icon="mdi-close" @click="close" />
        <v-toolbar-title class="text-body-1 font-weight-medium">Service Details</v-toolbar-title>
      </v-toolbar>

      <ServiceDetails :record="record" />
    </v-card>
  </v-dialog>

  <v-navigation-drawer v-else v-model="isOpen" location="right" width="450" temporary elevation="4" class="rounded-s-xl">
    <div class="d-flex flex-column h-100">
      <div class="d-flex align-center pa-2 pl-4 border-b">
        <span class="text-subtitle-1 font-weight-medium">Service Details</span>
        <v-spacer />
        <v-btn icon="mdi-close" variant="text" @click="close" />
      </div>

      <ServiceDetails :record="record" />
    </div>
  </v-navigation-drawer>
</template>

<style scoped>
.toolbar {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>

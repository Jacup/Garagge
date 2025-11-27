<script setup lang="ts">
import { computed } from 'vue'
import { useDisplay } from 'vuetify'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

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
    <v-card rounded="0">
      <v-toolbar density="compact" color="surface">
        <v-btn icon="mdi-close" @click="close" />
        <v-toolbar-title>Service Details (Mobile)</v-toolbar-title>
      </v-toolbar>
      <v-card-text class="pa-4">
        <h3 class="text-h6">Rekord ID: {{ record?.id }}</h3>
        <p>
          Tytuł: <strong>{{ record?.title }}</strong>
        </p>
        <p v-if="!record">Ładowanie danych lub brak rekordu...</p>
      </v-card-text>
    </v-card>
  </v-dialog>

  <v-navigation-drawer v-else v-model="isOpen" location="right" width="450" temporary elevation="4" class="rounded-s-xl">
    <div class="d-flex flex-column h-100">
      <div class="d-flex align-center pa-2 border-b">
        <span class="text-subtitle-1 font-weight-bold ml-2">Details (Desktop)</span>
        <v-spacer />
        <v-btn icon="mdi-close" variant="text" @click="close" />
      </div>

      <v-card-text class="pa-4 flex-grow-1 overflow-y-auto">
        <h3 class="text-h6">Rekord ID: {{ record?.id }}</h3>
        <p>
          Tytuł: <strong>{{ record?.title }}</strong>
        </p>
        <p v-if="!record">Ładowanie danych lub brak rekordu...</p>
      </v-card-text>
    </div>
  </v-navigation-drawer>
</template>

<style scoped>
/* Opcjonalne style dla wrappera */
</style>

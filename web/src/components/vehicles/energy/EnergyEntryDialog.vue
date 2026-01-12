<script setup lang="ts">
import { useDisplay } from 'vuetify'

interface Props {
  modelValue: boolean // Steruje widocznością (v-model)
  title?: string // Tytuł okna (np. "Nowe tankowanie")
  loading?: boolean // Opcjonalny stan ładowania przycisku zapisu
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  title: 'Wpis',
  loading: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean] // Do zamykania v-model
  save: [] // Sygnał: "Użytkownik chce zapisać"
}>()

// Hook Vuetify do wykrywania urządzenia mobilnego
const { mobile } = useDisplay()

function close() {
  emit('update:modelValue', false)
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    :fullscreen="mobile"
    :max-width="600"
    scrollable
    transition="dialog-bottom-transition"
  >
    <v-card class="d-flex flex-column h-100">
      <v-toolbar density="compact" color="surface" class="border-b">
        <v-btn icon="mdi-close" variant="text" @click="close"></v-btn>

        <v-toolbar-title class="text-h6 font-weight-bold">
          {{ title }}
        </v-toolbar-title>

        <v-spacer></v-spacer>

        <v-btn v-if="mobile" variant="text" color="primary" class="font-weight-bold" :loading="loading" @click="emit('save')">
          Zapisz
        </v-btn>
      </v-toolbar>

      <v-card-text class="pt-4 bg-surface">
        <slot></slot>
      </v-card-text>

      <v-card-actions v-if="!mobile" class="pa-4 border-t bg-surface">
        <v-spacer></v-spacer>
        <v-btn variant="text" @click="close"> Anuluj </v-btn>
        <v-btn color="primary" variant="flat" :loading="loading" @click="emit('save')"> Zapisz </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
/* Opcjonalne: Jeśli chcesz, aby nagłówek był "sticky" na mobile */
</style>

<script lang="ts" setup>
import { ref, watch, nextTick } from 'vue'
import type { ServiceTypeDto } from '@/api/generated'

const props = defineProps<{
  modelValue: boolean
  item?: ServiceTypeDto | null
  isLoading?: boolean
  apiError?: string | null
}>()

const emit = defineEmits(['update:modelValue', 'save', 'clear-error'])

const name = ref('')
const formRef = ref()
const isFormValid = ref(false)

const rules = {
  required: (v: string) => !!v || 'Field is required',
  max: (v: string) => v.length <= 64 || 'Max 64 characters',
}

watch(
  () => props.modelValue,
  async (isOpen) => {
    if (isOpen) {
      name.value = props.item?.name ?? ''
      await nextTick()
      formRef.value?.resetValidation()
    }
  },
)

watch(name, () => {
  if (props.apiError) {
    emit('clear-error')
  }
})

function close() {
  emit('update:modelValue', false)
}

async function submit() {
  const { valid } = await formRef.value.validate()

  if (valid && name.value.trim()) {
    emit('save', name.value.trim())
  }
}
</script>

<template>
  <v-dialog class="basic-dialog-container" :model-value="modelValue" @update:model-value="close" persistent>
    <v-card class="basic-dialog" :title="item ? 'Edit Service Type' : 'Add New Type'">
      <v-form ref="formRef" v-model="isFormValid" :disabled="isLoading" @submit.prevent="submit">
        <v-card-text class="pt-4">
          <v-text-field
            v-model="name"
            label="Type Name"
            placeholder="e.g. Oil Change"
            autofocus
            variant="outlined"
            :rules="[rules.required, rules.max]"
            maxlength="64"
            counter="64"
            required
            :disabled="isLoading"
            @keyup.esc="close"
          ></v-text-field>

          <v-expand-transition>
            <v-alert v-if="apiError" type="error" variant="tonal" class="mt-4" density="compact" icon="mdi-alert-circle">
              {{ apiError }}
            </v-alert>
          </v-expand-transition>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" :disabled="isLoading" @click="close">Cancel</v-btn>
          <v-btn color="primary" variant="flat" type="submit" :loading="isLoading" :disabled="!isFormValid"> Save </v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>
</template>

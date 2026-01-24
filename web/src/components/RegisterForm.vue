<script lang="ts" setup>
import { ref } from 'vue'
import type { RegisterRequest } from '@/api/generated/apiV1.schemas'

interface Props {
  loading?: boolean
  error?: string
}

interface Emits {
  (e: 'submit', payload: RegisterRequest): void
  (e: 'clearError'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  error: '',
})

const emit = defineEmits<Emits>()

const form = ref()
const email = ref('')
const firstName = ref('')
const lastName = ref('')
const password = ref('')
const passwordVisible = ref(false)

const rules = {
    required: (value: string | null) => !!value || 'This field is required.',
    passwordMinLength: (value: string) => value.length >= 8 || 'Password must be at least 8 characters long.',
    validEmail: (value: string) => /.+@.+\..+/.test(value) || 'Email is invalid.',
  }

async function validateAndSubmit() {
  const { valid } = await form.value.validate()

  if (valid) {
    emit('submit', {
      email: email.value,
      firstName: firstName.value,
      lastName: lastName.value,
      password: password.value,
    })
  }
}

function handleErrorDismiss() {
  if (props.error) emit('clearError')
}

defineExpose({
  submit: validateAndSubmit,
})
</script>

<template>
  <v-form ref="form" :disabled="loading" @submit.prevent="validateAndSubmit">
    <v-text-field
      v-model="email"
      label="Email"
      type="email"
      variant="outlined"
      autocomplete="email"
      required
      :rules="[rules.required, rules.validEmail]"
      class="mb-1"
      @update:model-value="handleErrorDismiss"
    />

    <v-text-field
      v-model="password"
      label="Password"
      :append-inner-icon="passwordVisible ? 'mdi-eye-off' : 'mdi-eye'"
      :type="passwordVisible ? 'text' : 'password'"
      :rules="[rules.required, rules.passwordMinLength]"
      variant="outlined"
      autocomplete="current-password"
      required
      class="mb-6"
      @click:append-inner="passwordVisible = !passwordVisible"
      @update:model-value="handleErrorDismiss"
      @keydown.enter="validateAndSubmit"
    />

    <v-text-field
      v-model="firstName"
      label="First Name"
      type="text"
      variant="outlined"
      class="mb-1"
      :rules="[rules.required]"
      @update:model-value="handleErrorDismiss"
    />

    <v-text-field
      v-model="lastName"
      label="Last Name"
      type="text"
      variant="outlined"
      class="mb-1"
      :rules="[rules.required]"
      @update:model-value="handleErrorDismiss"
    />

    <v-expand-transition>
      <v-alert v-if="error" type="error" variant="tonal" closable class="mt-4" @click:close="handleErrorDismiss">
        <template #close>
          <v-btn icon="mdi-close" variant="text" color="error" size="small" @click="handleErrorDismiss" />
        </template>
        {{ error }}
      </v-alert>
    </v-expand-transition>
  </v-form>
</template>

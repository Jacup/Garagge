<script lang="ts" setup>
import { ref } from 'vue'
import type { LoginRequest } from '@/api/generated/apiV1.schemas'

interface Props {
  loading?: boolean
  error?: string
}

interface Emits {
  (e: 'submit', payload: LoginRequest): void
  (e: 'clearError'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  error: '',
})

const emit = defineEmits<Emits>()

const form = ref()
const email = ref('')
const password = ref('')
const rememberMe = ref(false)
const passwordVisible = ref(false)

const emailRules = [(v: string) => !!v || 'Email is required', (v: string) => /.+@.+\..+/.test(v) || 'Email must be valid']

const passwordRules = [(v: string) => !!v || 'Password is required', (v: string) => v.length >= 8 || 'Minimum 8 characters']

async function validateAndSubmit() {
  const { valid } = await form.value.validate()

  if (valid) {
    emit('submit', {
      email: email.value,
      password: password.value,
      rememberMe: rememberMe.value,
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
      :rules="emailRules"
      class="mb-2"
      @update:model-value="handleErrorDismiss"
    />

    <v-text-field
      v-model="password"
      label="Password"
      :append-inner-icon="passwordVisible ? 'mdi-eye-off' : 'mdi-eye'"
      :type="passwordVisible ? 'text' : 'password'"
      :rules="passwordRules"
      variant="outlined"
      autocomplete="current-password"
      required
      class="mb-2"
      @click:append-inner="passwordVisible = !passwordVisible"
      @update:model-value="handleErrorDismiss"
      @keydown.enter="validateAndSubmit"
    />

    <v-checkbox v-model="rememberMe" label="Remember me" hide-details density="comfortable" />

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

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoginForm from '@/components/auth/LoginForm.vue'
import type { LoginRequest } from '@/api/generated/apiV1.schemas'

withDefaults(defineProps<{ showCreateAccount?: boolean }>(), {
  showCreateAccount: true,
})

const authStore = useAuthStore()
const router = useRouter()

const loginFormRef = ref<InstanceType<typeof LoginForm>>()
const loading = ref(false)
const error = ref('')

async function handleLogin(credentials: LoginRequest) {
  error.value = ''
  loading.value = true

  try {
    await authStore.login(credentials)
    await router.push('/')
  } catch (e: unknown) {
    if (e instanceof Error) {
      error.value = e.message
    } else {
      error.value = 'Login failed. Please try again.'
    }
  } finally {
    loading.value = false
  }
}

function submitForm() {
  loginFormRef.value?.submit()
}
</script>

<template>
  <v-card class="mx-auto" width="450" variant="flat" color="transparent">
    <v-card-title class="px-0 pt-6 pb-6 text-h4 font-weight-bold"> Welcome back </v-card-title>

    <v-card-text class="px-0">
      <LoginForm ref="loginFormRef" :loading="loading" :error="error" @submit="handleLogin" @clear-error="error = ''" />
    </v-card-text>

    <v-card-actions class="px-0 pt-4">
      <v-btn v-if="showCreateAccount" variant="text" :disabled="loading" @click="router.push('/register')"> Create account </v-btn>

      <v-spacer />

      <v-btn color="primary" variant="flat" size="large" :loading="loading" @click="submitForm"> Login </v-btn>
    </v-card-actions>
  </v-card>
</template>

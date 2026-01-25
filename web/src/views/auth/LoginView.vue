<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoginForm from '@/components/auth/LoginForm.vue'
import type { LoginRequest } from '@/api/generated/apiV1.schemas'

withDefaults(defineProps<{ showCreateAccount?: boolean }>(), {
  showCreateAccount: true,
})

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const loginFormRef = ref<InstanceType<typeof LoginForm>>()
const loading = ref(false)
const error = ref('')

async function handleLogin(credentials: LoginRequest) {
  error.value = ''
  loading.value = true

  try {
    await authStore.login(credentials)

    const redirectPath = route.query.redirect as string | undefined
    await router.push(redirectPath || '/')
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
  <v-card max-width="512" width="100%" variant="flat" color="transparent">
    <v-card-title class="px-0 pt-0 pb-6 text-h4 font-weight-bold text-wrap"> Login </v-card-title>

    <v-card-text class="px-0">
      <LoginForm ref="loginFormRef" :loading="loading" :error="error" @submit="handleLogin" @clear-error="error = ''" />
    </v-card-text>

    <v-card-actions class="pa-0 d-flex flex-column flex-sm-row">
      <v-btn v-if="showCreateAccount" variant="text" :disabled="loading" @click="router.push('/register')" class="mb-2 mb-sm-0">
        Create new account
      </v-btn>

      <v-spacer class="d-none d-sm-flex" />

      <v-btn color="primary" variant="flat" size="large" :loading="loading" @click="submitForm" :block="$vuetify.display.xs"> Login </v-btn>
    </v-card-actions>
  </v-card>
</template>

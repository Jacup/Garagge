<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import RegisterForm from '@/components/RegisterForm.vue'
import type { RegisterRequest } from '@/api/generated/apiV1.schemas'

withDefaults(defineProps<{ showBackToLogin?: boolean }>(), {
  showBackToLogin: true,
})

const authStore = useAuthStore()
const router = useRouter()

const registerFormRef = ref<InstanceType<typeof RegisterForm>>()
const loading = ref(false)
const error = ref('')

async function handleRegister(request: RegisterRequest) {
  error.value = ''
  loading.value = true

  try {
    await authStore.register(request)
    await router.push('/')
  } catch (e: unknown) {
    if (e instanceof Error) {
      error.value = e.message
    } else {
      error.value = 'Registration failed. Please try again.'
    }
  } finally {
    loading.value = false
  }
}

function submitForm() {
  registerFormRef.value?.submit()
}
</script>

<template>
  <v-card class="mx-auto" width="450" variant="flat" color="transparent">
    <v-card-title class="px-0 pt-6 pb-6 text-h4 font-weight-bold"> Register </v-card-title>

    <v-card-text class="px-0">
      <RegisterForm
        ref="registerFormRef"
        :loading="loading"
        :error="error"
        @submit="handleRegister"
        @clear-error="error = ''"
      />
    </v-card-text>

    <v-card-actions class="px-0 pt-4">
      <v-btn v-if="showBackToLogin" variant="text" :disabled="loading" @click="router.push('/login')"> Back to login </v-btn>

      <v-spacer />

      <v-btn color="primary" variant="flat" size="large" :loading="loading" @click="submitForm"> Register </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const email = ref('')
const password = ref('')
const rememberMe = ref(false)
const error = ref('')
const loading = ref(false)

const authStore = useAuthStore()
const router = useRouter()

async function onSubmit() {
  error.value = ''
  loading.value = true
  try {
    await authStore.login({
      email: email.value,
      password: password.value,
      rememberMe: rememberMe.value,
    })

    router.push('/')
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Login failed'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-form">
    <v-card>
      <v-card-title class="text-h5 text-center mb-4"> Logowanie </v-card-title>

      <v-form @submit.prevent="onSubmit">
        <v-text-field v-model="email" label="Email" type="email" variant="outlined" required class="form-field" :disabled="loading" />

        <v-text-field v-model="password" label="Hasło" type="password" variant="outlined" required class="form-field" :disabled="loading" />

        <v-checkbox v-model="rememberMe" label="Nie wylogowuj mnie" :disabled="loading" />

        <v-btn type="submit" color="primary" variant="elevated" block size="large" class="mt-4" :loading="loading" :disabled="loading">
          Zaloguj
        </v-btn>

        <v-alert v-if="error" type="error" variant="tonal" class="mt-4">
          {{ error }}
        </v-alert>
      </v-form>
    </v-card>
  </div>
</template>

<style scoped>
/* Wszystkie style są teraz globalne w main.css lub pochodzą z Vuetify theme */
</style>

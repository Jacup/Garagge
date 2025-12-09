<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'


const email = ref('')
const firstName = ref('')
const lastName = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

const authStore = useAuthStore()
const router = useRouter()

async function onSubmit() {
  error.value = ''
  loading.value = true

  try {
    authStore.register({
      email: email.value,
      firstName: firstName.value,
      lastName: lastName.value,
      password: password.value,
    })
    router.push('/login')
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'An error occurred during registration'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-form">
    <v-card>
      <v-card-title class="text-h5 text-center mb-4"> Rejestracja </v-card-title>

      <v-form @submit.prevent="onSubmit">
        <v-text-field v-model="email" label="Email" type="email" variant="outlined" required class="form-field" :disabled="loading" />

        <v-text-field v-model="firstName" label="Imię" type="text" variant="outlined" required class="form-field" :disabled="loading" />

        <v-text-field v-model="lastName" label="Nazwisko" type="text" variant="outlined" required class="form-field" :disabled="loading" />

        <v-text-field v-model="password" label="Hasło" type="password" variant="outlined" required class="form-field" :disabled="loading" />

        <v-btn type="submit" color="primary" variant="elevated" block size="large" class="mt-4" :loading="loading" :disabled="loading">
          Zarejestruj
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

<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

import { getUsers } from '@/api/generated/users/users'
import { useUserStore } from '@/stores/userStore'

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const userStore = useUserStore()
const router = useRouter()

const { postApiUsersLogin, getApiUsersMe } = getUsers()

async function onSubmit() {
  error.value = ''
  loading.value = true
  try {
    const loginRes = await postApiUsersLogin({ email: email.value, password: password.value })

    if (!loginRes.data.accessToken) {
      throw new Error('Missing access token in login response')
    }

    userStore.setToken(loginRes.data.accessToken)

    const profileRes = await getApiUsersMe()
    if (!profileRes.data) {
      throw new Error('Failed to fetch user profile')
    }
    userStore.setProfile(profileRes.data)
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

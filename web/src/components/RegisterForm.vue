<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { getUsers } from '@/api/generated/users/users'
import { useUserStore } from '@/stores/userStore'

const { postApiUsersRegister, postApiUsersLogin, getApiUsersMe } = getUsers()

const email = ref('')
const firstName = ref('')
const lastName = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const userStore = useUserStore()
const router = useRouter()

async function onSubmit() {
  error.value = ''
  loading.value = true

  try {
    const registerRes = await postApiUsersRegister({
      email: email.value,
      firstName: firstName.value,
      lastName: lastName.value,
      password: password.value,
    })

    if (registerRes.status === 409) {
      error.value = 'Email already exists.'
      loading.value = false
      return
    }
    if (registerRes.status >= 400) {
      error.value = 'Registration failed: ' + (registerRes.statusText || 'Unknown error')
      loading.value = false
      return
    }

    const loginRes = await postApiUsersLogin({
      email: email.value,
      password: password.value,
    })

    if (loginRes.status !== 200 || !loginRes.data?.accessToken) {
      error.value = 'Login failed: ' + (loginRes.statusText || 'Unknown error')
      loading.value = false
      return
    }
    userStore.setToken(loginRes.data.accessToken)

    const profileRes = await getApiUsersMe()
    if (profileRes.status !== 200 || !profileRes.data) {
      error.value = 'Failed to fetch profile: ' + (profileRes.statusText || 'Unknown error')
      loading.value = false
      return
    }
    userStore.setProfile(profileRes.data)
    router.push('/')
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

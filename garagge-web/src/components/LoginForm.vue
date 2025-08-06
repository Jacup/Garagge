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

const { postUsersLogin, getUsersMe } = getUsers()

async function onSubmit() {
  error.value = ''
  loading.value = true
  try {
    const loginRes = await postUsersLogin({ email: email.value, password: password.value })

    if (!loginRes.data.accessToken) {
      throw new Error('Missing access token in login response')
    }

    userStore.setToken(loginRes.data.accessToken)

    const profileRes = await getUsersMe()
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
  <form class="login-form" @submit.prevent="onSubmit">
    <h2>Logowanie</h2>
    <div class="form-group">
      <label for="email">Email</label>
      <input v-model="email" id="email" type="email" required />
    </div>
    <div class="form-group">
      <label for="password">Hasło</label>
      <input v-model="password" id="password" type="password" required />
    </div>
    <button type="submit" :disabled="loading">Zaloguj</button>
    <div v-if="error" class="error">{{ error }}</div>
  </form>
</template>

<style scoped>
.login-form {
  max-width: 350px;
  margin: 2rem auto;
  padding: 2rem;
  background: #1e293b;
  border-radius: 8px;
  color: #fff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}
.form-group {
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
}
label {
  margin-bottom: 0.25rem;
}
input {
  padding: 0.5rem;
  border-radius: 4px;
  border: 1px solid #334155;
  background: #0f172a;
  color: #fff;
}
button {
  width: 100%;
  padding: 0.75rem;
  background: #334155;
  color: #fff;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  margin-top: 1rem;
}
button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
.error {
  color: #f87171;
  margin-top: 1rem;
  text-align: center;
}
</style>

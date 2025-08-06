<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { getUsers } from '@/api/generated/users/users'
import { useUserStore } from '@/stores/userStore'

const { postUsersRegister, postUsersLogin, getUsersMe } = getUsers()

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
    const registerRes = await postUsersRegister({
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

    const loginRes = await postUsersLogin({
      email: email.value,
      password: password.value,
    })

    if (loginRes.status !== 200 || !loginRes.data?.accessToken) {
      error.value = 'Login failed: ' + (loginRes.statusText || 'Unknown error')
      loading.value = false
      return
    }
    userStore.setToken(loginRes.data.accessToken)

    const profileRes = await getUsersMe()
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
  <form class="register-form" @submit.prevent="onSubmit">
    <h2>Register</h2>
    <div class="form-group">
      <label for="email">Email</label>
      <input v-model="email" id="email" type="email" required />
    </div>
    <div class="form-group">
      <label for="firstName">First name</label>
      <input v-model="firstName" id="firstName" type="text" required />
    </div>
    <div class="form-group">
      <label for="lastName">Last name</label>
      <input v-model="lastName" id="lastName" type="text" required />
    </div>
    <div class="form-group">
      <label for="password">Password</label>
      <input v-model="password" id="password" type="password" required />
    </div>
    <button type="submit" :disabled="loading">Zarejestruj</button>
    <div v-if="error" class="error">{{ error }}</div>
  </form>
</template>

<style scoped>
.register-form {
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

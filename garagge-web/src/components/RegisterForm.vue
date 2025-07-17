<template>
  <form class="register-form" @submit.prevent="onSubmit">
    <h2>Rejestracja</h2>
    <div class="form-group">
      <label for="email">Email</label>
      <input v-model="email" id="email" type="email" required />
    </div>
    <div class="form-group">
      <label for="firstName">Imię</label>
      <input v-model="firstName" id="firstName" type="text" required />
    </div>
    <div class="form-group">
      <label for="lastName">Nazwisko</label>
      <input v-model="lastName" id="lastName" type="text" required />
    </div>
    <div class="form-group">
      <label for="password">Hasło</label>
      <input v-model="password" id="password" type="password" required />
    </div>
    <button type="submit" :disabled="loading">Zarejestruj</button>
    <div v-if="error" class="error">{{ error }}</div>
  </form>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import { register } from '@/api/userApi'
import { useUserStore } from '@/stores/userStore'
import { useRouter } from 'vue-router'

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
    const res = await register(email.value, firstName.value, lastName.value, password.value)
    // Załóżmy, że backend zwraca token i dane użytkownika
    userStore.setToken(res.accessToken)
    userStore.setProfile({
      userId: res.userId,
      email: res.email,
      firstName: res.firstName,
      lastName: res.lastName,
    })
    router.push('/')
  } catch (e: any) {
    error.value = e.message || 'Błąd rejestracji'
  } finally {
    loading.value = false
  }
}
</script>

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

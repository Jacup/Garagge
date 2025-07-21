import { useUserStore } from '@/stores/userStore'

const BASE_URL = import.meta.env.VITE_API_URL || ''

export async function login(email: string, password: string) {
  const res = await fetch(`${BASE_URL}/users/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })

  if (!res.ok) throw new Error('Login failed')

  return res.json()
}

export async function getUserProfile() {
  const userStore = useUserStore()
  const res = await fetch(`${BASE_URL}/users/me`, {
    headers: { Authorization: `Bearer ${userStore.accessToken}` },
  })

  if (!res.ok) throw new Error('Fetch profile failed')

  return res.json()
}

export async function register(email: string, firstName: string, lastName: string, password: string) {
  const res = await fetch(`${BASE_URL}/users/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, firstName, lastName, password }),
  })

  if (!res.ok) throw new Error('Registration failed')

  return res.json()
}

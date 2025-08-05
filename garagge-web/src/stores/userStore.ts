// src/stores/userStore.ts
import { defineStore } from 'pinia'
import type { UserDto } from '@/api/apiV1.schemas'
import { getUsers } from '@/api/generated/users/users'

const { getUsersMe } = getUsers()

type StoredUser = {
  userId: string
  email: string
  firstName: string
  lastName: string
} | null

interface UserState {
  accessToken: string
  user: StoredUser
}

const USER_STORAGE_KEY = 'app_user'
const TOKEN_STORAGE_KEY = 'app_token'

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    accessToken: localStorage.getItem(TOKEN_STORAGE_KEY) || '',
    user: JSON.parse(localStorage.getItem(USER_STORAGE_KEY) || 'null'),
  }),
  actions: {
    setToken(accessToken: string) {
      this.accessToken = accessToken
      localStorage.setItem(TOKEN_STORAGE_KEY, accessToken)
    },
    clearToken() {
      this.accessToken = ''
      this.user = null
      localStorage.removeItem(TOKEN_STORAGE_KEY)
      localStorage.removeItem(USER_STORAGE_KEY)
    },
    setProfile(user: UserDto) {
      this.user = {
        userId: user.id ?? '',
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
      }
      localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(this.user))
    },
    async initializeStore() {
      const token = localStorage.getItem(TOKEN_STORAGE_KEY)
      if (!token) return
      if (!this.accessToken) this.accessToken = token

      try {
        const res = await getUsersMe()
        if (res.data) {
          this.setProfile(res.data)
        }
      } catch (err) {
        this.clearToken()
      }
    },
  },
})

import { defineStore } from 'pinia'
import { getAuth } from '@/api/generated/auth/auth'
import { useUserStore } from './user'
import type { LoginRequest, RegisterRequest } from '@/api/generated/apiV1.schemas'
import { parseApiError } from '@/utils/error-handler'
import router from '@/router'

const { postApiAuthLogin, postApiAuthRegister, postApiAuthLogout } = getAuth()

export const useAuthStore = defineStore('auth', {
  state: () => ({
    isSuperAdmin: true as boolean,
  }),

  getters: {
    isAuthenticated(): boolean {
      const userStore = useUserStore()
      return userStore.id == '' ? false : true
    },
  },

  actions: {
    async login(loginRequest: LoginRequest) {
      const userStore = useUserStore()

      try {
        await postApiAuthLogin(loginRequest)

        await userStore.fetchUserData()
      } catch (error) {
        const parsedError = parseApiError(error)
        throw new Error(parsedError.message)
      }
    },

    async logout() {
      const userStore = useUserStore()
      try {
        await postApiAuthLogout()
      } catch (error) {
        console.error('Logout API failed, but clearing local state anyway', error)
      } finally {
        this.$reset()
        userStore.$reset()
      }
      router.push({ name: 'Login' })
    },

    async register(registerRequest: RegisterRequest) {
      try {
        await postApiAuthRegister(registerRequest)
      } catch (error) {
        const parsedError = parseApiError(error)
        throw new Error(parsedError.message)
      }
    },
  },
})

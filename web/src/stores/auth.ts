import { defineStore } from 'pinia'
import { getAuth } from '@/api/generated/auth/auth'
import { useUserStore } from './user'
import type { LoginRequest, RegisterRequest } from '@/api/generated/apiV1.schemas'
import { parseApiError } from '@/utils/error-handler'
import router from '@/router'

const { postApiAuthLogin, postApiAuthRegister, postApiAuthLogout } = getAuth()

export const useAuthStore = defineStore('auth', {
  state: () => ({}),

  getters: {
    isAuthenticated(): boolean {
      const userStore = useUserStore()
      return !!userStore.id
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

      this.$reset()
      userStore.$reset()

      try {
        await postApiAuthLogout()
      } catch (error) {
        console.error('Logout API failed, but clearing local state anyway', error)
      }

      if (router.currentRoute.value.name !== 'Login') {
        router.push({ name: 'Login' })
      }
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

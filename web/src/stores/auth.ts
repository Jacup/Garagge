import { defineStore } from 'pinia'
import { getAuth } from '@/api/generated/auth/auth'
import { useUserStore } from './user'
import type { LoginRequest, RegisterRequest } from '@/api/generated/apiV1.schemas'
import { parseApiError } from '@/utils/error-handler'

const { postApiAuthLogin, postApiAuthRegister, postApiAuthLogout } = getAuth()

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: null as string | null,
    isSuperAdmin: true as boolean,
  }),

  persist: true,

  getters: {
    isAuthenticated: (state) => !!state.accessToken,
  },

  actions: {
    setToken(token: string | null) {
      this.accessToken = token
    },

    async login(loginRequest: LoginRequest) {
      const userStore = useUserStore()

      try {
        const response = await postApiAuthLogin(loginRequest)

        if (!response.accessToken) {
          throw new Error('Login failed: No access token received')
        }

        this.setToken(response.accessToken)

        userStore.fetchUserData()
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
        this.setToken(null)
        this.$reset()
        userStore.$reset()
        localStorage.clear()
      }
    },

    async register(registerRequest: RegisterRequest) {
      try {
        await postApiAuthRegister(registerRequest)
      } catch (error) {
        const parsedError = parseApiError(error)
        throw new Error(parsedError.message)
      }

      console.log('Registration successful:')
    },
  },
})

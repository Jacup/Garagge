import { defineStore } from 'pinia'
import { getAuth } from '@/api/generated/auth/auth'
import { useUserStore } from './user'
import type { LoginRequest, RegisterRequest } from '@/api/generated/apiV1.schemas'
import axios from 'axios'

const { postApiAuthLogin, postApiAuthRegister, postApiAuthLogout } = getAuth()

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: null as string | null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.accessToken,
  },

  actions: {
    setToken(token: string | null) {
      this.accessToken = token
    },

    async login(loginRequest: LoginRequest) {
      const userStore = useUserStore()

      const response = await postApiAuthLogin(loginRequest)

      if (!response.accessToken) {
        throw new Error('Login failed: No access token received')
      }

      this.setToken(response.accessToken)

      userStore.fetchUserData()
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
        if (axios.isAxiosError(error) && error.response) {
          const status = error.response.status
          if (status === 400) {
            console.error('Registration failed: Bad Request -', error.response.data)
            return
          } else if (status === 409) {
            console.error('Registration failed: Conflict - Email already in use.')
            return
          }
        }

        console.error('Registration failed:', error)
      }

      console.log('Registration successful:')
    },
  },
})

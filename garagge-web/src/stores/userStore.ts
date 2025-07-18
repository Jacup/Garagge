import { defineStore } from 'pinia'
import { getUserProfile } from '@/api/userApi'

export const useUserStore = defineStore('user', {
  state: () => ({
    accessToken: localStorage.getItem('accessToken') || ('' as string),
    userId: localStorage.getItem('userId') || ('' as string),
    email: localStorage.getItem('email') || ('' as string),
    firstName: localStorage.getItem('firstName') || ('' as string),
    lastName: localStorage.getItem('lastName') || ('' as string),
  }),

  actions: {
    setToken(accessToken: string) {
      this.accessToken = accessToken
      localStorage.setItem('accessToken', accessToken)
    },

    clearToken() {
      this.accessToken = ''
      this.userId = ''
      this.email = ''
      this.firstName = ''
      this.lastName = ''
      localStorage.removeItem('accessToken')
      localStorage.removeItem('userId')
      localStorage.removeItem('email')
      localStorage.removeItem('firstName')
      localStorage.removeItem('lastName')
    },

    setProfile(data: { userId: string; email: string; firstName: string; lastName: string }) {
      this.userId = data.userId
      this.email = data.email
      this.firstName = data.firstName
      this.lastName = data.lastName
      // Zapisz również dane profilu w localStorage, aby były trwałe
      localStorage.setItem('userId', data.userId)
      localStorage.setItem('email', data.email)
      localStorage.setItem('firstName', data.firstName)
      localStorage.setItem('lastName', data.lastName)
    },
    async initializeStore() {
      // Stan jest już ustawiony na podstawie localStorage w 'state',
      // ale możesz tutaj dodać logikę weryfikacji tokenu np. z API
      const storedToken = localStorage.getItem('accessToken')
      if (storedToken && !this.accessToken) {
        this.accessToken = storedToken
        const profile = await getUserProfile()
        if (profile != null) {
          this.setProfile({
            userId: profile.userId,
            email: profile.email,
            firstName: profile.firstName,
            lastName: profile.lastName,
          })
        }
      }
    },
  },
})

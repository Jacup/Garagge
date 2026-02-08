import { defineStore } from 'pinia'
import type { UserDto } from '@/api/generated/apiV1.schemas'
import { getUsers } from '@/api/generated/users/users'

const { getApiUsersMe } = getUsers()

export const useUserStore = defineStore('user', {
  state: () => ({
    profile: null as UserDto | null,
  }),

  getters: {
    fullName: (state) => (state.profile ? `${state.profile.firstName} ${state.profile.lastName}` : ''),
    email: (state) => state.profile?.email || '',
    id: (state) => state.profile?.id || '',
  },

  actions: {
    async fetchUserData() {
      try {
        const res = await getApiUsersMe()
        this.profile = res
      } catch (error) {
        console.error('Failed to fetch profile:', error)
      }
    },
  },
})

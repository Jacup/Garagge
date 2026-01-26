import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useNotificationsStore = defineStore('notifications', {
  state: () => ({
    queue: ref<string[]>([])
  }),

  actions: {
    show(text: string) {
      this.queue.push(text)
    },
  },
})

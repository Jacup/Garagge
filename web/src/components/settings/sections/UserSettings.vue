<script lang="ts" setup>
import { useUserStore } from '@/stores/user'

import PasswordChangeItem from '@/components/settings/sections/user/PasswordChangeItem.vue'
import UserDetailsItem from '@/components/settings/sections/user/UserDetailsItem.vue'
import { useNotificationsStore } from '@/stores/notifications'

const userStore = useUserStore()
const notifications = useNotificationsStore()

const copyId = () => {
  if (userStore.id) {
    navigator.clipboard
      .writeText(userStore.id)
      .then(() => {
        notifications.show('User ID copied to clipboard.')
      })
      .catch(() => {
        notifications.show('Failed to copy User ID.')
      })
  }
}
</script>

<template>
  <v-list-item class="inner-item">
    <v-text-field
      v-model="userStore.id"
      label="User ID"
      variant="outlined"
      density="comfortable"
      readonly
      hide-details
      append-icon="mdi-content-copy"
      @click:append="copyId()"
      class="my-2"
    />
  </v-list-item>

  <UserDetailsItem />

  <PasswordChangeItem />
</template>

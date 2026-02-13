<script lang="ts" setup>
import { useUserStore } from '@/stores/user'

import PasswordChangeItem from '@/components/settings/sections/user/PasswordChangeItem.vue'
import UserDetailsItem from '@/components/settings/sections/user/UserDetailsItem.vue'

const userStore = useUserStore()
</script>

<template>
  <v-list-item class="inner-item">
    <v-list-item-title>User ID</v-list-item-title>
    <v-list-item-subtitle>
      {{ userStore.id }}
    </v-list-item-subtitle>
    <template #append>
      <v-btn icon="mdi-content-copy" variant="text" size="small" disabled></v-btn>
    </template>
  </v-list-item>

  <UserDetailsItem />

  <PasswordChangeItem />
</template>

<script lang="ts" setup>
import { useUserStore } from '@/stores/user'
import PasswordChangeItem from '@/components/settings/sections/user/PasswordChangeItem.vue'
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

  <v-list-item class="inner-item">
    <v-list-item-title>Email</v-list-item-title>
    <v-list-item-subtitle> {{ userStore.email }} </v-list-item-subtitle>
  </v-list-item>

  <v-list-item class="inner-item">
    <v-list-item-title>First name</v-list-item-title>
    <v-list-item-subtitle> {{ userStore.profile?.firstName }} </v-list-item-subtitle>
  </v-list-item>

  <v-list-item class="inner-item">
    <v-list-item-title>Last name</v-list-item-title>
    <v-list-item-subtitle> {{ userStore.profile?.lastName }} </v-list-item-subtitle>
  </v-list-item>

  <PasswordChangeItem />
</template>

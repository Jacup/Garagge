<script lang="ts" setup>
import { useUserStore } from '@/stores/user'

const userStore = useUserStore()
</script>

<template>
  <v-list-item title="User ID" class="inner-item list-item">
    <template #append>
      <v-text-field density="compact" disabled hide-details>{{ userStore.id }}</v-text-field>
    </template>
  </v-list-item>
  <v-list-item title="Email" class="inner-item list-item">
    <template #append>
      <v-text-field density="compact" disabled hide-details>{{ userStore.email }}</v-text-field>
    </template>
  </v-list-item>
  <v-list-item title="First name" class="inner-item list-item">
    <template #append>
      <v-text-field density="compact" disabled hide-details>{{ userStore.profile?.firstName }}</v-text-field>
    </template>
  </v-list-item>
  <v-list-item title="Last name" class="inner-item list-item">
    <template #append>
      <v-text-field density="compact" disabled hide-details>{{ userStore.profile?.lastName }}</v-text-field>
    </template>
  </v-list-item>
  <v-list-item title="Password" class="inner-item list-item" lines="three">
    <template #append>
      <v-btn variant="text" disabled>Change Password</v-btn>
    </template>
  </v-list-item>
</template>

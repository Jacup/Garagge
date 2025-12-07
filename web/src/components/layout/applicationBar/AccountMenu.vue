<script lang="ts" setup>
import { useAuthStore } from '@/stores/auth'
import { useUserStore } from '@/stores/user'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const userStore = useUserStore()

const router = useRouter()

const handleLogout = () => {
  authStore.logout()
  router.push('/')
}
</script>

<template>
  <v-menu location="bottom end" transition="fade-transition" offset="4">
    <template #activator="{ props }">
      <v-btn v-bind="props" icon size="48" class="rounded-circle">
        <v-avatar color="surface-variant" size="32">
          <span class="text-h8">{{ userStore.profile?.firstName?.[0] }}{{ userStore.profile?.lastName?.[0] }}</span>
        </v-avatar>
      </v-btn>
    </template>

    <v-card class="mx-auto" max-width="300">
      <template v-slot:prepend>
        <v-avatar color="surface-variant" size="40">
          <span class="text-h6">{{ userStore.profile?.firstName?.[0] }}{{ userStore.profile?.lastName?.[0] }}</span>
        </v-avatar>
      </template>
      <template v-slot:title> {{ userStore.fullName }} </template>
      <template v-slot:subtitle>{{ userStore.profile?.email }}</template>

      <v-list density="comfortable">
        <v-list-item v-if="authStore.isAuthenticated" prepend-icon="mdi-account" to="/profile" title="Profile" />
        <v-list-item v-if="authStore.isAuthenticated" prepend-icon="mdi-cog" to="/settings" title="Settings" />
      </v-list>
      <v-card-actions>
        <v-btn variant="tonal" block @click="handleLogout">
          <v-icon left>mdi-logout</v-icon>
          Logout
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-menu>
</template>

<style scoped></style>

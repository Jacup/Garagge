<script lang="ts" setup>
import { useRouter } from 'vue-router'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useAuthStore } from '@/stores/auth'
import { useUserStore } from '@/stores/user'

const authStore = useAuthStore()
const userStore = useUserStore()
const { isMobile } = useResponsiveLayout()
const router = useRouter()

const handleLogout = async () => {
  await authStore.logout()
  router.push('/')
}
</script>

<template>
  <v-menu location="bottom end" transition="fade-transition" offset="4" :scrim="isMobile">
    <template #activator="{ props }">
      <v-btn v-bind="props" icon size="48" class="rounded-circle">
        <v-avatar color="surface-variant" size="32">
          <span class="text-h8">{{ userStore.profile?.firstName?.[0] }}{{ userStore.profile?.lastName?.[0] }}</span>
        </v-avatar>
      </v-btn>
    </template>

    <v-card width="360" variant="flat" class="profile-dialog-card pa-6">
      <template v-slot:prepend>
        <v-avatar color="surface-variant" size="40" class="mr-2">
          <span class="text-h6">{{ userStore.profile?.firstName?.[0] }}{{ userStore.profile?.lastName?.[0] }}</span>
        </v-avatar>
      </template>
      <template v-slot:title> {{ userStore.fullName }} </template>
      <template v-slot:subtitle>{{ userStore.profile?.email }}</template>

      <v-list class="md3-list">
        <v-list-item class="md3-list-item" disabled prepend-icon="mdi-account" to="/profile" title="Profile" />
        <v-list-item class="md3-list-item" prepend-icon="mdi-cog" to="/settings" title="Settings" />
        <v-list-item
          v-if="authStore.isSuperAdmin"
          disabled
          class="md3-list-item"
          prepend-icon="mdi-cog"
          to="/admin-settings"
          title="Administration"
        />
      </v-list>
      <v-card-actions class="pt-4 pb-0 px-0">
        <v-btn variant="tonal" prepend-icon="mdi-logout" block @click="handleLogout"> Logout </v-btn>
      </v-card-actions>
    </v-card>
  </v-menu>
</template>

<style scoped>
.profile-dialog-card {
  background-color: rgb(var(--v-theme-surface-container-high)) !important;
  border-radius: 28px !important;
}

:deep(.v-card-item) {
  padding: 0 !important;
  margin-bottom: 16px;
}

.md3-list {
  background-color: transparent !important;
  padding: 0px;
}
.md3-list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
  border-radius: 2px !important;
}

.md3-list-item:first-child {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}

.md3-list-item:last-child {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
}
</style>

<script lang="ts" setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useUserStore } from '@/stores/userStore'
import AccountMenu from '@/components/NavBar/AccountMenu.vue'

const emit = defineEmits<{
  toggleDrawer: []
}>()

const router = useRouter()
const userStore = useUserStore()
const { isMobile } = useResponsiveLayout()

const isLoggedIn = computed(() => !!userStore.accessToken)

const navigateToLogin = () => router.push('/login')
const navigateToRegister = () => router.push('/register')
const handleDrawerToggle = () => emit('toggleDrawer')
</script>

<template>
  <v-app-bar elevation="1">
    <v-app-bar-nav-icon
      v-if="isMobile"
      @click="handleDrawerToggle"
      aria-label="Toggle navigation menu"
    />

    <v-app-bar-title>Application bar</v-app-bar-title>

    <v-spacer />

    <template v-slot:append>
      <div v-if="isLoggedIn" class="user-section">
        <AccountMenu />
      </div>
      <div v-else class="auth-section">
        <v-btn
          variant="text"
          @click="navigateToLogin"
          aria-label="Go to login page"
        >
          Login
        </v-btn>
        <v-btn
          variant="tonal"
          @click="navigateToRegister"
          aria-label="Go to register page"
        >
          Register
        </v-btn>
      </div>
    </template>
  </v-app-bar>
</template>

<style scoped>
.user-section,
.auth-section {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>

<script lang="ts" setup>
import AccountMenu from '@/components/NavBar/AccountMenu.vue'
import { useUserStore } from '@/stores/userStore'
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const { isMobile } = defineProps<{
  isMobile: boolean
}>()

const emit = defineEmits<{
  (e: 'update:drawer'): void
}>()
const router = useRouter()
const userStore = useUserStore()

const isLoggedIn = computed(() => !!userStore.accessToken)

const goToLogin = () => router.push('/login')
const goToRegister = () => router.push('/register')
</script>

<template>
  <v-app-bar>
    <v-app-bar-nav-icon v-if="isMobile" @click="emit('update:drawer')" />
    <v-app-bar-title>Application bar</v-app-bar-title>
    <v-spacer />

    <template v-slot:append>
      <template v-if="isLoggedIn">
        <AccountMenu />
      </template>
      <template v-else>
        <v-btn variant="text" @click="goToLogin">Login</v-btn>
        <v-btn variant="tonal" @click="goToRegister">Register</v-btn>
      </template>
    </template>
  </v-app-bar>
</template>

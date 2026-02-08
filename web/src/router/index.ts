import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

import DashboardView from '../views/home/DashboardView.vue'
import RegisterView from '../views/auth/RegisterView.vue'
import LoginView from '../views/auth/LoginView.vue'
import VehiclesView from '@/views/vehicles/VehiclesView.vue'
import VehicleView from '@/views/vehicles/VehicleView.vue'
import AddServiceRecordView from '@/views/vehicles/AddServiceRecordView.vue'
import SettingsView from '@/views/settings/SettingsView.vue'

import ApplicationLayout from '@/layouts/ApplicationLayout.vue'
import SetupLayout from '@/layouts/SetupLayout.vue'
import { useUserStore } from '@/stores/user'

const routes = [
  {
    path: '/auth',
    component: SetupLayout,
    children: [
      {
        path: 'login',
        name: 'Login',
        component: LoginView,
        meta: { requiresGuest: true },
      },
      {
        path: 'register',
        name: 'Register',
        component: RegisterView,
        meta: { requiresGuest: true },
      },
    ],
  },
  {
    path: '/',
    component: ApplicationLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: DashboardView,
      },
      {
        path: 'vehicles',
        name: 'Vehicles',
        component: VehiclesView,
      },
      {
        path: 'vehicles/:id',
        name: 'VehicleView',
        component: VehicleView,
        meta: {
          appBar: {
            type: 'context',
          },
        },
      },
      {
        path: 'vehicles/:id/services/add',
        name: 'AddServiceRecord',
        component: AddServiceRecordView,
        props: true,
      },
      {
        path: 'settings',
        name: 'Settings',
        component: SettingsView,
      },
    ],
  },

  {
    path: '/login',
    redirect: '/auth/login',
  },
  {
    path: '/register',
    redirect: '/auth/register',
  },
  {
    path: '/logout',
    redirect: '/auth/login',
  },

  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

let initPromise: Promise<void> | null = null

const ensureInitialized = (): Promise<void> => {
  if (!initPromise) {
    initPromise = useUserStore().fetchUserData()
  }
  return initPromise
}

router.beforeEach(async (to) => {
  await ensureInitialized()

  const authStore = useAuthStore()

  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const requiresGuest = to.matched.some((record) => record.meta.requiresGuest)

  if (requiresAuth && !authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }

  if (requiresGuest && authStore.isAuthenticated) {
    return { name: 'Dashboard' }
  }
})

export default router

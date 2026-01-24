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

  // Redirects
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

  // Catch all - redirect to dashboard
  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  // Sprawdź czy route wymaga autentykacji (dziedziczy z parent route)
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const requiresGuest = to.matched.some((record) => record.meta.requiresGuest)

  if (requiresAuth && !authStore.isAuthenticated) {
    // Użytkownik niezalogowany próbuje wejść na chronioną stronę
    next({ name: 'Login', query: { redirect: to.fullPath } })
  } else if (requiresGuest && authStore.isAuthenticated) {
    // Zalogowany użytkownik próbuje wejść na login/register
    next({ name: 'Dashboard' })
  } else {
    next()
  }
})

export default router

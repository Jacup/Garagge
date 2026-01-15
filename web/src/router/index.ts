import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

import DashboardView from '../views/home/DashboardView.vue'
import RegisterView from '../views/RegisterView.vue'
import LoginView from '../views/LoginView.vue'
import VehiclesView from '@/views/vehicles/VehiclesView.vue'
import ModifyVehicleView from '@/views/ModifyVehicleView.vue'
import VehicleView from '@/views/vehicles/VehicleView.vue'
import AddServiceRecordView from '@/views/vehicles/AddServiceRecordView.vue'
import SettingsView from '@/views/settings/SettingsView.vue'

const routes = [
  {
    path: '/',
    name: 'Dashboard',
    component: DashboardView,
  },
  {
    path: '/vehicles',
    name: 'Vehicles',
    component: VehiclesView,
    meta: { requiresAuth: true },
  },
  {
    path: '/vehicles/add',
    name: 'AddVehicle',
    component: ModifyVehicleView,
    meta: { requiresAuth: true },
  },
  {
    path: '/vehicles/edit/:id',
    name: 'EditVehicle',
    component: ModifyVehicleView,
    meta: { requiresAuth: true },
    props: true,
  },
  {
    path: '/vehicles/:id',
    name: 'VehicleView',
    component: VehicleView,
    meta: { requiresAuth: true },
  },
  {
    path: '/vehicles/:id/services/add',
    name: 'AddServiceRecord',
    component: AddServiceRecordView,
    meta: { requiresAuth: true },
    props: true,
  },
  {
    path: '/settings',
    name: 'Settings',
    component: SettingsView,
    meta: { requiresAuth: true },
  },
  {
    path: '/register',
    name: 'Register',
    component: RegisterView,
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
  },
  {
    path: '/logout',
    name: 'Logout',
    component: LoginView,
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else {
    next()
  }
})

export default router

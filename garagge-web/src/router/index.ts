import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/userStore'
import DashboardView from '../views/DashboardView.vue'
import RegisterView from '../views/RegisterView.vue'
import LoginView from '../views/LoginView.vue'
import VehiclesView from '@/views/VehiclesView.vue'
import AddVehicleView from '@/views/AddVehicleView.vue'

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
    component: AddVehicleView,
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
  const userStore = useUserStore()

  if (to.meta.requiresAuth && !userStore.accessToken) {
    next('/login')
  } else {
    next()
  }
})

export default router

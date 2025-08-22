import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/userStore'
import DashboardView from '../views/DashboardView.vue'
import RegisterView from '../views/RegisterView.vue'
import LoginView from '../views/LoginView.vue'
import VehiclesView from '@/views/VehiclesView.vue'
import ModifyVehicleView from '@/views/ModifyVehicleView.vue'
import ComponentGalleryView from '@/views/ComponentGalleryView.vue'

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
    path: '/components',
    name: 'ComponentGallery',
    component: ComponentGalleryView,
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

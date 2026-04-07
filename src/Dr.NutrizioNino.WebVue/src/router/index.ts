import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '@/modules/auth/composables/useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { public: true }
    },
    {
      path: '/',
      name: 'home',
      redirect: '/foods'
    },
    {
      path: '/brands',
      name: 'brands',
      component: () => import('../views/BrandsView.vue')
    },
    {
      path: '/foods',
      name: 'foods',
      component: () => import('../views/FoodsView.vue')
    },
    {
      path: '/dishes',
      name: 'dishes',
      component: () => import('../views/DishesView.vue')
    },
    {
      path: '/nutrients',
      name: 'nutrients',
      component: () => import('../views/NutrientsView.vue')
    },
    {
      path: '/units',
      name: 'units',
      component: () => import('../views/UnitsView.vue')
    },
    {
      path: '/supermarkets',
      name: 'supermarkets',
      component: () => import('../views/SupermarketsView.vue')
    },
    {
      path: '/categories',
      name: 'categories',
      component: () => import('../views/CategoriesView.vue')
    },
    {
      path: '/daily-simulations',
      name: 'daily-simulations',
      component: () => import('../views/DailySimulationsView.vue')
    },
    {
      path: '/profile',
      name: 'profile',
      component: () => import('../views/UserProfileView.vue')
    },
    {
      path: '/section-configs',
      name: 'section-configs',
      component: () => import('../views/SectionConfigsView.vue'),
      meta: { requiresAdmin: true }
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: () => import('../views/AdminUsersView.vue'),
      meta: { requiresAdmin: true }
    }
  ]
})

router.beforeEach(async (to) => {
  if (to.meta.public) return true

  const { isAuthenticated, isAdmin, checkAuth } = useAuth()
  await checkAuth()

  if (!isAuthenticated.value) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresAdmin && !isAdmin.value) {
    return { name: 'home' }
  }

  return true
})

export default router

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
    }
  ]
})

router.beforeEach(async (to) => {
  if (to.meta.public) return true

  const { isAuthenticated, checkAuth } = useAuth()
  await checkAuth()

  if (!isAuthenticated.value) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  return true
})

export default router

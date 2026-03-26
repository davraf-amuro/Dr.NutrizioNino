import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
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

export default router

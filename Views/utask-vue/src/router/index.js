import { createRouter, createWebHistory} from 'vue-router'



const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue')
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/Login.vue')
  },
  {
    path:'/register',
    name:'register',
    component: () => import('../views/Register.vue')
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('../views/Dashboard.vue'),
  },
  {
    path: '/profile',
    name: 'profile',
    component: () => import("../views/Profile.vue"),
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router

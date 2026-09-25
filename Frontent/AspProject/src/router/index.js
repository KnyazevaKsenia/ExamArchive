import Vue from 'vue'
import VueRouter from 'vue-router'
import MaterialsPage from '../components/MaterialsPage.vue'
import MaterialForm from '../components/MaterialForm.vue'
import MyPage from '../components/MyPage.vue'
import ExamImitating from '../components/ExamImitating.vue'
import FavoritePage from '../components/FavoritePage.vue'
import MaterialDetails from '../components/MaterialDetails.vue'
import { isAuthenticated } from '../utils/auth'

Vue.use(VueRouter)
const localhostPort = 44356
const routes = [
  {
    path: '/',
    redirect: '/materials'
  },
  {
    path: '/materials',
    name: 'materials',
    component: MaterialsPage
  },
  {
    path: '/materials/:id',
    name: 'material-details',
    component: MaterialDetails
  },
  {
    path: '/adding',
    name: 'adding',
    component: MaterialForm,
    meta: { requiresAuth: true }
  },
  {
    path: '/mypage',
    name: 'mypage',
    component: MyPage,
    meta: { requiresAuth: true }
  },
  {
    path: '/exam',
    name: 'exam',
    component: ExamImitating
  },
  {
    path: '/favorite',
    name: 'favorite',
    component: FavoritePage,
    meta: { requiresAuth: true }
  }
]

const router = new VueRouter({
  mode: 'history',
  base: import.meta.env.BASE_URL,
  routes
})

// Navigation guard to check authentication for protected routes
router.beforeEach((to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    // This route requires auth, check if logged in
    if (!isAuthenticated()) {
      // Not authenticated, redirect to the same page but with auth param
      next({ 
        path: to.path,
        query: { auth: 'required' }
      });
    } else {
      next(); // Authenticated, proceed
    }
  } else {
    next(); // Not a protected route, proceed
  }
});

export default router


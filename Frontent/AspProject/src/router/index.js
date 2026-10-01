import Vue from 'vue'
import VueRouter from 'vue-router'
import HomePage from '../components/HomePage.vue'
import MaterialsPage from '../components/MaterialsPage.vue'
import MaterialForm from '../components/MaterialForm.vue'
import MyPage from '../components/MyPage.vue'
import ExamImitating from '../components/ExamImitating.vue'
import FavoritePage from '../components/FavoritePage.vue'
import MaterialDetails from '../components/MaterialDetails.vue'
import AssistantPage from '../components/AssistantPage.vue'
import { isAuthenticated } from '../utils/auth'

Vue.use(VueRouter)

const routes = [
  { path: '/', name: 'home', component: HomePage },
  { path: '/materials', name: 'materials', component: MaterialsPage },
  { path: '/library', redirect: '/materials' },
  { path: '/materials/:id', name: 'material-details', component: MaterialDetails },
  { path: '/adding', name: 'adding', component: MaterialForm, meta: { requiresAuth: true } },
  { path: '/mypage', name: 'mypage', component: MyPage, meta: { requiresAuth: true } },
  { path: '/exam', name: 'exam', component: ExamImitating },
  { path: '/favorite', name: 'favorite', component: FavoritePage, meta: { requiresAuth: true } },
  { path: '/favorites', redirect: '/favorite' },
  { path: '/assistant', name: 'assistant', component: AssistantPage }
]

const router = new VueRouter({
  mode: 'history',
  base: import.meta.env.BASE_URL,
  routes,
  scrollBehavior() { return { x: 0, y: 0 } }
})

router.beforeEach((to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth) && !isAuthenticated()) {
    next({ path: '/', query: { auth: 'required', next: to.fullPath } })
    return
  }
  next()
})

export default router

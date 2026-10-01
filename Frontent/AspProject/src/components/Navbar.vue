<script>
import { defineComponent } from 'vue'
import axios from 'axios'
import { jwtDecode } from 'jwt-decode'
import { API_ENDPOINTS } from '@/config.js'

export default defineComponent({
  name: 'Navbar',
  data: () => ({
    mobileNav: false,
    isUserMenuOpen: false,
    showLoginModal: false,
    showRegisterModal: false,
    isLoggedIn: false,
    userName: '',
    userLastName: '',
    loginForm: { login: '', password: '' },
    registerForm: {
      firstName: '', lastName: '', university: '', institute: '',
      login: '', password: '', confirmPassword: ''
    },
    validationErrors: {}
  }),
  computed: {
    currentRoute() { return this.$route.path },
    displayName() {
      if (!this.isLoggedIn) return 'Аккаунт'
      return [this.userName, this.userLastName].filter(Boolean).join(' ') || 'Профиль'
    }
  },
  watch: {
    '$route.query.auth': {
      immediate: true,
      handler(value) {
        if (value === 'required' && !this.isLoggedIn) this.showLoginModal = true
      }
    },
    '$route.path'() {
      this.mobileNav = false
      this.isUserMenuOpen = false
    }
  },
  created() { this.readToken() },
  methods: {
    readToken() {
      const token = localStorage.getItem('AuthToken')
      this.isLoggedIn = Boolean(token)
      if (!token) return
      try {
        const decoded = jwtDecode(token)
        this.userName = decoded.FirstName || decoded.firstName || ''
        this.userLastName = decoded.LastName || decoded.lastName || ''
      } catch (error) {
        console.error('Token decode failed', error)
      }
    },
    navigate(path) {
      if (this.$route.path !== path) this.$router.push(path)
      this.mobileNav = false
    },
    openLoginModal() {
      this.showLoginModal = true
      this.isUserMenuOpen = false
    },
    closeLoginModal() {
      this.showLoginModal = false
      this.loginForm = { login: '', password: '' }
    },
    openRegisterModal() {
      this.showRegisterModal = true
      this.isUserMenuOpen = false
    },
    closeRegisterModal() {
      this.showRegisterModal = false
      this.validationErrors = {}
    },
    validateRegister() {
      const e = {}
      if (!this.registerForm.firstName) e.firstName = 'Введите имя'
      if (!this.registerForm.university) e.university = 'Введите университет'
      if (!this.registerForm.institute) e.institute = 'Введите институт'
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.registerForm.login)) e.login = 'Введите корректную почту'
      if ((this.registerForm.password || '').length < 8) e.password = 'Минимум 8 символов'
      if (this.registerForm.password !== this.registerForm.confirmPassword) e.confirmPassword = 'Пароли не совпадают'
      this.validationErrors = e
      return Object.keys(e).length === 0
    },
    async registerUser() {
      if (!this.validateRegister()) return
      try {
        await axios.post(API_ENDPOINTS.REGISTER, {
          Login: this.registerForm.login,
          Password: this.registerForm.password,
          FirstName: this.registerForm.firstName,
          LastName: this.registerForm.lastName,
          University: this.registerForm.university,
          Institute: this.registerForm.institute
        })
        const email = this.registerForm.login
        this.closeRegisterModal()
        this.loginForm.login = email
        this.openLoginModal()
        this.$notify.success('Регистрация завершена. Теперь войдите в аккаунт.')
      } catch (error) {
        this.$notify.error(error.response?.data || 'Не удалось зарегистрироваться')
      }
    },
    async loginUser() {
      if (!this.loginForm.login || !this.loginForm.password) {
        this.$notify.error('Заполните почту и пароль')
        return
      }
      try {
        const response = await axios.post(API_ENDPOINTS.LOGIN, {
          Login: this.loginForm.login,
          Password: this.loginForm.password
        })
        const token = response.data?.token || response.data?.Token
        if (!token) throw new Error('Token missing')
        localStorage.setItem('AuthToken', token)
        axios.defaults.headers.common.Authorization = `Bearer ${token}`
        this.readToken()
        this.closeLoginModal()
        this.$notify.success('Вы вошли в аккаунт')
        const next = this.$route.query.next
        if (next) this.$router.push(String(next))
      } catch (error) {
        this.$notify.error(error.response?.data || 'Не удалось войти')
      }
    },
    async logoutUser() {
      try { await axios.post(API_ENDPOINTS.LOGOUT, {}, { withCredentials: true }) } catch (_) {}
      localStorage.removeItem('AuthToken')
      delete axios.defaults.headers.common.Authorization
      this.isLoggedIn = false
      this.userName = ''
      this.userLastName = ''
      this.isUserMenuOpen = false
      this.$notify.success('Вы вышли из аккаунта')
      if (this.$route.meta.requiresAuth) this.$router.push('/')
    }
  }
})
</script>

<template>
  <div>
    <header class="site-header page-container">
      <button class="wordmark" type="button" @click="navigate('/')">examarchive<span>.</span></button>

      <nav class="main-nav" :class="{ 'nav-open': mobileNav }" aria-label="Главная навигация">
        <router-link to="/" exact>Главная</router-link>
        <router-link to="/materials">Библиотека</router-link>
        <router-link to="/exam">Подготовка к экзамену</router-link>
        <router-link to="/favorite">Избранное</router-link>
        <router-link to="/assistant">AI-ассистент</router-link>
      </nav>

      <div class="header-actions">
        <button class="search-button" type="button" @click="navigate('/materials')" aria-label="Поиск материалов">
          <span class="search-glyph">⌕</span><span class="search-label">Поиск...</span>
        </button>

        <button v-if="isLoggedIn" class="upload-button" type="button" @click="navigate('/adding')">
          <span>⇧</span><span>Загрузить материал</span>
        </button>

        <div class="profile-wrap">
          <button class="avatar-button" type="button" @click="isUserMenuOpen = !isUserMenuOpen" :aria-label="displayName">
            <img src="/assets/avatar.jpg" alt="" />
          </button>
          <div v-if="isUserMenuOpen" class="user-dropdown">
            <template v-if="isLoggedIn">
              <div class="dropdown-name">{{ displayName }}</div>
              <button @click="navigate('/mypage')">Моя страница</button>
              <button class="danger" @click="logoutUser">Выйти</button>
            </template>
            <template v-else>
              <button @click="openLoginModal">Войти</button>
              <button @click="openRegisterModal">Регистрация</button>
            </template>
          </div>
        </div>

        <button class="mobile-menu" type="button" @click="mobileNav = !mobileNav" aria-label="Меню">{{ mobileNav ? '×' : '☰' }}</button>
      </div>
    </header>

    <div v-if="showLoginModal" class="modal-backdrop" @click.self="closeLoginModal">
      <div class="modal-card">
        <button class="modal-close" @click="closeLoginModal">×</button>
        <span class="eyebrow">EXAMARCHIVE</span>
        <h2 class="serif modal-title">с возвращением.</h2>
        <p class="modal-subtitle">Войди, чтобы открыть избранное, профиль и загрузку материалов.</p>
        <form class="auth-form" @submit.prevent="loginUser">
          <label>Электронная почта<input v-model="loginForm.login" type="email" placeholder="name@example.com" /></label>
          <label>Пароль<input v-model="loginForm.password" type="password" placeholder="Введите пароль" /></label>
          <button class="pill-button pill-dark" type="submit">Войти →</button>
        </form>
        <button class="switch-auth" @click="closeLoginModal(); openRegisterModal()">Нет аккаунта? Зарегистрироваться</button>
      </div>
    </div>

    <div v-if="showRegisterModal" class="modal-backdrop" @click.self="closeRegisterModal">
      <div class="modal-card modal-card-wide">
        <button class="modal-close" @click="closeRegisterModal">×</button>
        <span class="eyebrow">НОВЫЙ АККАУНТ</span>
        <h2 class="serif modal-title">присоединяйся.</h2>
        <form class="auth-form two-columns" @submit.prevent="registerUser">
          <label>Имя<input v-model="registerForm.firstName" /><small>{{ validationErrors.firstName }}</small></label>
          <label>Фамилия<input v-model="registerForm.lastName" /></label>
          <label>Университет<input v-model="registerForm.university" placeholder="КФУ" /><small>{{ validationErrors.university }}</small></label>
          <label>Институт<input v-model="registerForm.institute" placeholder="ИТИС" /><small>{{ validationErrors.institute }}</small></label>
          <label class="full">Электронная почта<input v-model="registerForm.login" type="email" /><small>{{ validationErrors.login }}</small></label>
          <label>Пароль<input v-model="registerForm.password" type="password" /><small>{{ validationErrors.password }}</small></label>
          <label>Повторите пароль<input v-model="registerForm.confirmPassword" type="password" /><small>{{ validationErrors.confirmPassword }}</small></label>
          <div class="full auth-actions"><button class="pill-button pill-dark" type="submit">Создать аккаунт →</button></div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.site-header { height: 66px; display: flex; align-items: center; gap: 26px; position: relative; z-index: 40; }
.wordmark { border: 0; background: transparent; padding: 0; color: #111; font: 700 clamp(27px,3vw,39px)/1 Georgia,serif; letter-spacing: -.075em; white-space: nowrap; }
.wordmark span { color: #6d9f8d; }
.main-nav { display: flex; align-items: center; gap: 5px; flex: 1; min-width: 0; white-space: nowrap; }
.main-nav a { color: #252525; text-decoration: none; font-size: 11px; padding: 9px 11px; border-radius: 10px; transition: background .2s; }
.main-nav a:hover, .main-nav a.router-link-exact-active { background: var(--color-blue); }
.header-actions { display: flex; align-items: center; gap: 10px; }
.search-button { border: 1px solid #e2e2dc; background: #ffffffb8; height: 36px; width: 150px; padding: 0 12px; border-radius: 22px; display: flex; align-items: center; gap: 8px; color: #111; }
.search-label { color: #999; font-size: 10px; flex: 1; text-align: left; }
.search-glyph { font-size: 18px; }
.upload-button { border: 0; background: var(--color-green); height: 36px; padding: 0 15px; border-radius: 20px; display: flex; align-items: center; gap: 7px; font-weight: 700; font-size: 10px; }
.avatar-button { width: 35px; height: 35px; padding: 0; border: 0; border-radius: 50%; overflow: hidden; }
.avatar-button img { width: 100%; height: 100%; object-fit: cover; }
.profile-wrap { position: relative; }
.user-dropdown { position: absolute; top: calc(100% + 9px); right: 0; width: 205px; padding: 7px; border: 1px solid var(--color-border); background: var(--color-surface); border-radius: 12px; box-shadow: var(--shadow-card); }
.user-dropdown button, .dropdown-name { width: 100%; text-align: left; padding: 10px; border: 0; border-radius: 8px; background: transparent; font-size: 12px; }
.dropdown-name { font-weight: 800; border-bottom: 1px solid var(--color-border); border-radius: 0; margin-bottom: 4px; }
.user-dropdown button:hover { background: var(--color-blue); }
.user-dropdown .danger:hover { background: #ffe7e5; }
.mobile-menu { display: none; border: 0; background: transparent; font-size: 22px; }
.modal-backdrop { position: fixed; inset: 0; z-index: 100; background: #161b1c7a; display: grid; place-items: center; padding: 20px; backdrop-filter: blur(4px); }
.modal-card { width: min(100%, 510px); max-height: 88vh; overflow: auto; position: relative; background: var(--color-bg); border-radius: 17px; padding: 30px; box-shadow: 0 24px 90px #0003; }
.modal-card-wide { width: min(100%, 680px); }
.modal-close { position: absolute; top: 18px; right: 18px; width: 33px; height: 33px; border: 1px solid var(--color-border); border-radius: 50%; background: white; font-size: 20px; }
.modal-title { font-size: 48px; line-height: 1.05; margin: 8px 35px 5px 0; }
.modal-subtitle { color: #666; font-size: 13px; margin: 0 0 22px; }
.auth-form { display: grid; gap: 14px; }
.auth-form.two-columns { grid-template-columns: 1fr 1fr; }
.auth-form .full { grid-column: 1 / -1; }
.auth-form label { display: grid; gap: 6px; font-size: 11px; font-weight: 700; }
.auth-form input { height: 45px; border: 1px solid var(--color-border); border-radius: 11px; background: white; padding: 0 13px; outline: 0; }
.auth-form small { min-height: 13px; color: #aa4740; font-weight: 500; }
.auth-actions { display: flex; justify-content: flex-end; margin-top: 7px; }
.switch-auth { margin-top: 18px; border: 0; background: transparent; font-size: 11px; text-decoration: underline; }
@media (max-width: 950px) { .main-nav a { font-size: 9px; padding: 8px 6px; } .search-button { width: 37px; justify-content: center; } .search-label { display: none; } .upload-button span:last-child { display: none; } .upload-button { width: 36px; padding: 0; justify-content: center; } }
@media (max-width: 700px) { .site-header { justify-content: space-between; } .main-nav { display:none; position:absolute; top:58px; left:0; right:0; padding:12px; border:1px solid var(--color-border); border-radius:12px; background:var(--color-surface); box-shadow:var(--shadow-card); flex-direction:column; align-items:stretch; } .main-nav.nav-open{display:flex}.main-nav a{font-size:13px;padding:11px}.mobile-menu{display:block}.auth-form.two-columns{grid-template-columns:1fr}.auth-form .full{grid-column:auto}.wordmark{font-size:31px} }
</style>

<script>
import { defineComponent } from 'vue';
import axios from 'axios';
import { API_ENDPOINTS } from '@/config.js';
import { jwtDecode } from 'jwt-decode';
import ErrorHandler from './ErrorHandler.vue';
import SuccessHandler from './SuccessHandler.vue';

export default defineComponent({
  name: 'Navbar',
  components: {
    ErrorHandler,
    SuccessHandler
  },
  data: () => ({
    isUserMenuOpen: false,
    showRegisterModal: false,
    showLoginModal: false,
    isLoggedIn: false,
    registerForm: {
      firstName: '',
      lastName: '',
      university: '',
      institute: '',
      login: '',
      password: '',
      confirmPassword: ''
    },
    loginForm: {
      login: '',
      password: ''
    },
    validationErrors: {
      firstName: '',
      lastName: '',
      university: '',
      institute: '',
      login: '',
      password: '',
      confirmPassword: ''
    },
    userName: '',
    userLastName: ''
  }),
  created() {
    // Check for existing token to get user info
    const token = localStorage.getItem('AuthToken');
    if (token) {
      this.isLoggedIn = true;
      try {
        const decodedToken = jwtDecode(token);
        if (decodedToken.FirstName) {
          console.log(decodedToken);
          this.userName = decodedToken.FirstName;
          this.userLastName = decodedToken.LastName;
        }
      } catch (error) {
        console.error('Error decoding token for user info:', error);
      }
    }
  },
  computed: {
    currentRoute() {
      return this.$route.path;
    }
  },
  methods: {
    handleNavigation(route) {
      if (route === '/materials') {
        this.$router.push(route);
        return;
      }
      
      if (!this.isLoggedIn) {
        this.$notify.error('Для доступа к этой странице необходимо войти в аккаунт или зарегистрироваться');
        return;
      }
      
      this.$router.push(route);
    },
    toggleUserMenu() {
      this.isUserMenuOpen = !this.isUserMenuOpen;
    },
    openRegisterModal() {
      this.showRegisterModal = true;
      this.isUserMenuOpen = false;
    },
    closeRegisterModal() {
      this.showRegisterModal = false;
      this.registerForm = {
        firstName: '',
        lastName: '',
        university: '',
        institute: '',
        login: '',
        password: '',
        confirmPassword: ''
      };
      this.validationErrors = {
        firstName: '',
        lastName: '',
        university: '',
        institute: '',
        login: '',
        password: '',
        confirmPassword: ''
      };
    },
    openLoginModal() {
      this.showLoginModal = true;
      this.isUserMenuOpen = false;
    },
    closeLoginModal() {
      this.showLoginModal = false;
      this.loginForm = { login: '', password: '' };
    },
    validateEmail(email) {
      return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    },
    validatePassword(password) {
      return {
        hasUppercase: /[A-Z]/.test(password),
        hasLowercase: /[a-z]/.test(password),
        hasNumber: /[0-9]/.test(password),
        hasSpecial: /[^a-zA-Z0-9]/.test(password),
        isLongEnough: password.length >= 8
      };
    },
    validateName(name) {
      return /^[А-ЯЁA-Z][а-яёa-z-]+$/.test(name);
    },
    validateForm() {
      let isValid = true;
      this.validationErrors = {
        firstName: '',
        lastName: '',
        university: '',
        institute: '',
        login: '',
        password: '',
        confirmPassword: ''
      };
      
      if (!this.registerForm.login) {
        this.validationErrors.login = "Электронная почта обязательна";
        isValid = false;
      } else if (this.registerForm.login.length < 3 || this.registerForm.login.length > 50) {
        this.validationErrors.login = "Длина электронной почты должна быть от 3 до 50 символов";
        isValid = false;
      } else if (!this.validateEmail(this.registerForm.login)) {
        this.validationErrors.login = "Некорректный адрес электронной почты";
        isValid = false;
      }
      
      if (!this.registerForm.password) {
        this.validationErrors.password = "Пароль обязателен";
        isValid = false;
      } else {
        const pwdVal = this.validatePassword(this.registerForm.password);
        if (!pwdVal.isLongEnough) {
          this.validationErrors.password = "Пароль должен быть не менее 8 символов";
        } else if (!pwdVal.hasUppercase) {
          this.validationErrors.password = "Пароль должен содержать хотя бы одну заглавную букву";
        } else if (!pwdVal.hasLowercase) {
          this.validationErrors.password = "Пароль должен содержать хотя бы одну строчную букву";
        } else if (!pwdVal.hasNumber) {
          this.validationErrors.password = "Пароль должен содержать хотя бы одну цифру";
        } else if (!pwdVal.hasSpecial) {
          this.validationErrors.password = "Пароль должен содержать хотя бы один специальный символ";
        }
        if (this.validationErrors.password) isValid = false;
      }
      
      if (this.registerForm.password !== this.registerForm.confirmPassword) {
        this.validationErrors.confirmPassword = "Пароли не совпадают";
        isValid = false;
      }
      
      if (!this.registerForm.firstName) {
        this.validationErrors.firstName = "Имя обязательно";
        isValid = false;
      } else if (this.registerForm.firstName.length < 2 || this.registerForm.firstName.length > 50) {
        this.validationErrors.firstName = "Длина имени должна быть от 2 до 50 символов";
        isValid = false;
      } else if (!this.validateName(this.registerForm.firstName)) {
        this.validationErrors.firstName = "Имя должно начинаться с заглавной буквы и содержать только буквы и дефис";
        isValid = false;
      }
      
      if (this.registerForm.lastName) {
        if (this.registerForm.lastName.length < 2 || this.registerForm.lastName.length > 50) {
          this.validationErrors.lastName = "Длина фамилии должна быть от 2 до 50 символов";
          isValid = false;
        } else if (!this.validateName(this.registerForm.lastName)) {
          this.validationErrors.lastName = "Фамилия должна начинаться с заглавной буквы и содержать только буквы и дефис";
          isValid = false;
        }
      }
      
      if (!this.registerForm.university) {
        this.validationErrors.university = "Университет обязателен";
        isValid = false;
      }
      
      if (!this.registerForm.institute) {
        this.validationErrors.institute = "Институт обязателен";
        isValid = false;
      }
      
      return isValid;
    },
    async registerUser() {
      if (!this.validateForm()) return;
      
      try {
        const response = await axios.post(API_ENDPOINTS.REGISTER, {
          Login: this.registerForm.login,
          Password: this.registerForm.password,
          FirstName: this.registerForm.firstName,
          LastName: this.registerForm.lastName,
          University: this.registerForm.university,
          Institute: this.registerForm.institute
        });
        
        console.log('Registration response:', response.data);
        this.$notify.success(response.data);
        
        const userEmail = this.registerForm.login;
        this.closeRegisterModal();
        this.loginForm.login = userEmail;
        this.openLoginModal();
      } catch (error) {
        console.error('Registration error:', error);
        
        if (error.response) {
          const status = error.response.status;
          
          if (status === 400) {
            const validationErrors = error.response.data;
            let errorMessage = 'Ошибки валидации:\n';
            
            if (Array.isArray(validationErrors)) {
              validationErrors.forEach(err => {
                errorMessage += `- ${err.propertyName}: ${err.errorMessage}\n`;
              });
            } else {
              errorMessage = 'Ошибка валидации данных';
            }
            
            this.$notify.error(errorMessage);
          } else if (status === 409) {
            this.$notify.error(error.response.data || 'Пользователь с такой почтой уже существует');
          } else if (status >= 500) {
            this.$notify.error(error.response.data || 'Ошибка при регистрации пользователя. Попробуйте позже.');
          } else {
            this.$notify.error('Ошибка при регистрации: ' + (error.response.data || error.message));
          }
        } else {
          this.$notify.error('Ошибка соединения с сервером. Пожалуйста, проверьте подключение к интернету.');
        }
      }
    },
    logoutUser() {
      // Send logout request to the server
      axios.post(API_ENDPOINTS.LOGOUT, {}, {
        withCredentials: true
      })
      .then(response => {
        console.log('Logout successful:', response.data);
      })
      .catch(error => {
        console.error('Logout error:', error);
      })
      .finally(() => {
        // Clear the token from localStorage
        localStorage.removeItem('AuthToken');
        
        // Clear cookies related to authentication
        document.cookie.split(";").forEach(function(c) {
          document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
        });
        
        // Reset user data
        this.userName = '';
        this.userLastName = '';
        this.isLoggedIn = false;
        this.isUserMenuOpen = false;
        
        // Inform the user
        this.$notify.success('Вы успешно вышли из системы');
        
        // Redirect to materials page instead of reloading
        this.$router.push('/materials');
      });
    },
    loginUser() {
      if (!this.loginForm.login || !this.loginForm.password) {
        this.$notify.error('Пожалуйста, заполните все поля');
        return;
      }
      
      axios.post(API_ENDPOINTS.LOGIN, {
        Login: this.loginForm.login,
        Password: this.loginForm.password
      })
      .then(response => {
        console.log(response.data);
        if (response.data && response.data.token) {
          localStorage.setItem("AuthToken", response.data.token);
          console.log('Token stored:', response.data.token);
          
          try {
            const decodedToken = jwtDecode(response.data.token);
            console.log(decodedToken);
            if (decodedToken.FirstName) {
              console.log(decodedToken.FirstName);
              this.userName = decodedToken.FirstName;
              this.userLastName = decodedToken.LastName;
            }
          } catch (error) {
            console.error('Error decoding token for user info:', error);
          }
          
          this.isLoggedIn = true;
          this.closeLoginModal();
          console.log(document.cookie);
          // The token refresh will be handled automatically by the global interceptor
          this.$notify.success('Вы успешно вошли в систему');
          // Redirect to materials page instead of reloading
          this.$router.push('/materials');
        }
      })
      .catch(error => {
        console.error('Login error:', error);
        
        if (error.response) {
          const status = error.response.status;
          
          if (status === 400) {
            const validationErrors = error.response.data;
            let errorMessage = 'Ошибки валидации:\n';
            
            if (Array.isArray(validationErrors)) {
              validationErrors.forEach(err => {
                errorMessage += `- ${err.propertyName}: ${err.errorMessage}\n`;
              });
            } else {
              errorMessage = error.response.data || 'Ошибка валидации данных';
            }
            
            this.$notify.error(errorMessage);
          } else if (status === 401) {
            this.$notify.error('Неверный пароль');
          } else if (status === 404) {
            this.$notify.error(error.response.data || 'Пользователь не найден');
          } else if (status >= 500) {
            this.$notify.error(error.response.data || 'Ошибка сервера. Попробуйте позже.');
          } else {
            this.$notify.error('Ошибка при входе: ' + (error.response.data || error.message));
          }
        } else {
          this.$notify.error('Ошибка соединения с сервером. Пожалуйста, проверьте подключение к интернету.');
        }
      });
    }
  }
});
</script>

<template>
  <div>
    <ErrorHandler ref="errorHandler" />
    <SuccessHandler ref="successHandler" />
    <!-- Navigation Bar with Frosted Glass Effect -->
    <div class="navbar">
      <div class="navbar-content">
        <div class="logo">
          <img src="/ChatGPT Image May 4, 2025, 02_01_20 AM — копия.png" alt="Архив Экзаменов Логотип" />
        </div>
        <nav>
          <a @click="handleNavigation('/materials')" class="nav-link" :class="{ active: currentRoute === '/materials' }">Материалы</a>
          <a @click="handleNavigation('/adding')" class="nav-link" :class="{ active: currentRoute === '/adding' }">Добавить Материал</a>
          <a @click="handleNavigation('/mypage')" class="nav-link" :class="{ active: currentRoute === '/mypage' }">Моя Страница</a>
          <a @click="handleNavigation('/exam')" class="nav-link" :class="{ active: currentRoute === '/exam' }">Имитация Экзамена</a>
          <a @click="handleNavigation('/favorite')" class="nav-link" :class="{ active: currentRoute === '/favorite' }">Избранное</a>
        </nav>
        <div class="user-section">
          <span class="user-name">{{userName}} {{ userLastName }} </span>
          <div class="avatar" @click="toggleUserMenu">👤</div>
          
          <!-- User dropdown menu -->
          <div v-if="isUserMenuOpen" class="user-dropdown">
            <template v-if="isLoggedIn">
              <div class="user-dropdown-item" @click="logoutUser">Выйти</div>
            </template>
            <template v-else>
              <div class="user-dropdown-item" @click="openRegisterModal">Зарегистрироваться</div>
              <div class="user-dropdown-item" @click="openLoginModal">Войти</div>
            </template>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Add a spacer to prevent content from being hidden under the navbar -->
    <div class="navbar-spacer"></div>
    
    <!-- Registration Modal -->
    <div v-if="showRegisterModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Регистрация</h2>
          <button class="close-btn" @click="closeRegisterModal">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="registerUser">
            <div class="form-group">
              <label for="firstName">Имя</label>
              <input 
                type="text" 
                id="firstName" 
                v-model="registerForm.firstName" 
                placeholder="Введите имя"
                :class="{ 'input-error': validationErrors.firstName }"
                required
              />
              <div v-if="validationErrors.firstName" class="error-message">{{ validationErrors.firstName }}</div>
            </div>
            <div class="form-group">
              <label for="lastName">Фамилия</label>
              <input 
                type="text" 
                id="lastName" 
                v-model="registerForm.lastName" 
                placeholder="Введите фамилию"
                :class="{ 'input-error': validationErrors.lastName }"
                required
              />
              <div v-if="validationErrors.lastName" class="error-message">{{ validationErrors.lastName }}</div>
            </div>
            <div class="form-group">
              <label for="university">Университет</label>
              <input 
                type="text" 
                id="university" 
                v-model="registerForm.university" 
                placeholder="Введите университет"
                :class="{ 'input-error': validationErrors.university }"
                required
              />
              <div v-if="validationErrors.university" class="error-message">{{ validationErrors.university }}</div>
            </div>
            <div class="form-group">
              <label for="institute">Институт</label>
              <input 
                type="text" 
                id="institute" 
                v-model="registerForm.institute" 
                placeholder="Введите институт"
                :class="{ 'input-error': validationErrors.institute }"
                required
              />
              <div v-if="validationErrors.institute" class="error-message">{{ validationErrors.institute }}</div>
            </div>
            <div class="form-group">
              <label for="login">Электронная почта</label>
              <input 
                type="email" 
                id="login" 
                v-model="registerForm.login" 
                placeholder="Введите электронную почту"
                :class="{ 'input-error': validationErrors.login }"
                required
              />
              <div v-if="validationErrors.login" class="error-message">{{ validationErrors.login }}</div>
            </div>
            <div class="form-group">
              <label for="password">Пароль</label>
              <input 
                type="password" 
                id="password" 
                v-model="registerForm.password" 
                placeholder="Введите пароль"
                :class="{ 'input-error': validationErrors.password }"
                required
              />
              <div v-if="validationErrors.password" class="error-message">{{ validationErrors.password }}</div>
            </div>
            <div class="form-group">
              <label for="confirmPassword">Подтвердите пароль</label>
              <input 
                type="password" 
                id="confirmPassword" 
                v-model="registerForm.confirmPassword" 
                placeholder="Подтвердите пароль"
                :class="{ 'input-error': validationErrors.confirmPassword }"
                required
              />
              <div v-if="validationErrors.confirmPassword" class="error-message">{{ validationErrors.confirmPassword }}</div>
            </div>
            <div class="form-actions">
              <button type="submit" class="btn-primary">Зарегистрироваться</button>
              <button type="button" class="btn-secondary" @click="closeRegisterModal">Отмена</button>
            </div>
          </form>
        </div>
      </div>
    </div>
    
    <!-- Login Modal -->
    <div v-if="showLoginModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Вход</h2>
          <button class="close-btn" @click="closeLoginModal">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="loginUser">
            <div class="form-group">
              <label for="loginUsername">Логин</label>
              <input 
                type="text" 
                id="loginUsername" 
                v-model="loginForm.login" 
                placeholder="Введите логин"
                required
              />
            </div>
            <div class="form-group">
              <label for="loginPassword">Пароль</label>
              <input 
                type="password" 
                id="loginPassword" 
                v-model="loginForm.password" 
                placeholder="Введите пароль"
                required
              />
            </div>
            <div class="form-actions">
              <button type="submit" class="btn-primary">Войти</button>
              <button type="button" class="btn-secondary" @click="closeLoginModal">Отмена</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Navigation Bar Styles */
.navbar {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  border-bottom: 1px solid rgba(203, 213, 224, 0.5);
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.navbar-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 2rem;
  height: 70px;
  max-width: 1400px;
  margin: 0 auto;
}
  
.logo {
  font-weight: bold;
  font-size: 1.5rem;
  color: #3182ce;
  display: flex;
  align-items: center;
  margin-left: -15px;
}

.logo img {
  height: 50px;
  width: auto;
  object-fit: contain;
}

nav {
  display: flex;
  gap: 1.5rem;
}

.nav-link {
  text-decoration: none;
  color: #4a5568;
  font-weight: 500;
  padding: 0.5rem 0;
  position: relative;
  transition: color 0.3s;
}

.nav-link:hover,
.nav-link.active {
  color: #3182ce;
  cursor: pointer;
}

.nav-link.active::after {
  content: '';
  position: absolute;
  bottom: -5px;
  left: 0;
  width: 100%;
  height: 3px;
  background-color: #3182ce;
  border-radius: 2px;
}

.user-section {
  display: flex;
  align-items: center;
  gap: 1rem;
  position: relative;
}

.user-name {
  font-weight: 500;
  color: #4a5568;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-color: #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

.avatar:hover {
  background-color: #cbd5e0;
}

.navbar-spacer {
  height: 70px;
}

/* User Dropdown Menu */
.user-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  padding: 8px 0;
  margin-top: 8px;
  min-width: 180px;
  z-index: 1001;
}

.user-dropdown-item {
  padding: 10px 16px;
  cursor: pointer;
  color: #4a5568;
  transition: background-color 0.3s;
}

.user-dropdown-item:hover {
  background-color: #f7fafc;
  color: #3182ce;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h2 {
  margin: 0;
  color: #2d3748;
  font-size: 20px;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  color: #a0aec0;
  cursor: pointer;
  transition: color 0.3s;
}

.close-btn:hover {
  color: #718096;
}

.modal-body {
  padding: 24px;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  color: #4a5568;
}

.form-group input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 16px;
  transition: border-color 0.3s, box-shadow 0.3s;
}

.form-group input:focus {
  outline: none;
  border-color: #4299e1;
  box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.2);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.btn-primary {
  background-color: #3182ce;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 10px 16px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-primary:hover {
  background-color: #2b6cb0;
}

.btn-secondary {
  background-color: #e2e8f0;
  color: #4a5568;
  border: none;
  border-radius: 6px;
  padding: 10px 16px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-secondary:hover {
  background-color: #cbd5e0;
}

/* Form Validation Styles */
.input-error {
  border-color: #e53e3e !important;
  box-shadow: 0 0 0 1px #e53e3e !important;
}

.error-message {
  color: #e53e3e;
  font-size: 0.875rem;
  margin-top: 4px;
}

/* Reset validation errors when closing modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
}
</style> 
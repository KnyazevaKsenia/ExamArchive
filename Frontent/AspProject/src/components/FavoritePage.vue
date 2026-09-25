<script>
import axios from 'axios';
import { API_ENDPOINTS } from '../config';
import { isAuthenticated } from '../utils/auth';
import Material from './Material.vue';

export default {
  name: 'FavoritePage',
  components: {
    Material
  },
  data() {
    return {
      favorites: [],
      isAuthenticated: false,
      loading: false,
      error: null
    };
  },
  mounted() {
    this.checkAuthentication();
    if (this.isAuthenticated) {
      this.fetchFavorites();
    }
    else{

    }
  },
  methods: {
    checkAuthentication() {
      this.isAuthenticated = isAuthenticated();
    },
    async fetchFavorites() {
      this.loading = true;
      try {
        const response = await axios.get(API_ENDPOINTS.GET_FAVORITES);
        if (response.data) {
          this.favorites = response.data.map(material => ({
            id: material.id || material.Id || material.materialId || material.MaterialId,
            description: material.description || material.Description,
            course: material.course || material.Course,
            subject: material.subject || material.Subject,
            teacherName: material.teacher || material.Teacher || material.teacherName || material.TeacherName,
            semester: material.semester || material.Semester,
            date: material.dateAdded || material.createdAt || material.CreatedAt || material.addedDate,
            imagesNameUrl: material.imagesNameUrl || material.ImagesNameUrl || {},
            fileNameUrl: material.fileNameUrl || material.FileNameUrl || {}
          }));
        }
      } catch (error) {
        console.error('Ошибка при загрузке избранного:', error);
        this.error = 'Не удалось загрузить список избранных материалов';
      } finally {
        this.loading = false;
      }
    },
    
    async removeFavorite(id) {
      try {
        const response = await axios.get(`${API_ENDPOINTS.REMOVE_FAVORITE}/${id}`);
        if (response.status === 200) {
          this.favorites = this.favorites.filter(fav => fav.id !== id);
          alert('Материал удален из избранного');
        }
      } catch (error) {
        console.error('Ошибка при удалении из избранного:', error);
        alert('Не удалось удалить материал из избранного');
      }
    },
    viewMaterialDetails(materialId) {
      this.$router.push(`/materials/${materialId}`);
    },
    goToLogin() {
      this.$router.push('/login');
    }
  }
};
</script>

<template>
  <div class="favorite-page">
    <div class="tittle">
    <h2>Избранное</h2>

    </div>
    
    <!-- Authentication Required Message -->
    <div v-if="!isAuthenticated" class="auth-required">
      <h3>Требуется авторизация</h3>
      <p>Для доступа к избранным материалам необходимо авторизоваться.</p>
      <button @click="goToLogin" class="btn btn-primary">Войти</button>
    </div>
    
    <div v-else>
      <div v-if="loading" class="loading">
        Загрузка избранного...
      </div>
      
      <div v-else-if="error" class="error-message">
        {{ error }}
      </div>
      
      <div v-else-if="favorites.length === 0" class="no-favorites">
        <p>У вас пока нет избранных материалов.</p>
        <p>Просматривайте материалы и добавляйте их в избранное, нажав на значок сердечка.</p>
      </div>
      
      <div v-else class="favorites-list">
        <div class="material-grid">
          <div v-for="favorite in favorites" :key="favorite.id" class="material-container">
            <Material :material="favorite" @view-details="viewMaterialDetails" />
            <div class="material-actions">
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.tittle{
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
.favorite-page {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.auth-required {
  background-color: #fff8e1;
  border: 1px solid #ffe082;
  border-radius: 6px;
  padding: 30px;
  text-align: center;
  margin: 40px auto;
  max-width: 500px;
}

.auth-required h3 {
  color: #f57c00;
  margin-bottom: 15px;
}

.auth-required p {
  margin-bottom: 20px;
  color: #5d4037;
}

.loading, .error-message {
  padding: 20px;
  text-align: center;
  background-color: #f7fafc;
  border-radius: 6px;
  margin: 10px 0;
}

.loading {
  color: #4a5568;
}

.error-message {
  color: #e53e3e;
  background-color: #fff5f5;
  border: 1px solid #fed7d7;
}

.no-favorites {
  padding: 40px;
  text-align: center;
  background-color: #f7fafc;
  border-radius: 6px;
  color: #718096;
  margin-top: 20px;
}

.no-favorites p {
  margin-bottom: 10px;
}

.material-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.material-container {
  display: flex;
  flex-direction: column;
}

.material-actions {
  margin-top: 10px;
  display: flex;
  justify-content: center;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
  transition: background-color 0.3s;
}

.btn-primary {
  background-color: #4299e1;
  color: white;
  text-decoration: none;
}

.btn-primary:hover {
  background-color: #3182ce;
}

.btn-remove {
  background-color: #fed7d7;
  color: #e53e3e;
  display: flex;
  align-items: center;
  gap: 5px;
  justify-content: center;
  width: 100%;
}

.btn-remove:hover {
  background-color: #feb2b2;
}

.heart-icon {
  font-size: 16px;
}
</style> 
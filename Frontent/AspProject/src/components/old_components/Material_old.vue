<script>
import axios from 'axios';
import { API_ENDPOINTS } from '../config';
import { jwtDecode } from 'jwt-decode';
import { isAuthenticated } from '../utils/auth';

export default {
  name: 'Material',
  props: {
    material: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      isLiked: false,
      isLiking: false,
      studentId: null,
      isAuthenticated: false
    };
  },
  computed: {
    normalizedMaterial() {
      const m = this.material;
      return {
        id: m.id || m.Id || m.MaterialId,
        description: m.description || m.Description,
        course: m.course || m.Course,
        subject: m.subject || m.Subject,
        teacherName: m.teacherName || m.teacher || m.TeacherName || m.Teacher,
        semester: m.semester || m.Semester,
        date: m.date || m.CreatedAt || m.createdAt,
        imagesNameUrl: m.imagesNameUrl || m.ImagesNameUrl || {},
        fileNameUrl: m.fileNameUrl || m.FileNameUrl || {}
      };
    },
    hasImages() {
      return this.normalizedMaterial.imagesNameUrl &&
             Object.keys(this.normalizedMaterial.imagesNameUrl).length > 0;
    },
    hasFiles() {
      return this.normalizedMaterial.fileNameUrl &&
             Object.keys(this.normalizedMaterial.fileNameUrl).length > 0;
    },
    imagesCount() {
      return this.normalizedMaterial.imagesNameUrl ? 
        Object.keys(this.normalizedMaterial.imagesNameUrl).length : 0;
    },
    filesCount() {
      return this.normalizedMaterial.fileNameUrl ? 
        Object.keys(this.normalizedMaterial.fileNameUrl).length : 0;
    },
    truncatedDescription() {
      const description = this.normalizedMaterial.description;
      if (!description) return '';
      return description.length > 100 ? description.substring(0, 100) + '...' : description;
    },
    formattedDate() {
      if (!this.normalizedMaterial.date) return '';
      return new Date(this.normalizedMaterial.date).toLocaleDateString();
    }
  },
  mounted() {
    this.checkAuthentication();
    if (this.isAuthenticated) {
      this.getUserInfoFromToken();
      this.checkIfLiked();
    }
  },
  methods: {
    checkAuthentication() {
      this.isAuthenticated = isAuthenticated();
    },
    getUserInfoFromToken() {
      try {
        const token = localStorage.getItem('AuthToken');
        if (token) {
          const decodedToken = jwtDecode(token);
          this.studentId = decodedToken.StudentId || decodedToken.studentId;
        }
      } catch (error) {
        console.error('Error decoding token:', error);
      }
    },
    async checkIfLiked() {
      if (!this.isAuthenticated) return;
      
      try {
        const response = await axios.get(API_ENDPOINTS.GET_STUDENT_FAVORITES_IDS);
        if (response.data && Array.isArray(response.data)) {
          // Check if current material is in favorites by ID
          const materialId = this.normalizedMaterial.id;
          this.isLiked = response.data.some(id => id === materialId);
        }
      } catch (error) {
        console.error('Ошибка при проверке избранного:', error);
      }
    },
    downloadFile(url, fileName) {
      let downloadUrl = url;
      if (!/^https?:\/\//i.test(url)) {
        downloadUrl = `${import.meta.env.VITE_APP_API_URL || 'https://localhost:44356'}${url.startsWith('/') ? url : `/${url}`}`;
      }
      
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    },
    viewDetails() {
      this.$emit('view-details', this.normalizedMaterial.id);
    },
    async toggleLike() {
      if (!this.isAuthenticated) {
        alert('Для добавления материала в избранное необходимо авторизоваться');
        return;
      }

      if (this.isLiking) return;
      
      this.isLiking = true;
      
      try {
        if (!this.isLiked) {
          // Add to favorites
          const response = await axios.post(`${API_ENDPOINTS.ADD_FAVORITE}/${this.normalizedMaterial.id}`);
          if (response.status === 200) {
            this.isLiked = true;
          }
        } else {
          // Remove from favorites
          const response = await axios.get(`${API_ENDPOINTS.REMOVE_FAVORITE}/${this.normalizedMaterial.id}`);
          if (response.status === 200) {
            this.isLiked = false;
          }
        }
      } catch (error) {
        console.error('Ошибка при работе с избранным:', error);
      } finally {
        this.isLiking = false;
      }
    }
  }
}
</script>


<template>
    <div class="material-card">
      <h3>{{ normalizedMaterial.subject }}</h3>
  
      <div v-if="normalizedMaterial.description" class="description">
        <p>{{ truncatedDescription }}</p>
      </div>
      
      <div class="material-info">
        <p v-if="normalizedMaterial.teacherName"><strong>Преподаватель:</strong> {{ normalizedMaterial.teacherName }}</p>
        <p><strong>Курс:</strong> {{ normalizedMaterial.course }}</p>
        <p><strong>Семестр:</strong> {{ normalizedMaterial.semester }}</p>
        <p v-if="formattedDate"><strong>Дата:</strong> {{ formattedDate }}</p>
      </div>
      
      <div class="material-counts">
        <div v-if="imagesCount > 0" class="count-item">
          <div class="count-icon">🖼️</div>
          <div class="count-text">
            <span class="count-number">{{ imagesCount }}</span>
            <span class="count-label">{{ imagesCount === 1 ? 'Изображение' : imagesCount < 5 ? 'Изображения' : 'Изображений' }}</span>
          </div>
        </div>
        
        <div v-if="filesCount > 0" class="count-item">
          <div class="count-icon">📄</div>
          <div class="count-text">
            <span class="count-number">{{ filesCount }}</span>
            <span class="count-label">{{ filesCount === 1 ? 'Файл' : filesCount < 5 ? 'Файла' : 'Файлов' }}</span>
          </div>
        </div>
      </div>
      
      <div class="material-actions">
        <button class="btn btn-primary" @click="viewDetails">Просмотр деталей</button>
        <button 
          class="btn-favorite" 
          :class="{ 'liked': isLiked }" 
          @click="toggleLike"
          :disabled="isLiking"
          title="Добавить в избранное"
        >
          <span class="heart-icon">♥</span>
        </button>
      </div>
    </div>
  </template>
  

<style scoped>
.material-card {
  background-color: white;
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  transition: transform 0.2s, box-shadow 0.2s;
}

.material-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.material-card h3 {
  color: #2d3748;
  margin-bottom: 12px;
}

.description {
  margin-bottom: 16px;
  color: #4a5568;
  line-height: 1.5;
}

.material-info p {
  margin-bottom: 8px;
  color: #4a5568;
}

.material-counts {
  display: flex;
  gap: 20px;
  margin-top: 16px;
}

.count-item {
  display: flex;
  align-items: center;
  background-color: #f7fafc;
  padding: 8px 16px;
  border-radius: 6px;
}

.count-icon {
  font-size: 24px;
  margin-right: 10px;
}

.count-text {
  display: flex;
  flex-direction: column;
}

.count-number {
  font-size: 18px;
  font-weight: bold;
  color: #2d3748;
}

.count-label {
  font-size: 14px;
  color: #4a5568;
}

.material-actions {
  margin-top: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  font-weight: bold;
  transition: all 0.3s;
}

.btn-primary {
  background-color: #4299e1;
  color: white;
}

.btn-primary:hover {
  background-color: #3182ce;
}

.btn-favorite {
  background: none;
  border: none;
  font-size: 22px;
  cursor: pointer;
  transition: transform 0.2s, color 0.2s;
  color: #cbd5e0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.btn-favorite:hover {
  color: #fc8181;
  transform: scale(1.1);
}

.btn-favorite.liked {
  color: #fc8181;
}

.heart-icon {
  display: block;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style> 
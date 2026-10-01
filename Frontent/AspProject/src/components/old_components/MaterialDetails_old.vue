<script>
import { defineComponent } from 'vue';
import axios from 'axios';
import { API_ENDPOINTS, API_BASE_URL } from '../config';

export default defineComponent({
  name: 'MaterialDetails',
  data() {
    return {
      material: null,
      isLoading: true,
      error: null
    };
  },
  computed: {
    normalizedMaterial() {
      if (!this.material) return null;
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
      return this.normalizedMaterial?.imagesNameUrl && 
             Object.keys(this.normalizedMaterial.imagesNameUrl).length > 0;
    },
    hasFiles() {
      return this.normalizedMaterial?.fileNameUrl && 
             Object.keys(this.normalizedMaterial.fileNameUrl).length > 0;
    },
    formattedDate() {
      if (!this.normalizedMaterial?.date) return '';
      return new Date(this.normalizedMaterial.date).toLocaleDateString();
    }
  },
  created() {
    this.fetchMaterialDetails();
  },
  methods: {
    async fetchMaterialDetails() {
      this.isLoading = true;
      try {
        const materialId = this.$route.params.id;
        const response = await axios.get(`${API_ENDPOINTS.GET_MATERIAL_DETAILS}/${materialId}`);
        this.material = response.data;
      } catch (error) {
        console.error('Ошибка при загрузке деталей материала:', error);
        this.error = 'Не удалось загрузить детали материала. Пожалуйста, попробуйте позже.';
      } finally {
        this.isLoading = false;
      }
    },
    downloadFile(url, fileName) {
      // Проверяем, есть ли у URL схема
      let downloadUrl = url;
      if (!/^https?:\/\//i.test(url)) {
        // Если схема отсутствует, добавляем к базовому URL API
        downloadUrl = `${API_BASE_URL}${url.startsWith('/') ? url : `/${url}`}`;
      }
      
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    },
    goBack() {
      this.$router.go(-1);
    }
  }
});
</script>

<template>
  <div class="material-details">
    <button @click="goBack" class="btn btn-back">
      ← Назад к материалам
    </button>

    <div v-if="isLoading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Загрузка деталей материала...</p>
    </div>

    <div v-else-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-else-if="normalizedMaterial" class="material-content">
      <h2>{{ normalizedMaterial.subject }}</h2>
      
      <div class="material-info">
        <div class="info-item">
          <span class="label">Курс:</span>
          <span class="value">{{ normalizedMaterial.course }}</span>
        </div>
        <div class="info-item">
          <span class="label">Преподаватель:</span>
          <span class="value">{{ normalizedMaterial.teacherName }}</span>
        </div>
        <div class="info-item">
          <span class="label">Семестр:</span>
          <span class="value">{{ normalizedMaterial.semester }}</span>
        </div>
        <div class="info-item">
          <span class="label">Дата:</span>
          <span class="value">{{ formattedDate }}</span>
        </div>
      </div>

      <div v-if="normalizedMaterial.description" class="description">
        <h3>Описание</h3>
        <p>{{ normalizedMaterial.description }}</p>
      </div>

      <div v-if="hasImages" class="images-section">
        <h3>Изображения</h3>
        <div class="images-grid">
          <div 
            v-for="(url, name) in normalizedMaterial.imagesNameUrl" 
            :key="name" 
            class="image-container"
          >
            <img :src="url" :alt="name" @click="downloadFile(url, name)" />
            <div class="image-name">{{ name }}</div>
          </div>
        </div>
      </div>
        
      <div v-if="hasFiles" class="files-section">
        <h3>Файлы</h3>
        <ul class="files-list">
          <li v-for="(url, name) in normalizedMaterial.fileNameUrl" :key="name" class="file-item">
            <div class="file-icon">📄</div>
            <div class="file-info">
              <div class="file-name">{{ name }}</div>
              <button @click="downloadFile(url, name)" class="btn btn-download">
                Скачать
              </button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style scoped>
.material-details {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.btn-back {
  background-color: #e2e8f0;
  color: #4a5568;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  font-weight: bold;
  cursor: pointer;
  margin-bottom: 30px;
  display: inline-flex;
  align-items: center;
  transition: background-color 0.3s;
}

.btn-back:hover {
  background-color: #cbd5e0;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #3182ce;
  animation: spin 1s ease-in-out infinite;
  margin-bottom: 10px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-message {
  background-color: #fc8181;
  color: white;
  padding: 15px;
  border-radius: 6px;
  text-align: center;
}

.material-content {
  background-color: white;
  border-radius: 8px;
  padding: 30px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.material-content h2 {
  color: #2d3748;
  margin-bottom: 20px;
  font-size: 1.8rem;
}

.material-info {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 30px;
  background-color: #f7fafc;
  padding: 20px;
  border-radius: 6px;
}

.info-item {
  display: flex;
  flex-direction: column;
}

.label {
  font-weight: bold;
  color: #4a5568;
  margin-bottom: 5px;
  font-size: 14px;
}

.value {
  color: #2d3748;
  font-size: 16px;
}

h3 {
  color: #2d3748;
  margin-bottom: 15px;
  font-size: 1.3rem;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 10px;
}

.description {
  margin-bottom: 30px;
  line-height: 1.6;
}

.description p {
  color: #4a5568;
}

.images-section,
.files-section {
  margin-top: 30px;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 20px;
}

.image-container {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.image-container img {
  width: 100%;
  height: 200px;
  object-fit: cover;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.3s;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.image-container img:hover {
  transform: scale(1.03);
}

.image-name {
  margin-top: 8px;
  color: #4a5568;
  font-size: 14px;
  text-align: center;
}

.files-list {
  list-style: none;
  padding: 0;
}

.file-item {
  display: flex;
  align-items: center;
  padding: 15px;
  background-color: #f7fafc;
  border-radius: 6px;
  margin-bottom: 10px;
}

.file-icon {
  font-size: 24px;
  margin-right: 15px;
}

.file-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex: 1;
}

.file-name {
  font-weight: 500;
  color: #2d3748;
}

.btn-download {
  background-color: #4299e1;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-download:hover {
  background-color: #3182ce;
}
</style> 
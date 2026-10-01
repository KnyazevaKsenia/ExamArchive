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
      return this.normalizedMaterial.imagesNameUrl
        ? Object.keys(this.normalizedMaterial.imagesNameUrl).length
        : 0;
    },
    filesCount() {
      return this.normalizedMaterial.fileNameUrl
        ? Object.keys(this.normalizedMaterial.fileNameUrl).length
        : 0;
    },
    truncatedDescription() {
      const description = this.normalizedMaterial.description;
      if (!description) return '';
      return description.length > 120
        ? description.substring(0, 120) + '...'
        : description;
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
        downloadUrl =
          `${import.meta.env.VITE_APP_API_URL || 'https://localhost:44356'}` +
          `${url.startsWith('/') ? url : `/${url}`}`;
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
          const response = await axios.post(
            `${API_ENDPOINTS.ADD_FAVORITE}/${this.normalizedMaterial.id}`
          );

          if (response.status === 200) {
            this.isLiked = true;
          }
        } else {
          const response = await axios.get(
            `${API_ENDPOINTS.REMOVE_FAVORITE}/${this.normalizedMaterial.id}`
          );

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
  <article class="material-card">
    <div class="card-top">
      <div class="subject-block">
        <div class="subject-label">ПРЕДМЕТ</div>
        <h3>{{ normalizedMaterial.subject || 'Без названия' }}</h3>
      </div>

      <button
        class="favorite-button"
        :class="{ liked: isLiked }"
        :disabled="isLiking"
        :title="isLiked ? 'Убрать из избранного' : 'Добавить в избранное'"
        type="button"
        @click.stop="toggleLike"
      >
        <span aria-hidden="true">{{ isLiked ? '♥' : '♡' }}</span>
      </button>
    </div>

    <p v-if="normalizedMaterial.description" class="description">
      {{ truncatedDescription }}
    </p>

    <div class="meta-list">
      <div v-if="normalizedMaterial.teacherName" class="meta-row">
        <span class="meta-label">Преподаватель</span>
        <span class="meta-value">{{ normalizedMaterial.teacherName }}</span>
      </div>

      <div class="meta-row">
        <span class="meta-label">Курс</span>
        <span class="meta-value">{{ normalizedMaterial.course || '—' }}</span>
      </div>

      <div class="meta-row">
        <span class="meta-label">Семестр</span>
        <span class="meta-value">{{ normalizedMaterial.semester || '—' }}</span>
      </div>

      <div v-if="formattedDate" class="meta-row">
        <span class="meta-label">Добавлено</span>
        <span class="meta-value">{{ formattedDate }}</span>
      </div>
    </div>

    <div v-if="imagesCount > 0 || filesCount > 0" class="attachments">
      <span v-if="filesCount > 0" class="attachment-pill">
        <span class="attachment-icon">▤</span>
        {{ filesCount }}
        {{ filesCount === 1 ? 'файл' : filesCount < 5 ? 'файла' : 'файлов' }}
      </span>

      <span v-if="imagesCount > 0" class="attachment-pill attachment-pill--blue">
        <span class="attachment-icon">□</span>
        {{ imagesCount }}
        {{ imagesCount === 1 ? 'изображение' : imagesCount < 5 ? 'изображения' : 'изображений' }}
      </span>
    </div>

    <div class="card-actions">
      <button
        class="details-button"
        type="button"
        @click="viewDetails"
      >
        Открыть материал
        <span>→</span>
      </button>
    </div>
  </article>
</template>

<style scoped>
.material-card {
  min-height: 330px;
  display: flex;
  flex-direction: column;
  padding: 20px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: var(--shadow-card, 0 8px 30px rgba(24, 24, 20, 0.06));
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}

.material-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 16px 38px rgba(24, 24, 20, 0.1);
}

.card-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
}

.subject-block {
  min-width: 0;
}

.subject-label {
  margin-bottom: 7px;
  color: var(--color-text-secondary, #64645f);
  font-size: 9px;
  font-weight: 800;
  letter-spacing: 0.13em;
}

.material-card h3 {
  margin: 0;
  color: var(--color-text, #161616);
  font-size: 22px;
  line-height: 1.1;
  letter-spacing: -0.035em;
  overflow-wrap: anywhere;
}

.favorite-button {
  width: 39px;
  height: 39px;
  flex: 0 0 39px;
  display: grid;
  place-items: center;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 50%;
  background: var(--color-bg, #fbfaf6);
  color: #6f6f69;
  font-size: 22px;
  line-height: 1;
  cursor: pointer;
  transition:
    background-color 0.18s ease,
    transform 0.18s ease,
    color 0.18s ease;
}

.favorite-button:hover:not(:disabled) {
  transform: scale(1.05);
  background: var(--color-cream, #f4ead4);
}

.favorite-button.liked {
  background: #f4dfdf;
  color: #a64e4e;
}

.favorite-button:disabled {
  opacity: 0.55;
  cursor: wait;
}

.description {
  margin: 18px 0 20px;
  color: var(--color-text-secondary, #64645f);
  font-size: 13px;
  line-height: 1.55;
}

.meta-list {
  display: grid;
  gap: 0;
  border-top: 1px solid var(--color-border, #d9d8d2);
}

.meta-row {
  display: grid;
  grid-template-columns: minmax(90px, 0.8fr) 1.2fr;
  gap: 14px;
  padding: 10px 0;
  border-bottom: 1px solid var(--color-border, #d9d8d2);
  font-size: 12px;
}

.meta-label {
  color: var(--color-text-secondary, #64645f);
}

.meta-value {
  color: var(--color-text, #161616);
  font-weight: 700;
  text-align: right;
  overflow-wrap: anywhere;
}

.attachments {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 16px;
}

.attachment-pill {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  min-height: 32px;
  padding: 0 11px;
  border-radius: 999px;
  background: var(--color-green, #dce8c6);
  color: var(--color-text, #161616);
  font-size: 10px;
  font-weight: 800;
}

.attachment-pill--blue {
  background: var(--color-blue, #cbe8f2);
}

.attachment-icon {
  font-size: 14px;
}

.card-actions {
  margin-top: auto;
  padding-top: 20px;
}

.details-button {
  width: 100%;
  min-height: 43px;
  display: inline-flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 0 15px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 999px;
  background: var(--color-text, #161616);
  color: #fff;
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
  transition:
    background-color 0.18s ease,
    transform 0.18s ease;
}

.details-button:hover {
  background: #30302d;
}

.details-button span {
  font-size: 17px;
}

@media (max-width: 520px) {
  .material-card {
    min-height: auto;
  }

  .meta-row {
    grid-template-columns: 1fr;
    gap: 3px;
  }

  .meta-value {
    text-align: left;
  }
}
</style>

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
      this.error = null;

      try {
        const materialId = this.$route.params.id;
        const response = await axios.get(
          `${API_ENDPOINTS.GET_MATERIAL_DETAILS}/${materialId}`
        );

        this.material = response.data;
      } catch (error) {
        console.error('Ошибка при загрузке деталей материала:', error);
        this.error = 'Не удалось загрузить детали материала. Пожалуйста, попробуйте позже.';
      } finally {
        this.isLoading = false;
      }
    },

    downloadFile(url, fileName) {
      let downloadUrl = url;

      if (!/^https?:\/\//i.test(url)) {
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
  <div class="material-details-page grid-background">
    <section class="page-container details-shell">
      <button class="back-button" type="button" @click="goBack">
        <span>←</span>
        Назад к материалам
      </button>

      <div v-if="isLoading" class="state-card">
        <div class="state-icon">…</div>
        <div>
          <strong>Загружаем материал</strong>
          <p>Это займёт несколько секунд.</p>
        </div>
      </div>

      <div v-else-if="error" class="state-card state-card--error">
        <div class="state-icon">!</div>
        <div>
          <strong>{{ error }}</strong>
        </div>
      </div>

      <article v-else-if="normalizedMaterial" class="material-sheet">
        <div class="material-header">
          <div>
            <div class="section-kicker">МАТЕРИАЛ</div>
            <h1>{{ normalizedMaterial.subject || 'Без названия' }}</h1>
          </div>
        </div>

        <div class="meta-grid">
          <div class="meta-item">
            <span class="meta-label">Курс</span>
            <strong>{{ normalizedMaterial.course || '—' }}</strong>
          </div>

          <div class="meta-item">
            <span class="meta-label">Семестр</span>
            <strong>{{ normalizedMaterial.semester || '—' }}</strong>
          </div>

          <div v-if="normalizedMaterial.teacherName" class="meta-item">
            <span class="meta-label">Преподаватель</span>
            <strong>{{ normalizedMaterial.teacherName }}</strong>
          </div>

          <div v-if="formattedDate" class="meta-item">
            <span class="meta-label">Добавлено</span>
            <strong>{{ formattedDate }}</strong>
          </div>
        </div>

        <section v-if="normalizedMaterial.description" class="content-section">
          <div class="section-heading">
            <div>
              <div class="section-kicker">ОПИСАНИЕ</div>
              <h2>о материале</h2>
            </div>
          </div>

          <div class="description-card">
            <p>{{ normalizedMaterial.description }}</p>
          </div>
        </section>

        <section v-if="hasFiles" class="content-section">
          <div class="section-heading">
            <div>
              <div class="section-kicker">ФАЙЛЫ</div>
              <h2>скачать</h2>
            </div>
          </div>

          <div class="files-list">
            <div
              v-for="(url, name) in normalizedMaterial.fileNameUrl"
              :key="name"
              class="file-row"
            >
              <div class="file-left">
                <div class="file-icon">PDF</div>

                <div class="file-copy">
                  <strong>{{ name }}</strong>
                  <span>Файл материала</span>
                </div>
              </div>

              <button
                class="download-button"
                type="button"
                @click="downloadFile(url, name)"
              >
                Скачать
                <span>↓</span>
              </button>
            </div>
          </div>
        </section>

        <section v-if="hasImages" class="content-section">
          <div class="section-heading">
            <div>
              <div class="section-kicker">ИЗОБРАЖЕНИЯ</div>
              <h2>просмотр</h2>
            </div>
          </div>

          <div class="images-grid">
            <figure
              v-for="(url, name) in normalizedMaterial.imagesNameUrl"
              :key="name"
              class="image-card"
            >
              <button
                class="image-button"
                type="button"
                @click="downloadFile(url, name)"
              >
                <img :src="url" :alt="name" />
              </button>

              <figcaption>{{ name }}</figcaption>
            </figure>
          </div>
        </section>

        <section
          v-if="!normalizedMaterial.description && !hasFiles && !hasImages"
          class="content-section"
        >
          <div class="empty-card">
            <strong>В материале пока нет содержимого</strong>
            <p>
              Здесь появятся описание, файлы или изображения, когда они будут добавлены.
            </p>
          </div>
        </section>
      </article>
    </section>
  </div>
</template>

<style scoped>
.material-details-page {
  min-height: 100vh;
  padding-bottom: 90px;
  color: var(--color-text, #161616);
}

.details-shell {
  padding-top: 42px;
}

.back-button {
  min-height: 40px;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 22px;
  padding: 0 14px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 999px;
  background: rgba(255,255,255,.82);
  color: var(--color-text, #161616);
  font-size: 12px;
  font-weight: 800;
}

.back-button:hover {
  background: var(--color-cream, #f4ead4);
}

.material-sheet {
  overflow: hidden;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 20px;
  background: rgba(255,255,255,.94);
  box-shadow: var(--shadow-card, 0 8px 30px rgba(24,24,20,.06));
}

.material-header {
  padding: 42px 42px 34px;
  background:
    linear-gradient(
      135deg,
      rgba(203,232,242,.8),
      rgba(244,234,212,.55)
    );
  border-bottom: 1px solid var(--color-border-dark, #202020);
}

.section-kicker {
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
  font-weight: 800;
  letter-spacing: .14em;
}

.material-header h1 {
  max-width: 900px;
  margin: 10px 0 0;
  font-size: clamp(44px, 6vw, 86px);
  line-height: .92;
  letter-spacing: -.055em;
  overflow-wrap: anywhere;
}

.meta-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  border-bottom: 1px solid var(--color-border, #d9d8d2);
}

.meta-item {
  min-height: 112px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 15px;
  padding: 21px;
  border-right: 1px solid var(--color-border, #d9d8d2);
}

.meta-item:last-child {
  border-right: 0;
}

.meta-label {
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
  font-weight: 700;
  letter-spacing: .08em;
  text-transform: uppercase;
}

.meta-item strong {
  font-size: 16px;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.content-section {
  padding: 34px 42px 0;
}

.content-section:last-child {
  padding-bottom: 42px;
}

.section-heading {
  margin-bottom: 18px;
}

.section-heading h2 {
  margin: 5px 0 0;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 44px;
  font-weight: 400;
  line-height: .95;
  letter-spacing: -.04em;
}

.description-card {
  padding: 22px;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 14px;
  background: var(--color-bg, #fbfaf6);
}

.description-card p {
  margin: 0;
  color: #33332f;
  font-size: 15px;
  line-height: 1.7;
  white-space: pre-line;
}

.files-list {
  display: grid;
  gap: 10px;
}

.file-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  padding: 14px;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 14px;
  background: var(--color-bg, #fbfaf6);
}

.file-left {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 14px;
}

.file-icon {
  width: 48px;
  height: 48px;
  flex: 0 0 48px;
  display: grid;
  place-items: center;
  border-radius: 11px;
  background: var(--color-cream, #f4ead4);
  font-size: 10px;
  font-weight: 900;
}

.file-copy {
  min-width: 0;
}

.file-copy strong,
.file-copy span {
  display: block;
}

.file-copy strong {
  font-size: 13px;
  overflow-wrap: anywhere;
}

.file-copy span {
  margin-top: 4px;
  color: var(--color-text-secondary, #64645f);
  font-size: 11px;
}

.download-button {
  min-height: 39px;
  display: inline-flex;
  align-items: center;
  gap: 16px;
  padding: 0 14px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 999px;
  background: var(--color-text, #161616);
  color: #fff;
  font-size: 11px;
  font-weight: 800;
}

.download-button:hover {
  background: #30302d;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}

.image-card {
  margin: 0;
  overflow: hidden;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 14px;
  background: var(--color-bg, #fbfaf6);
}

.image-button {
  width: 100%;
  display: block;
  padding: 0;
  border: 0;
  background: transparent;
  cursor: pointer;
}

.image-card img {
  width: 100%;
  aspect-ratio: 4 / 3;
  display: block;
  object-fit: cover;
  transition: transform .2s ease;
}

.image-button:hover img {
  transform: scale(1.02);
}

.image-card figcaption {
  padding: 11px 13px;
  color: var(--color-text-secondary, #64645f);
  font-size: 11px;
  overflow-wrap: anywhere;
}

.empty-card,
.state-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 24px;
  border: 1px dashed var(--color-border-dark, #202020);
  border-radius: 15px;
  background: rgba(255,255,255,.72);
}

.empty-card {
  display: block;
}

.empty-card strong,
.state-card strong {
  display: block;
  font-size: 14px;
}

.empty-card p,
.state-card p {
  margin: 5px 0 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 12px;
  line-height: 1.5;
}

.state-icon {
  width: 48px;
  height: 48px;
  flex: 0 0 48px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  background: var(--color-blue, #cbe8f2);
  font-size: 20px;
  font-weight: 800;
}

.state-card--error .state-icon {
  background: #f4dfdf;
  color: #9b3939;
}

@media (max-width: 850px) {
  .meta-grid {
    grid-template-columns: 1fr 1fr;
  }

  .meta-item:nth-child(2n) {
    border-right: 0;
  }

  .content-section,
  .material-header {
    padding-left: 24px;
    padding-right: 24px;
  }
}

@media (max-width: 560px) {
  .details-shell {
    padding-top: 28px;
  }

  .material-header {
    padding-top: 30px;
    padding-bottom: 28px;
  }

  .material-header h1 {
    font-size: clamp(40px, 13vw, 62px);
  }

  .meta-grid {
    grid-template-columns: 1fr;
  }

  .meta-item {
    min-height: auto;
    border-right: 0;
  }

  .file-row {
    align-items: flex-start;
    flex-direction: column;
  }

  .download-button {
    width: 100%;
    justify-content: space-between;
  }

  .images-grid {
    grid-template-columns: 1fr;
  }
}
</style>

<script>
import axios from 'axios';
import { jwtDecode } from 'jwt-decode';
import { API_ENDPOINTS } from '../config';
import { isAuthenticated } from '../utils/auth';
import Material from './Material.vue';

export default {
  name: 'MyPage',

  components: {
    Material
  },

  data() {
    return {
      user: {
        FirstName: '',
        LastName: '',
        University: '',
        Institute: '',
        StudentId: ''
      },

      userMaterials: [],
      loading: false,
      error: null,
      isAuthenticated: false,

      showEditDialog: false,
      editingMaterial: null,

      editForm: {
        course: 1,
        subject: '',
        teacherName: '',
        semester: 1,
        description: '',
        files: []
      },

      courses: [1, 2, 3, 4],
      semesters: [1, 2],

      selectionData: {},
      availableSubjects: [],
      availableTeachers: [],

      isSubmitting: false,
      customSubject: false,
      customTeacher: false,
      fileInputKey: 0,
      previews: [],
      dragover: false,
      editMessage: ''
    };
  },

  computed: {
    fullName() {
      const value = `${this.user.FirstName || ''} ${this.user.LastName || ''}`.trim();
      return value || 'Студент';
    },

    initials() {
      const first = this.user.FirstName?.charAt(0) || '';
      const last = this.user.LastName?.charAt(0) || '';
      return `${first}${last}`.toUpperCase() || 'EA';
    },

    materialsCount() {
      return this.userMaterials.length;
    },

    subjectsCount() {
      return new Set(
        this.userMaterials
          .map(material => material.subject)
          .filter(Boolean)
      ).size;
    },

    filesCount() {
      return this.userMaterials.reduce((count, material) => {
        const files = material.fileNameUrl || {};
        const images = material.imagesNameUrl || {};
        return count + Object.keys(files).length + Object.keys(images).length;
      }, 0);
    }
  },

  mounted() {
    this.checkAuthentication();

    if (this.isAuthenticated) {
      this.getUserInfoFromToken();
      this.fetchUserMaterials();
      this.fetchSelectionData();
    }
  },

  watch: {
    'editForm.course'(newCourse) {
      this.editForm.subject = '';
      this.editForm.teacherName = '';
      this.customSubject = false;
      this.customTeacher = false;
      this.updateAvailableSubjects();
    },

    'editForm.subject'(newSubject) {
      if (newSubject === 'custom') {
        this.customSubject = true;
        this.editForm.subject = '';
      } else if (newSubject) {
        this.updateAvailableTeachers();
        this.editForm.teacherName = '';
        this.customTeacher = false;
      }
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

          this.user = {
            FirstName: decodedToken.FirstName || '',
            LastName: decodedToken.LastName || '',
            University: decodedToken.University || '',
            Institute: decodedToken.Institute || '',
            StudentId: decodedToken.StudentId || ''
          };
        }
      } catch (error) {
        console.error('Error decoding token:', error);
        this.error = 'Не удалось загрузить информацию профиля.';
      }
    },

    async fetchUserMaterials() {
      this.loading = true;
      this.error = null;

      try {
        const response = await axios.get(API_ENDPOINTS.PROFILE_MATERIALS);

        if (response.data && response.data.materials) {
          this.userMaterials = response.data.materials.map(material => ({
            id: material.id || material.Id,
            description: material.description || material.Description,
            course: material.course || material.Course,
            subject: material.subject || material.Subject,
            teacherName: material.teacher || material.Teacher,
            semester: material.semester || material.Semester,
            date: material.dateAdded || material.createdAt || material.CreatedAt,
            imagesNameUrl: material.imagesNameUrl || material.ImagesNameUrl || {},
            fileNameUrl: material.fileNameUrl || material.FileNameUrl || {}
          }));
        }
      } catch (error) {
        console.error('Error fetching user materials:', error);
        this.error = 'Не удалось загрузить ваши материалы.';
      } finally {
        this.loading = false;
      }
    },

    async fetchSelectionData() {
      try {
        const response = await axios.get(API_ENDPOINTS.SELECTION_LIST);
        this.selectionData = response.data;
      } catch (error) {
        console.error('Ошибка загрузки данных для выбора:', error);
      }
    },

    updateAvailableSubjects() {
      if (this.selectionData[this.editForm.course]) {
        const subjects = this.selectionData[this.editForm.course].subjects;
        this.availableSubjects = Object.keys(subjects || {});
      } else {
        this.availableSubjects = [];
      }
    },

    updateAvailableTeachers() {
      if (
        this.editForm.course &&
        this.editForm.subject &&
        this.selectionData[this.editForm.course] &&
        this.selectionData[this.editForm.course].subjects[this.editForm.subject]
      ) {
        this.availableTeachers =
          this.selectionData[this.editForm.course].subjects[this.editForm.subject].teachers || [];
      } else {
        this.availableTeachers = [];
      }
    },

    async deleteMaterial(materialId) {
      if (!confirm('Удалить этот материал? Это действие нельзя отменить.')) {
        return;
      }

      try {
        const response = await axios.delete(
          `${API_ENDPOINTS.DELETE_MATERIAL}/${materialId}`
        );

        if (response.status === 200) {
          this.userMaterials = this.userMaterials.filter(
            material => material.id !== materialId
          );

          if (this.$notify) {
            this.$notify.success('Материал удалён');
          }
        }
      } catch (error) {
        console.error('Ошибка при удалении материала:', error);

        if (this.$notify) {
          this.$notify.error(
            error.response?.status === 404
              ? 'Материал не найден'
              : 'Не удалось удалить материал'
          );
        }
      }
    },

    goToHome() {
      this.$router.push('/');
    },

    goToUpload() {
      this.$router.push('/adding');
    },

    goToLibrary() {
      this.$router.push('/materials');
    },

    viewMaterialDetails(materialId) {
      this.$router.push(`/materials/${materialId}`);
    },

    editMaterial(materialId) {
      this.editingMaterial = this.userMaterials.find(
        material => material.id === materialId
      );

      if (!this.editingMaterial) {
        return;
      }

      this.editForm = {
        course: this.editingMaterial.course || 1,
        subject: this.editingMaterial.subject || '',
        teacherName: this.editingMaterial.teacherName || '',
        semester: this.editingMaterial.semester || 1,
        description: this.editingMaterial.description || '',
        files: []
      };

      this.updateAvailableSubjects();
      this.updateAvailableTeachers();
      this.showEditDialog = true;
    },

    handleFileChange(event) {
      const newFiles = Array.from(event.target.files || []);
      this.editForm.files = [...this.editForm.files, ...newFiles];
      this.generatePreviews();
    },

    handleDrop(event) {
      event.preventDefault();
      this.dragover = false;

      if (event.dataTransfer.files) {
        const newFiles = Array.from(event.dataTransfer.files);
        this.editForm.files = [...this.editForm.files, ...newFiles];
        this.generatePreviews();
      }
    },

    handleDragOver(event) {
      event.preventDefault();
      this.dragover = true;
    },

    handleDragLeave() {
      this.dragover = false;
    },

    generatePreviews() {
      this.previews = [];

      this.editForm.files.forEach(file => {
        if (!file.type.startsWith('image/')) {
          return;
        }

        const reader = new FileReader();

        reader.onload = event => {
          this.previews.push({
            file,
            src: event.target.result
          });
        };

        reader.readAsDataURL(file);
      });
    },

    removeFile(index) {
      this.editForm.files.splice(index, 1);
      this.generatePreviews();
    },

    handleCustomTeacher() {
      if (this.editForm.teacherName === 'custom') {
        this.customTeacher = true;
        this.editForm.teacherName = '';
      }
    },

    closeEditDialog() {
      this.showEditDialog = false;
      this.editingMaterial = null;

      this.editForm = {
        course: 1,
        subject: '',
        teacherName: '',
        semester: 1,
        description: '',
        files: []
      };

      this.editMessage = '';
      this.previews = [];
      this.fileInputKey += 1;
      this.dragover = false;
    },

    async submitEditForm() {
      if (!this.editingMaterial) {
        return;
      }

      this.isSubmitting = true;
      this.editMessage = '';

      try {
        const formData = new FormData();

        formData.append('StudentId', this.user.StudentId);
        formData.append('Course', this.editForm.course);
        formData.append('Subject', this.editForm.subject);
        formData.append('TeacherName', this.editForm.teacherName);
        formData.append('Semester', this.editForm.semester);
        formData.append('Description', this.editForm.description);

        this.editForm.files.forEach(file => {
          formData.append('Files', file);
        });

        const response = await axios.post(
          `${API_ENDPOINTS.CHANGE_MATERIAL}/${this.editingMaterial.id}`,
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data'
            },
            timeout: 30000
          }
        );

        if (response.status === 200) {
          const index = this.userMaterials.findIndex(
            material => material.id === this.editingMaterial.id
          );

          if (index !== -1) {
            this.userMaterials.splice(index, 1, {
              ...this.userMaterials[index],
              course: this.editForm.course,
              subject: this.editForm.subject,
              teacherName: this.editForm.teacherName,
              semester: this.editForm.semester,
              description: this.editForm.description
            });
          }

          this.closeEditDialog();

          if (this.$notify) {
            this.$notify.success('Материал обновлён');
          }

          this.fetchUserMaterials();
        }
      } catch (error) {
        console.error('Ошибка при изменении материала:', error);

        if (error.response) {
          this.editMessage = `Ошибка сервера: ${error.response.status}`;
        } else if (error.request) {
          this.editMessage = 'Сервер не отвечает. Проверьте подключение.';
        } else {
          this.editMessage = 'Не удалось сохранить изменения.';
        }
      } finally {
        this.isSubmitting = false;
      }
    }
  }
};
</script>

<template>
  <main class="profile-page grid-background">
    <div class="page-container profile-container">
      <template v-if="!isAuthenticated">
        <section class="auth-state">
          <div class="auth-icon">↗</div>
          <div class="section-kicker">ЛИЧНЫЙ КАБИНЕТ</div>
          <h1>войдите,<br><span>чтобы продолжить.</span></h1>
          <p>
            В профиле находятся ваши материалы, данные образовательной программы
            и быстрый доступ к работе с архивом.
          </p>

          <button class="pill-button pill-dark" type="button" @click="goToHome">
            На главную
            <span>→</span>
          </button>
        </section>
      </template>

      <template v-else>
        <section class="profile-hero">
          <div class="profile-copy">
            <div class="section-kicker">ТВОЙ ПРОФИЛЬ</div>

            <h1>
              привет,
              <span>{{ user.FirstName || 'студент' }}.</span>
            </h1>

            <p>
              Здесь собраны твои данные, загруженные материалы
              и всё, чем ты уже поделился с архивом.
            </p>

            <div class="hero-actions">
              <button
                class="pill-button pill-dark"
                type="button"
                @click="goToUpload"
              >
                + Загрузить материал
              </button>

              <button
                class="pill-button"
                type="button"
                @click="goToLibrary"
              >
                Открыть библиотеку
                <span>→</span>
              </button>
            </div>
          </div>

          <article class="identity-card">
            <div class="identity-top">
              <div class="avatar-wrap">
                <img
                  src="/assets/avatar.jpg"
                  alt=""
                  class="profile-avatar"
                  @error="$event.target.style.display = 'none'"
                >
                <span class="avatar-fallback">{{ initials }}</span>
              </div>

              <div class="identity-copy">
                <span class="identity-eyebrow">СТУДЕНТ</span>
                <h2>{{ fullName }}</h2>
                <p>{{ user.University || 'Университет не указан' }}</p>
              </div>
            </div>

            <div class="identity-divider"></div>

            <div class="identity-meta">
              <div>
                <span>Институт</span>
                <strong>{{ user.Institute || '—' }}</strong>
              </div>

              <div>
                <span>Профиль</span>
                <strong>ExamArchive</strong>
              </div>
            </div>

            <div class="profile-note">
              <span>✳</span>
              материалы и предметы будут подбираться под твою программу
            </div>
          </article>
        </section>

        <div v-if="error" class="error-banner">
          <span>!</span>
          {{ error }}
        </div>

        <section class="stats-grid">
          <article class="stat-card stat-card--blue">
            <span class="stat-kicker">МАТЕРИАЛЫ</span>
            <strong>{{ materialsCount }}</strong>
            <p>загружено тобой</p>
          </article>

          <article class="stat-card stat-card--green">
            <span class="stat-kicker">ПРЕДМЕТЫ</span>
            <strong>{{ subjectsCount }}</strong>
            <p>в твоих материалах</p>
          </article>

          <article class="stat-card stat-card--cream">
            <span class="stat-kicker">ФАЙЛЫ</span>
            <strong>{{ filesCount }}</strong>
            <p>прикреплено всего</p>
          </article>

          <article class="stat-card stat-card--dark">
            <span class="stat-kicker">АРХИВ</span>
            <strong>∞</strong>
            <p>место для знаний</p>
          </article>
        </section>

        <section class="materials-section">
          <div class="section-heading">
            <div>
              <span class="section-kicker">МОИ МАТЕРИАЛЫ</span>
              <h2>то, чем ты<br>поделился.</h2>
            </div>

            <div class="section-side">
              <p>
                Редактируй описание и параметры материалов
                или удаляй то, что больше не актуально.
              </p>

              <button
                class="text-link"
                type="button"
                @click="goToUpload"
              >
                Добавить материал
                <span>→</span>
              </button>
            </div>
          </div>

          <div v-if="loading" class="state-card">
            <div class="state-symbol">…</div>
            <div>
              <strong>Загружаем материалы</strong>
              <p>Это займёт несколько секунд.</p>
            </div>
          </div>

          <div v-else-if="userMaterials.length === 0" class="empty-state">
            <div class="empty-art">
              <span>＋</span>
            </div>

            <div>
              <span class="section-kicker">ПОКА ПУСТО</span>
              <h3>добавь первый материал.</h3>
              <p>
                Загрузи конспект, методичку, презентацию или другой полезный файл.
              </p>

              <button
                class="pill-button pill-dark"
                type="button"
                @click="goToUpload"
              >
                Загрузить материал
                <span>→</span>
              </button>
            </div>
          </div>

          <div v-else class="material-grid">
            <article
              v-for="material in userMaterials"
              :key="material.id"
              class="owned-material"
            >
              <Material
                :material="material"
                @view-details="viewMaterialDetails"
              />

              <div class="owned-actions">
                <button
                  type="button"
                  class="material-action material-action--edit"
                  @click="editMaterial(material.id)"
                >
                  Изменить
                </button>

                <button
                  type="button"
                  class="material-action material-action--delete"
                  @click="deleteMaterial(material.id)"
                >
                  Удалить
                </button>
              </div>
            </article>
          </div>
        </section>
      </template>
    </div>

    <div
      v-if="showEditDialog"
      class="edit-overlay"
      @mousedown.self="closeEditDialog"
    >
      <section
        class="edit-modal"
        role="dialog"
        aria-modal="true"
        aria-label="Редактирование материала"
      >
        <header class="edit-header">
          <div>
            <span class="section-kicker">РЕДАКТИРОВАНИЕ</span>
            <h2>обновить материал.</h2>
          </div>

          <button
            class="close-button"
            type="button"
            aria-label="Закрыть"
            @click="closeEditDialog"
          >
            ×
          </button>
        </header>

        <div class="edit-body">
          <div v-if="editMessage" class="edit-message">
            {{ editMessage }}
          </div>

          <div class="form-grid">
            <div class="field">
              <label for="edit-course">Курс</label>
              <select id="edit-course" v-model="editForm.course">
                <option
                  v-for="course in courses"
                  :key="course"
                  :value="course"
                >
                  {{ course }} курс
                </option>
              </select>
            </div>

            <div class="field">
              <label for="edit-semester">Семестр</label>
              <select id="edit-semester" v-model="editForm.semester">
                <option
                  v-for="semester in semesters"
                  :key="semester"
                  :value="semester"
                >
                  {{ semester }} семестр
                </option>
              </select>
            </div>
          </div>

          <div class="field">
            <div class="field-heading">
              <label for="edit-subject">Предмет</label>
              <button
                type="button"
                class="field-switch"
                @click="customSubject = !customSubject"
              >
                {{ customSubject ? 'Выбрать из списка' : 'Ввести вручную' }}
              </button>
            </div>

            <input
              v-if="customSubject"
              id="edit-subject"
              v-model="editForm.subject"
              type="text"
              placeholder="Название предмета"
            >

            <select
              v-else
              id="edit-subject"
              v-model="editForm.subject"
            >
              <option value="">Выберите предмет</option>
              <option
                v-for="subject in availableSubjects"
                :key="subject"
                :value="subject"
              >
                {{ subject }}
              </option>
              <option value="custom">Другой предмет...</option>
            </select>
          </div>

          <div class="field">
            <div class="field-heading">
              <label for="edit-teacher">Преподаватель</label>
              <button
                type="button"
                class="field-switch"
                @click="customTeacher = !customTeacher"
              >
                {{ customTeacher ? 'Выбрать из списка' : 'Ввести вручную' }}
              </button>
            </div>

            <input
              v-if="customTeacher"
              id="edit-teacher"
              v-model="editForm.teacherName"
              type="text"
              placeholder="Имя преподавателя"
            >

            <select
              v-else
              id="edit-teacher"
              v-model="editForm.teacherName"
              @change="handleCustomTeacher"
            >
              <option value="">Выберите преподавателя</option>
              <option
                v-for="teacher in availableTeachers"
                :key="teacher"
                :value="teacher"
              >
                {{ teacher }}
              </option>
              <option value="custom">Другой преподаватель...</option>
            </select>
          </div>

          <div class="field">
            <label for="edit-description">Описание</label>
            <textarea
              id="edit-description"
              v-model="editForm.description"
              rows="5"
              placeholder="Кратко опишите материал"
            ></textarea>
          </div>

          <div class="field">
            <label>Новые файлы</label>

            <label
              class="drop-zone"
              :class="{ 'is-dragover': dragover }"
              for="edit-files"
              @dragover="handleDragOver"
              @dragleave="handleDragLeave"
              @drop="handleDrop"
            >
              <span class="drop-icon">↑</span>
              <strong>Перетащи файлы сюда</strong>
              <small>или нажми, чтобы выбрать</small>

              <input
                id="edit-files"
                :key="fileInputKey"
                class="file-input"
                type="file"
                multiple
                @change="handleFileChange"
              >
            </label>
          </div>

          <div v-if="editForm.files.length" class="selected-files">
            <div
              v-for="(file, index) in editForm.files"
              :key="`${file.name}-${index}`"
              class="selected-file"
            >
              <div>
                <strong>{{ file.name }}</strong>
                <span>{{ (file.size / 1024).toFixed(1) }} KB</span>
              </div>

              <button
                type="button"
                aria-label="Удалить файл"
                @click="removeFile(index)"
              >
                ×
              </button>
            </div>
          </div>

          <div v-if="previews.length" class="preview-grid">
            <figure
              v-for="(preview, index) in previews"
              :key="`${preview.file.name}-${index}`"
              class="preview-card"
            >
              <img :src="preview.src" :alt="preview.file.name">
              <figcaption>{{ preview.file.name }}</figcaption>
            </figure>
          </div>
        </div>

        <footer class="edit-footer">
          <button
            class="pill-button"
            type="button"
            @click="closeEditDialog"
          >
            Отмена
          </button>

          <button
            class="pill-button pill-dark"
            type="button"
            :disabled="isSubmitting"
            @click="submitEditForm"
          >
            {{ isSubmitting ? 'Сохраняем...' : 'Сохранить изменения' }}
          </button>
        </footer>
      </section>
    </div>
  </main>
</template>

<style scoped>
.profile-page {
  min-height: calc(100vh - 66px);
  padding: 32px 0 90px;
  color: var(--color-text, #111112);
}

.profile-container {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.section-kicker,
.stat-kicker,
.identity-eyebrow {
  display: block;
  font-size: 10px;
  font-weight: 800;
  letter-spacing: 0.11em;
  color: #626b67;
}

.profile-hero {
  min-height: 330px;
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(330px, 0.65fr);
  gap: 16px;
}

.profile-copy,
.identity-card {
  border: 1px solid #d9ddd8;
  border-radius: 17px;
}

.profile-copy {
  position: relative;
  overflow: hidden;
  padding: 46px 52px;
  background: #dceff5;
}

.profile-copy::after {
  content: '✳';
  position: absolute;
  top: 24px;
  right: 42px;
  font-family: Georgia, serif;
  font-size: 45px;
  transform: rotate(16deg);
}

.profile-copy h1 {
  margin: 12px 0 14px;
  display: flex;
  flex-direction: column;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: clamp(54px, 6vw, 88px);
  line-height: 0.93;
  font-weight: 600;
  letter-spacing: -0.065em;
}

.profile-copy h1 span {
  margin-left: 42px;
}

.profile-copy p {
  max-width: 580px;
  margin: 0;
  font-size: 15px;
  line-height: 1.45;
  color: #4e5552;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 28px;
}

.pill-button {
  min-height: 42px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 0 18px;
  border: 1.25px solid #222;
  border-radius: 999px;
  background: #fffefb;
  color: #111;
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}

.pill-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 2px 3px 0 #111;
}

.pill-button:disabled {
  cursor: wait;
  opacity: 0.65;
}

.pill-dark {
  background: #29292a;
  color: #fff;
}

.identity-card {
  padding: 30px;
  background: #fff;
}

.identity-top {
  display: flex;
  align-items: center;
  gap: 18px;
}

.avatar-wrap {
  position: relative;
  width: 88px;
  height: 88px;
  flex: 0 0 88px;
}

.profile-avatar,
.avatar-fallback {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  border-radius: 50%;
}

.profile-avatar {
  z-index: 2;
  object-fit: cover;
  border: 5px solid #edf3eb;
}

.avatar-fallback {
  display: grid;
  place-items: center;
  background: #e5edd6;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: 30px;
}

.identity-copy {
  min-width: 0;
}

.identity-copy h2 {
  margin: 6px 0 3px;
  font-size: 24px;
  line-height: 1.05;
  letter-spacing: -0.045em;
}

.identity-copy p {
  margin: 0;
  color: #737873;
  font-size: 12px;
  line-height: 1.4;
}

.identity-divider {
  height: 1px;
  margin: 26px 0 20px;
  background: #e8e9e5;
}

.identity-meta {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.identity-meta div {
  padding: 13px 14px;
  border-radius: 11px;
  background: #f7f8f5;
}

.identity-meta span,
.identity-meta strong {
  display: block;
}

.identity-meta span {
  margin-bottom: 4px;
  color: #8a8e89;
  font-size: 10px;
}

.identity-meta strong {
  font-size: 12px;
}

.profile-note {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 16px;
  padding: 13px;
  border-radius: 11px;
  background: #eaf2e1;
  font: italic 12px/1.35 Georgia, serif;
}

.profile-note > span {
  font-size: 25px;
  font-style: normal;
}

.error-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 16px;
  border: 1px solid #e4b9b9;
  border-radius: 12px;
  background: #fff0ef;
  color: #8e3b3b;
  font-size: 12px;
}

.error-banner > span {
  width: 26px;
  height: 26px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #f5cece;
  font-weight: 800;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 10px;
}

.stat-card {
  min-height: 142px;
  position: relative;
  overflow: hidden;
  padding: 19px;
  border: 1px solid #d9ddd8;
  border-radius: 14px;
}

.stat-card--blue {
  background: #d8f0f9;
}

.stat-card--green {
  background: #e6efd9;
}

.stat-card--cream {
  background: #f7f2e2;
}

.stat-card--dark {
  background: #29292a;
  color: #fff;
}

.stat-card--dark .stat-kicker,
.stat-card--dark p {
  color: #d0d0cf;
}

.stat-card strong {
  display: block;
  margin-top: 12px;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: 47px;
  line-height: 0.9;
  letter-spacing: -0.06em;
}

.stat-card p {
  margin: 8px 0 0;
  color: #707470;
  font-size: 11px;
}

.materials-section {
  margin-top: 20px;
}

.section-heading {
  display: flex;
  align-items: end;
  gap: 30px;
  margin: 0 6px 19px;
}

.section-heading h2 {
  margin: 5px 0 0;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: clamp(42px, 5vw, 68px);
  line-height: 0.92;
  font-weight: 600;
  letter-spacing: -0.065em;
}

.section-side {
  margin-left: auto;
  max-width: 390px;
  display: flex;
  align-items: end;
  gap: 22px;
}

.section-side p {
  margin: 0;
  color: #686e69;
  font-size: 11px;
  line-height: 1.45;
}

.text-link {
  flex: none;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 0 0 3px;
  border: 0;
  background: transparent;
  color: #111;
  font-size: 11px;
  font-weight: 800;
  cursor: pointer;
}

.material-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 15px;
}

.owned-material {
  min-width: 0;
}

.owned-actions {
  display: flex;
  gap: 7px;
  margin-top: 8px;
}

.material-action {
  flex: 1;
  min-height: 38px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 800;
  cursor: pointer;
}

.material-action--edit {
  border: 1px solid #cbd2cd;
  background: #fff;
}

.material-action--edit:hover {
  background: #e8f2f5;
}

.material-action--delete {
  border: 1px solid #ead0d0;
  background: #fff7f5;
  color: #8b4646;
}

.material-action--delete:hover {
  background: #f7e6e3;
}

.state-card,
.empty-state {
  border: 1px dashed #cbd4d0;
  border-radius: 15px;
  background: rgba(255, 255, 255, 0.72);
}

.state-card {
  min-height: 160px;
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 30px;
}

.state-symbol {
  width: 54px;
  height: 54px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  background: #dff2f9;
  font-family: Georgia, serif;
  font-size: 26px;
}

.state-card strong,
.state-card p {
  display: block;
}

.state-card p {
  margin: 4px 0 0;
  color: #777;
  font-size: 12px;
}

.empty-state {
  min-height: 280px;
  display: grid;
  grid-template-columns: 220px 1fr;
  align-items: center;
  gap: 42px;
  padding: 35px 55px;
}

.empty-art {
  height: 190px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  background: #e7efda;
  transform: rotate(-3deg);
}

.empty-art span {
  width: 70px;
  height: 70px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #fff;
  font-size: 35px;
}

.empty-state h3 {
  margin: 7px 0 6px;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: 40px;
  line-height: 1;
  font-weight: 600;
  letter-spacing: -0.055em;
}

.empty-state p {
  max-width: 500px;
  margin: 0 0 21px;
  color: #6d716d;
  font-size: 12px;
  line-height: 1.45;
}

.auth-state {
  min-height: 540px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  justify-content: center;
  position: relative;
  overflow: hidden;
  padding: 55px;
  border: 1px solid #d4dcd5;
  border-radius: 18px;
  background: #e6efd9;
}

.auth-state h1 {
  margin: 11px 0 15px;
  display: flex;
  flex-direction: column;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: clamp(55px, 7vw, 96px);
  line-height: 0.91;
  font-weight: 600;
  letter-spacing: -0.07em;
}

.auth-state h1 span {
  margin-left: 46px;
}

.auth-state p {
  max-width: 520px;
  margin: 0 0 25px;
  color: #565d58;
  font-size: 14px;
  line-height: 1.5;
}

.auth-icon {
  position: absolute;
  right: 70px;
  top: 60px;
  width: 150px;
  height: 150px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #fff;
  font: 60px Georgia, serif;
}

/* edit dialog */
.edit-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
  display: grid;
  place-items: center;
  padding: 22px;
  background: rgba(21, 25, 24, 0.58);
  backdrop-filter: blur(5px);
}

.edit-modal {
  width: min(100%, 720px);
  max-height: min(90vh, 860px);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid #fff;
  border-radius: 18px;
  background-color: #faf9f4;
  background-image:
    linear-gradient(#435b5408 1px, transparent 1px),
    linear-gradient(90deg, #435b5408 1px, transparent 1px);
  background-size: 17px 17px;
  box-shadow: 0 28px 90px rgba(0, 0, 0, 0.25);
}

.edit-header {
  display: flex;
  justify-content: space-between;
  gap: 20px;
  padding: 26px 30px 18px;
  border-bottom: 1px solid #e2e4df;
}

.edit-header h2 {
  margin: 6px 0 0;
  font-family: 'Playfair Display', Georgia, serif;
  font-size: 44px;
  line-height: 1;
  font-weight: 600;
  letter-spacing: -0.06em;
}

.close-button {
  width: 36px;
  height: 36px;
  flex: 0 0 36px;
  border: 1px solid #d8dcd8;
  border-radius: 50%;
  background: #fff;
  font-size: 24px;
  cursor: pointer;
}

.edit-body {
  overflow-y: auto;
  padding: 22px 30px 30px;
}

.edit-message {
  margin-bottom: 17px;
  padding: 12px 14px;
  border: 1px solid #e5c0c0;
  border-radius: 10px;
  background: #fff0ef;
  color: #8b4444;
  font-size: 12px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.field {
  margin-bottom: 16px;
}

.field label {
  display: block;
  margin-bottom: 7px;
  font-size: 11px;
  font-weight: 800;
}

.field-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.field-switch {
  margin-bottom: 7px;
  padding: 0;
  border: 0;
  background: transparent;
  color: #477284;
  font-size: 10px;
  font-weight: 700;
  cursor: pointer;
}

.field input,
.field select,
.field textarea {
  width: 100%;
  border: 1px solid #d5dad5;
  border-radius: 11px;
  outline: none;
  background: #fff;
  color: #111;
  font: inherit;
  font-size: 12px;
}

.field input,
.field select {
  height: 44px;
  padding: 0 13px;
}

.field textarea {
  min-height: 112px;
  padding: 12px 13px;
  resize: vertical;
  line-height: 1.45;
}

.field input:focus,
.field select:focus,
.field textarea:focus {
  border-color: #7497a3;
  box-shadow: 0 0 0 3px rgba(201, 231, 241, 0.6);
}

.drop-zone {
  min-height: 145px;
  display: flex !important;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  border: 1.5px dashed #a9bbb5;
  border-radius: 13px;
  background: rgba(230, 239, 217, 0.56);
  cursor: pointer;
}

.drop-zone.is-dragover {
  background: #dceff5;
  border-color: #638c9a;
}

.drop-icon {
  font-size: 28px;
  line-height: 1;
}

.drop-zone strong {
  font-size: 12px;
}

.drop-zone small {
  color: #747b76;
  font-size: 10px;
}

.file-input {
  display: none;
}

.selected-files {
  display: grid;
  gap: 7px;
  margin: 4px 0 17px;
}

.selected-file {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 11px;
  border: 1px solid #e1e3df;
  border-radius: 9px;
  background: #fff;
}

.selected-file > div {
  flex: 1;
  min-width: 0;
}

.selected-file strong,
.selected-file span {
  display: block;
}

.selected-file strong {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 11px;
}

.selected-file span {
  margin-top: 2px;
  color: #8a8e8a;
  font-size: 9px;
}

.selected-file button {
  width: 27px;
  height: 27px;
  border: 0;
  border-radius: 50%;
  background: #f7e8e6;
  color: #8d4343;
  cursor: pointer;
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 8px;
}

.preview-card {
  margin: 0;
  padding: 5px 5px 8px;
  overflow: hidden;
  border: 1px solid #e1e3df;
  border-radius: 9px;
  background: #fff;
}

.preview-card img {
  width: 100%;
  height: 90px;
  display: block;
  object-fit: cover;
  border-radius: 6px;
}

.preview-card figcaption {
  margin-top: 5px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #777;
  font-size: 9px;
}

.edit-footer {
  display: flex;
  justify-content: flex-end;
  gap: 9px;
  padding: 15px 30px 22px;
  border-top: 1px solid #e2e4df;
  background: rgba(250, 249, 244, 0.96);
}

@media (max-width: 1000px) {
  .profile-hero {
    grid-template-columns: 1fr;
  }

  .identity-card {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 20px;
  }

  .identity-divider {
    display: none;
  }

  .identity-meta {
    align-self: center;
  }

  .profile-note {
    grid-column: span 2;
    margin-top: 0;
  }

  .stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .material-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 700px) {
  .profile-page {
    padding-top: 14px;
  }

  .profile-copy {
    padding: 32px 25px;
  }

  .profile-copy h1 span {
    margin-left: 22px;
  }

  .identity-card {
    display: block;
    padding: 22px;
  }

  .identity-divider {
    display: block;
  }

  .profile-note {
    margin-top: 16px;
  }

  .section-heading {
    display: block;
  }

  .section-side {
    max-width: none;
    margin: 15px 0 0;
    align-items: flex-end;
  }

  .material-grid {
    grid-template-columns: 1fr;
  }

  .empty-state {
    grid-template-columns: 1fr;
    gap: 20px;
    padding: 25px;
  }

  .empty-art {
    height: 130px;
  }

  .form-grid {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .preview-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .edit-header,
  .edit-body,
  .edit-footer {
    padding-left: 20px;
    padding-right: 20px;
  }
}

@media (max-width: 450px) {
  .stats-grid {
    gap: 7px;
  }

  .stat-card {
    min-height: 126px;
    padding: 15px;
  }

  .stat-card strong {
    font-size: 40px;
  }

  .identity-top {
    align-items: flex-start;
  }

  .avatar-wrap {
    width: 68px;
    height: 68px;
    flex-basis: 68px;
  }

  .identity-meta {
    grid-template-columns: 1fr;
  }

  .section-side {
    display: block;
  }

  .text-link {
    margin-top: 10px;
  }

  .hero-actions,
  .edit-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .pill-button {
    width: 100%;
  }
}
</style>

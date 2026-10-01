<script>
import { defineComponent } from 'vue';
import axios from 'axios';
import { API_ENDPOINTS } from '../config';
import { isAuthenticated } from '../utils/auth';
import { jwtDecode } from 'jwt-decode';

export default defineComponent({
  name: 'MaterialForm',
  data() {
    return {
      form: {
        userId: null,
        course: 1,
        subject: '',
        teacherName: '',
        semester: 1,
        description: '',
        files: []
      },
      courses: [1, 2, 3, 4],
      selectionData: {},
      availableSubjects: [],
      availableTeachers: [],
      semesters: [1, 2],
      isSubmitting: false,
      isLoading: true,
      message: '',
      fileInputKey: 0,
      previews: [],
      dragover: false,
      customSubject: false,
      customTeacher: false,
      showConfirmDialog: false,
      showSuccessNotification: false,
      isAuthenticated: false
    };
  },

  mounted() {
    this.checkAuthentication();

    if (this.isAuthenticated) {
      this.fetchSelectionData();
    }
  },

  watch: {
    'form.course': function() {
      this.form.subject = '';
      this.form.teacherName = '';
      this.customSubject = false;
      this.customTeacher = false;
      this.updateAvailableSubjects();
    },

    'form.subject': function(newSubject) {
      if (newSubject === 'custom') {
        this.customSubject = true;
        this.form.subject = '';
      } else if (newSubject) {
        this.updateAvailableTeachers();
        this.form.teacherName = '';
        this.customTeacher = false;
      }
    }
  },

  methods: {
    checkAuthentication() {
      this.isAuthenticated = isAuthenticated();
    },

    async fetchSelectionData() {
      this.isLoading = true;

      try {
        const response = await axios.get(API_ENDPOINTS.SELECTION_LIST);
        this.selectionData = response.data;
        this.updateAvailableSubjects();
      } catch (error) {
        console.error('Ошибка загрузки данных для выбора:', error);
        this.message = 'Не удалось загрузить данные для выбора. Пожалуйста, обновите страницу.';
      } finally {
        this.isLoading = false;
      }
    },

    updateAvailableSubjects() {
      if (this.selectionData[this.form.course]) {
        const subjects = this.selectionData[this.form.course].subjects;
        this.availableSubjects = Object.keys(subjects || {});
      } else {
        this.availableSubjects = [];
      }
    },

    updateAvailableTeachers() {
      if (
        this.form.course &&
        this.form.subject &&
        this.selectionData[this.form.course] &&
        this.selectionData[this.form.course].subjects[this.form.subject]
      ) {
        this.availableTeachers =
          this.selectionData[this.form.course].subjects[this.form.subject].teachers || [];
      } else {
        this.availableTeachers = [];
      }
    },

    handleFileChange(event) {
      const newFiles = Array.from(event.target.files);
      this.form.files = [...this.form.files, ...newFiles];
      this.generatePreviews();
    },

    handleDrop(event) {
      event.preventDefault();
      this.dragover = false;

      if (event.dataTransfer.files) {
        const newFiles = Array.from(event.dataTransfer.files);
        this.form.files = [...this.form.files, ...newFiles];
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

      this.form.files.forEach(file => {
        if (file.type.startsWith('image/')) {
          const reader = new FileReader();

          reader.onload = e => {
            this.previews.push({
              file,
              src: e.target.result
            });
          };

          reader.readAsDataURL(file);
        }
      });
    },

    removeFile(index) {
      this.form.files.splice(index, 1);
      this.generatePreviews();
    },

    resetForm() {
      this.form = {
        userId: '00000000-0000-0000-0000-000000000000',
        course: 1,
        subject: '',
        teacherName: '',
        semester: 1,
        description: '',
        files: []
      };

      this.fileInputKey += 1;
      this.message = '';
      this.previews = [];
      this.updateAvailableSubjects();
    },

    handleCustomTeacher() {
      if (this.form.teacherName === 'custom') {
        this.customTeacher = true;
        this.form.teacherName = '';
      }
    },

    confirmSubmit() {
      if (!this.isAuthenticated) {
        this.message = 'Для добавления материала необходимо авторизоваться';
        return;
      }

      if (!this.form.subject) {
        this.message = 'Пожалуйста, заполните все обязательные поля';
        return;
      }

      this.showConfirmDialog = true;
    },

    async submitForm() {
      if (!this.isAuthenticated) {
        this.message = 'Для добавления материала необходимо авторизоваться';
        return;
      }

      this.isSubmitting = true;
      this.message = '';
      this.showConfirmDialog = false;

      try {
        const formData = new FormData();
        const token = localStorage.getItem('AuthToken');

        if (!token) {
          throw new Error('Токен авторизации не найден');
        }

        const decodedToken = jwtDecode(token);
        const studentId = decodedToken.StudentId || decodedToken.studentId;

        if (!studentId) {
          throw new Error('Идентификатор пользователя не найден в токене');
        }

        formData.append('StudentId', studentId);
        formData.append('Course', this.form.course);
        formData.append('Subject', this.form.subject);
        formData.append('TeacherName', this.form.teacherName);
        formData.append('Semester', this.form.semester);
        formData.append('Description', this.form.description);

        for (let i = 0; i < this.form.files.length; i++) {
          formData.append('Files', this.form.files[i]);
        }

        const response = await axios.post(API_ENDPOINTS.ADD_MATERIAL, formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          },
          timeout: 30000
        });

        if (response.status === 200 || response.status === 201) {
          this.message = 'Материал успешно добавлен!';
          this.resetForm();
          this.$root.$emit('material-added');
          this.showSuccessNotification = true;
        } else {
          this.message = `Не удалось добавить материал. Ответ сервера: ${response.status}`;
        }
      } catch (error) {
        console.error('Ошибка при добавлении материала:', error);

        let errorMessage = 'Произошла ошибка. Пожалуйста, попробуйте позже.';

        if (error.response) {
          errorMessage = `Ошибка сервера: ${error.response.status} - ${error.response.statusText}`;
        } else if (error.request) {
          errorMessage = 'Нет ответа от сервера. Пожалуйста, проверьте подключение.';
        } else if (error.message) {
          errorMessage = `Ошибка запроса: ${error.message}`;
        }

        this.message = errorMessage;
      } finally {
        this.isSubmitting = false;
      }
    },

    closeSuccessNotification() {
      this.showSuccessNotification = false;
      this.$router.push('/adding');
    }
  }
});
</script>

<template>
  <div class="material-form-page grid-background">
    <section class="page-container form-hero">
      <div class="section-kicker">ДОБАВЛЕНИЕ МАТЕРИАЛА</div>

      <h1>
        поделись
        <span>материалом</span>
      </h1>

      <p>
        Добавь учебный материал в архив, чтобы им могли воспользоваться другие студенты.
      </p>
    </section>

    <section class="page-container form-content">
      <div v-if="!isAuthenticated" class="state-card">
        <div class="state-icon">↗</div>

        <div>
          <strong>Нужна авторизация</strong>
          <p>
            Войди в аккаунт через меню профиля, чтобы добавлять материалы.
          </p>
        </div>
      </div>

      <div
        v-else-if="message"
        class="message-card"
        :class="{ success: message.includes('успешно') }"
      >
        {{ message }}
      </div>

      <div v-if="isAuthenticated && isLoading" class="state-card">
        <div class="state-icon">…</div>

        <div>
          <strong>Загружаем данные</strong>
          <p>Подготавливаем предметы и преподавателей.</p>
        </div>
      </div>

      <form
        v-if="isAuthenticated && !isLoading"
        class="material-form"
        @submit.prevent="confirmSubmit"
      >
        <div class="form-section">
          <div class="form-section-title">
            <div>
              <div class="section-kicker">ОСНОВНОЕ</div>
              <h2>о материале</h2>
            </div>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label for="course">Курс</label>

              <select id="course" v-model="form.course" required>
                <option
                  v-for="course in courses"
                  :key="course"
                  :value="course"
                >
                  {{ course }} курс
                </option>
              </select>
            </div>

            <div class="form-group">
              <label for="semester">Семестр</label>

              <select id="semester" v-model="form.semester" required>
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

          <div class="form-group">
            <div class="field-heading">
              <label for="subject">Предмет</label>

              <button
                type="button"
                class="mode-button"
                @click="customSubject = !customSubject"
              >
                {{ customSubject ? 'Выбрать из списка' : 'Ввести вручную' }}
              </button>
            </div>

            <select
              v-if="!customSubject"
              id="subject"
              v-model="form.subject"
              required
            >
              <option value="" disabled>Выберите предмет</option>

              <option
                v-for="subject in availableSubjects"
                :key="subject"
                :value="subject"
              >
                {{ subject }}
              </option>
            </select>

            <input
              v-else
              id="customSubject"
              v-model="form.subject"
              type="text"
              placeholder="Введите название предмета"
              required
            />
          </div>

          <div class="form-group">
            <div class="field-heading">
              <label for="teacher">Преподаватель</label>

              <button
                type="button"
                class="mode-button"
                @click="customTeacher = !customTeacher"
              >
                {{ customTeacher ? 'Выбрать из списка' : 'Ввести вручную' }}
              </button>
            </div>

            <select
              v-if="!customTeacher"
              id="teacher"
              v-model="form.teacherName"
              :disabled="!form.subject"
              @change="handleCustomTeacher"
            >
              <option value="">Не указан</option>

              <option
                v-for="teacher in availableTeachers"
                :key="teacher"
                :value="teacher"
              >
                {{ teacher }}
              </option>
            </select>

            <input
              v-else
              id="customTeacher"
              v-model="form.teacherName"
              type="text"
              placeholder="Введите имя преподавателя"
            />
          </div>

          <div class="form-group">
            <label for="description">Описание</label>

            <textarea
              id="description"
              v-model="form.description"
              rows="6"
              placeholder="Кратко опиши, что находится в материале..."
              required
            ></textarea>
          </div>
        </div>

        <div class="form-section">
          <div class="form-section-title">
            <div>
              <div class="section-kicker">ВЛОЖЕНИЯ</div>
              <h2>файлы</h2>
            </div>
          </div>

          <div
            class="file-drop-area"
            :class="{ active: dragover }"
            @dragover="handleDragOver"
            @dragleave="handleDragLeave"
            @drop="handleDrop"
          >
            <div class="drop-icon">＋</div>

            <strong>Перетащи файлы сюда</strong>
            <p>или выбери их с устройства</p>

            <label for="files" class="file-select-btn">
              Выбрать файлы
            </label>

            <input
              id="files"
              :key="fileInputKey"
              class="file-input"
              type="file"
              multiple
              @change="handleFileChange"
            />
          </div>

          <div v-if="previews.length > 0" class="preview-block">
            <div class="subheading">
              Предпросмотр изображений
            </div>

            <div class="preview-grid">
              <div
                v-for="(preview, index) in previews"
                :key="`${preview.file.name}-${index}`"
                class="preview-item"
              >
                <img
                  :src="preview.src"
                  :alt="preview.file.name"
                />

                <div class="preview-footer">
                  <div>
                    <strong>{{ preview.file.name }}</strong>
                    <span>{{ (preview.file.size / 1024).toFixed(2) }} КБ</span>
                  </div>

                  <button
                    type="button"
                    class="remove-button"
                    @click="removeFile(index)"
                  >
                    ×
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div v-if="form.files.length > 0" class="files-block">
            <div class="subheading">
              Прикреплено: {{ form.files.length }}
            </div>

            <div class="file-list">
              <div
                v-for="(file, index) in form.files"
                :key="`${file.name}-${index}`"
                class="file-row"
              >
                <div class="file-left">
                  <div
                    class="file-icon"
                    :class="{ image: file.type.startsWith('image/') }"
                  >
                    {{ file.type.startsWith('image/') ? 'IMG' : 'FILE' }}
                  </div>

                  <div class="file-copy">
                    <strong>{{ file.name }}</strong>
                    <span>{{ (file.size / 1024).toFixed(2) }} КБ</span>
                  </div>
                </div>

                <button
                  type="button"
                  class="remove-button"
                  @click="removeFile(index)"
                >
                  ×
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button
            type="button"
            class="secondary-button"
            :disabled="isSubmitting"
            @click="resetForm"
          >
            Сбросить
          </button>

          <button
            type="submit"
            class="primary-button"
            :disabled="isSubmitting"
          >
            {{ isSubmitting ? 'Сохранение...' : 'Сохранить материал' }}
            <span v-if="!isSubmitting">→</span>
          </button>
        </div>
      </form>
    </section>

    <div
      v-if="showConfirmDialog"
      class="dialog-overlay"
      @click.self="showConfirmDialog = false"
    >
      <div class="dialog">
        <div class="section-kicker">ПОДТВЕРЖДЕНИЕ</div>
        <h2>Отправить материал?</h2>

        <p>
          Проверь введённые данные перед отправкой.
        </p>

        <div class="dialog-actions">
          <button
            type="button"
            class="secondary-button"
            @click="showConfirmDialog = false"
          >
            Отмена
          </button>

          <button
            type="button"
            class="primary-button"
            @click="submitForm"
          >
            Отправить
            <span>→</span>
          </button>
        </div>
      </div>
    </div>

    <div
      v-if="showSuccessNotification"
      class="dialog-overlay"
    >
      <div class="dialog success-dialog">
        <div class="success-mark">✓</div>

        <div class="section-kicker">ГОТОВО</div>
        <h2>Материал добавлен</h2>

        <p>
          Материал успешно отправлен в архив.
        </p>

        <button
          type="button"
          class="primary-button"
          @click="closeSuccessNotification"
        >
          Продолжить
          <span>→</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.material-form-page {
  min-height: 100vh;
  padding-bottom: 90px;
  color: var(--color-text, #161616);
}

.form-hero {
  padding-top: 62px;
  padding-bottom: 40px;
}

.section-kicker {
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
  font-weight: 800;
  letter-spacing: .14em;
}

.form-hero h1 {
  margin: 10px 0 18px;
  font-size: clamp(56px, 7vw, 102px);
  font-weight: 900;
  line-height: .88;
  letter-spacing: -.065em;
}

.form-hero h1 span {
  display: inline-block;
  margin-top: 8px;
  padding: 0 12px 7px;
  background: var(--color-cream, #f4ead4);
  font-family: Georgia, 'Times New Roman', serif;
  font-weight: 400;
  transform: rotate(-1deg);
}

.form-hero p {
  max-width: 630px;
  margin: 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 17px;
  line-height: 1.55;
}

.form-content {
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.material-form {
  display: grid;
  gap: 18px;
}

.form-section {
  padding: 24px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 18px;
  background: rgba(255,255,255,.92);
  box-shadow: var(--shadow-card, 0 8px 30px rgba(24,24,20,.06));
}

.form-section-title {
  margin-bottom: 22px;
}

.form-section-title h2 {
  margin: 5px 0 0;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 42px;
  font-weight: 400;
  line-height: .95;
  letter-spacing: -.04em;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.form-group {
  margin-bottom: 18px;
}

.form-group:last-child {
  margin-bottom: 0;
}

.form-group label {
  display: block;
  margin-bottom: 7px;
  color: var(--color-text, #161616);
  font-size: 12px;
  font-weight: 800;
}

.field-heading {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 14px;
}

.field-heading label {
  margin-bottom: 7px;
}

.mode-button {
  padding: 0;
  border: 0;
  background: transparent;
  color: var(--color-text-secondary, #64645f);
  font-size: 11px;
  font-weight: 700;
}

.mode-button:hover {
  color: var(--color-text, #161616);
  text-decoration: underline;
}

.form-group select,
.form-group input,
.form-group textarea {
  width: 100%;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 12px;
  outline: none;
  background: var(--color-bg, #fbfaf6);
  color: var(--color-text, #161616);
  font-size: 13px;
}

.form-group select,
.form-group input {
  min-height: 46px;
  padding: 0 13px;
}

.form-group textarea {
  min-height: 150px;
  padding: 13px;
  resize: vertical;
  line-height: 1.5;
}

.form-group select:focus,
.form-group input:focus,
.form-group textarea:focus {
  border-color: #8fb9c9;
  box-shadow: 0 0 0 3px rgba(203,232,242,.45);
}

.form-group select:disabled {
  opacity: .55;
  cursor: not-allowed;
}

.file-drop-area {
  min-height: 250px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px;
  border: 1px dashed var(--color-border-dark, #202020);
  border-radius: 15px;
  background: var(--color-bg, #fbfaf6);
  text-align: center;
  transition:
    background-color .18s ease,
    border-color .18s ease;
}

.file-drop-area.active {
  border-color: #7da8b8;
  background: rgba(203,232,242,.42);
}

.drop-icon {
  width: 56px;
  height: 56px;
  display: grid;
  place-items: center;
  margin-bottom: 15px;
  border-radius: 16px;
  background: var(--color-blue, #cbe8f2);
  font-size: 28px;
}

.file-drop-area strong {
  font-size: 17px;
}

.file-drop-area p {
  margin: 6px 0 16px;
  color: var(--color-text-secondary, #64645f);
  font-size: 12px;
}

.file-select-btn {
  min-height: 40px;
  display: inline-flex;
  align-items: center;
  padding: 0 15px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 999px;
  background: #fff;
  color: var(--color-text, #161616);
  font-size: 11px;
  font-weight: 800;
  cursor: pointer;
}

.file-input {
  position: absolute;
  width: .1px;
  height: .1px;
  opacity: 0;
  overflow: hidden;
  z-index: -1;
}

.preview-block,
.files-block {
  margin-top: 22px;
}

.subheading {
  margin-bottom: 12px;
  font-size: 12px;
  font-weight: 800;
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 12px;
}

.preview-item {
  overflow: hidden;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 13px;
  background: var(--color-bg, #fbfaf6);
}

.preview-item img {
  width: 100%;
  height: 145px;
  display: block;
  object-fit: cover;
}

.preview-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px;
}

.preview-footer > div {
  min-width: 0;
}

.preview-footer strong,
.preview-footer span {
  display: block;
}

.preview-footer strong {
  overflow: hidden;
  font-size: 11px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.preview-footer span {
  margin-top: 3px;
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
}

.file-list {
  display: grid;
  gap: 9px;
}

.file-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 12px;
  background: var(--color-bg, #fbfaf6);
}

.file-left {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.file-icon {
  width: 44px;
  height: 44px;
  flex: 0 0 44px;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: var(--color-cream, #f4ead4);
  font-size: 9px;
  font-weight: 900;
}

.file-icon.image {
  background: var(--color-blue, #cbe8f2);
}

.file-copy {
  min-width: 0;
}

.file-copy strong,
.file-copy span {
  display: block;
}

.file-copy strong {
  overflow: hidden;
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-copy span {
  margin-top: 3px;
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
}

.remove-button {
  width: 30px;
  height: 30px;
  flex: 0 0 30px;
  display: grid;
  place-items: center;
  padding: 0;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 50%;
  background: #fff;
  color: #8e4c4c;
  font-size: 17px;
}

.remove-button:hover {
  background: #f5e3e3;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.primary-button,
.secondary-button {
  min-height: 45px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 20px;
  padding: 0 18px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 800;
}

.primary-button {
  border: 1px solid var(--color-border-dark, #202020);
  background: var(--color-text, #161616);
  color: #fff;
}

.primary-button:hover:not(:disabled) {
  background: #30302d;
}

.secondary-button {
  border: 1px solid var(--color-border, #d9d8d2);
  background: #fff;
  color: var(--color-text, #161616);
}

.secondary-button:hover:not(:disabled) {
  background: var(--color-cream, #f4ead4);
}

.primary-button:disabled,
.secondary-button:disabled {
  opacity: .5;
  cursor: not-allowed;
}

.message-card {
  padding: 13px 16px;
  border: 1px solid #c37f78;
  border-radius: 12px;
  background: #f4dfdf;
  color: #7d332d;
  font-size: 13px;
  font-weight: 700;
}

.message-card.success {
  border-color: #9eb38b;
  background: var(--color-green, #dce8c6);
  color: var(--color-text, #161616);
}

.state-card {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 26px;
  border: 1px dashed var(--color-border-dark, #202020);
  border-radius: 16px;
  background: rgba(255,255,255,.72);
}

.state-icon {
  width: 50px;
  height: 50px;
  flex: 0 0 50px;
  display: grid;
  place-items: center;
  border-radius: 14px;
  background: var(--color-blue, #cbe8f2);
  font-size: 22px;
  font-weight: 800;
}

.state-card strong {
  display: block;
  font-size: 15px;
}

.state-card p {
  margin: 5px 0 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 12px;
  line-height: 1.5;
}

.dialog-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background: rgba(20,20,18,.42);
  backdrop-filter: blur(4px);
}

.dialog {
  width: min(440px, 100%);
  padding: 28px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 18px;
  background: var(--color-bg, #fbfaf6);
  box-shadow: 0 24px 80px rgba(18,18,16,.18);
}

.dialog h2 {
  margin: 7px 0 12px;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 36px;
  font-weight: 400;
  line-height: .95;
  letter-spacing: -.04em;
}

.dialog p {
  margin: 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 13px;
  line-height: 1.5;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 9px;
  margin-top: 24px;
}

.success-dialog {
  text-align: center;
}

.success-mark {
  width: 62px;
  height: 62px;
  display: grid;
  place-items: center;
  margin: 0 auto 18px;
  border-radius: 50%;
  background: var(--color-green, #dce8c6);
  font-size: 26px;
  font-weight: 900;
}

.success-dialog .primary-button {
  margin-top: 22px;
}

@media (max-width: 680px) {
  .form-hero {
    padding-top: 42px;
  }

  .form-hero h1 {
    font-size: clamp(48px, 15vw, 72px);
  }

  .form-grid {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .field-heading {
    align-items: flex-start;
    flex-direction: column;
    gap: 0;
  }

  .form-actions,
  .dialog-actions {
    flex-direction: column-reverse;
  }

  .primary-button,
  .secondary-button {
    width: 100%;
  }

  .file-row {
    align-items: flex-start;
  }
}
</style>

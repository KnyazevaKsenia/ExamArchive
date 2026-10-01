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
      fileInputKey: 0, // Used to reset file input
      previews: [], // To store image previews
      dragover: false,
      customSubject: false,
      customTeacher: false,
      showConfirmDialog: false,
      showSuccessNotification: false, // Для отображения уведомления об успехе
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
    'form.course': function(newCourse) {
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
      if (this.form.course && this.form.subject && 
          this.selectionData[this.form.course] && 
          this.selectionData[this.form.course].subjects[this.form.subject]) {
        this.availableTeachers = this.selectionData[this.form.course].subjects[this.form.subject].teachers || [];
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
          reader.onload = (e) => {
            this.previews.push({
              file: file,
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
      this.fileInputKey += 1; // Force reset the file input
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
        const token = localStorage.getItem("AuthToken");
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
          
          // Показываем уведомление об успехе вместо перенаправления
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
    },
    goToLogin() {
      this.$router.push('/login');
    }
  }
});
</script>

<template>
  <div class="material-form">
    <div class="tittle">
        <h2>Добавление нового материала</h2>
    </div>
    
    
    <!-- Authentication Required Message -->
    <div v-if="!isAuthenticated" class="auth-required">
      <h3>Требуется авторизация</h3>
      <p>Для добавления материалов необходимо авторизоваться.</p>
      <button @click="goToLogin" class="btn btn-primary">Войти</button>
    </div>
    
    <div v-else>
      <div v-if="message" class="message" :class="{ 'success': message.includes('успешно') }">
        {{ message }}
      </div>
      
      <div v-if="isLoading" class="loading-message">
        <div class="loading-spinner"></div>
        <p>Загрузка данных...</p>
      </div>
      
      <form @submit.prevent="confirmSubmit" v-if="!isLoading">
        <div class="form-group">
          <label for="course">Курс:</label>
          <select id="course" v-model="form.course" required>
            <option v-for="course in courses" :key="course" :value="course">
              {{ course }}
            </option>
          </select>
        </div>
        
        <div class="form-group">
          <label for="subject">Предмет:</label>
          <div class="custom-field-container">
            <select id="subject" v-model="form.subject" required v-if="!customSubject">
              <option v-for="subject in availableSubjects" :key="subject" :value="subject">
                {{ subject }}
              </option>
            </select>
            <input
              v-if="customSubject"
              type="text"
              id="customSubject"
              v-model="form.subject"
              placeholder="Введите свой предмет"
              required
            />
            <button 
              type="button" 
              class="toggle-btn" 
              @click="customSubject = !customSubject"
            >
              {{ customSubject ? 'Из списка' : 'Свой' }}
            </button>
          </div>
        </div>
        
        <div class="form-group">
          <label for="teacher">Преподаватель:</label>
          <div class="custom-field-container">
            <select 
              id="teacher" 
              v-model="form.teacherName" 
              :disabled="!form.subject" 
              v-if="!customTeacher"
              @change="handleCustomTeacher"
            >
              
              <option v-for="teacher in availableTeachers" :key="teacher" :value="teacher">
                {{ teacher }}
              </option>
              
            </select>
            <input
              v-if="customTeacher"
              type="text"
              id="customTeacher"
              v-model="form.teacherName"
              placeholder="Введите имя преподавателя"
            />
            <button 
              type="button" 
              class="toggle-btn" 
              @click="customTeacher = !customTeacher"
            >
              {{ customTeacher ? 'Из списка' : 'Свой' }}
            </button>
          </div>
        </div>
        
        <div class="form-group">
          <label for="semester">Семестр:</label>
          <select id="semester" v-model="form.semester" required>
            <option v-for="semester in semesters" :key="semester" :value="semester">
              {{ semester }}
            </option>
          </select>
        </div>
        
        <div class="form-group">
          <label for="description">Описание:</label>
          <textarea
            id="description"
            v-model="form.description"
            rows="6"
            placeholder="Введите описание материала..."
            required
          ></textarea>
        </div>
        
        <div class="form-group">
          <label>Прикрепить файлы и фотографии:</label>
          <div 
            class="file-drop-area" 
            :class="{ 'active': dragover }"
            @dragover="handleDragOver"
            @dragleave="handleDragLeave"
            @drop="handleDrop"
          >
            <div class="file-drop-text">
              <div class="icon">📁</div>
              <p>Перетащите файлы сюда</p>
              <p>или</p>
              <label for="files" class="file-select-btn">Выбрать файлы</label>
              <input
                type="file"
                id="files"
                multiple
                @change="handleFileChange"
                :key="fileInputKey"
                class="file-input"
              />
            </div>
          </div>
          
          <div v-if="previews.length > 0" class="image-previews">
            <h4>Предпросмотр изображений</h4>
            <div class="preview-container">
              <div v-for="(preview, index) in previews" :key="index" class="preview-item">
                <img :src="preview.src" :alt="preview.file.name" class="preview-image" />
                <div class="preview-info">
                  <div class="preview-name">{{ preview.file.name }}</div>
                  <div class="preview-size">{{ (preview.file.size / 1024).toFixed(2) }} КБ</div>
                </div>
                <button type="button" class="remove-btn" @click="removeFile(index)">×</button>
              </div>
            </div>
          </div>
          
          <div v-if="form.files.length > 0" class="file-info">
            <h4>Прикрепленные файлы ({{ form.files.length }})</h4>
            <ul class="file-list">
              <li v-for="(file, index) in form.files" :key="index" class="file-item">
                <div class="file-icon">
                  {{ file.type.startsWith('image/') ? '🖼️' : '📄' }}
                </div>
                <div class="file-details">
                  <div class="file-name">{{ file.name }}</div>
                  <div class="file-size">{{ (file.size / 1024).toFixed(2) }} КБ</div>
                </div>
                <button type="button" class="remove-btn" @click="removeFile(index)">×</button>
              </li>
            </ul>
          </div>
        </div>
        
        <div class="form-actions">
          <button type="button" @click="resetForm" class="btn btn-secondary" :disabled="isSubmitting">Сбросить</button>
          <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
            <span v-if="isSubmitting" class="btn-spinner"></span>
            {{ isSubmitting ? 'Сохранение...' : 'Сохранить материал' }}
          </button>
        </div>
      </form>
    </div>
    
    <div v-if="showConfirmDialog" class="confirm-dialog-overlay">
      <div class="confirm-dialog">
        <h3>Подтверждение отправки</h3>
        <p>Вы уверены, что хотите отправить этот материал?</p>
        <div class="confirm-actions">
          <button @click="showConfirmDialog = false" class="btn btn-secondary">Отмена</button>
          <button @click="submitForm" class="btn btn-primary">Отправить</button>
        </div>
      </div>
    </div>
    
    <!-- Уведомление об успешном добавлении материала -->
    <div v-if="showSuccessNotification" class="success-notification-overlay">
      <div class="success-notification">
        <h3>Успех!</h3>
        <p>Материал успешно добавлен.</p>
        <button @click="closeSuccessNotification" class="btn btn-primary">Продолжить</button>
      </div>
    </div>
  </div>
</template>

<style scoped>

.tittle{
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
.material-form {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.form-group {
  margin-bottom: 30px;
}

label {
  display: block;
  margin-bottom: 8px;
  font-weight: bold;
  color: #4a5568;
}

select, textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #cbd5e0;
  border-radius: 15px;
  font-size: 16px;
  transition: border-color 0.3s;
}

select:focus, textarea:focus {
  border-color: #4299e1;
  outline: none;
  box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.2);
}

select:disabled {
  background-color: #edf2f7;
  cursor: not-allowed;
}

textarea {
  resize: vertical;
  min-height: 120px;
}

.loading-spinner, .btn-spinner {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
  margin-right: 10px;
}

.loading-spinner {
  width: 30px;
  height: 30px;
  border-width: 4px;
  margin-bottom: 15px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.confirm-dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.confirm-dialog {
  background-color: white;
  border-radius: 6px;
  padding: 20px;
  width: 90%;
  max-width: 400px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.confirm-dialog h3 {
  margin-top: 0;
  color: #2d3748;
  margin-bottom: 15px;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}

.loading-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.file-drop-area {
  border: 2px dashed #cbd5e0;
  border-radius: 6px;
  padding: 30px;
  text-align: center;
  transition: all 0.3s;
  background-color: #f7fafc;
  cursor: pointer;
}

.file-drop-area.active {
  border-color: #4299e1;
  background-color: rgba(66, 153, 225, 0.1);
}

.file-drop-text {
  display: flex;
  flex-direction: column;
  align-items: center;
  color: #4a5568;
}

.file-drop-text .icon {
  font-size: 48px;
  margin-bottom: 10px;
}

.file-drop-text p {
  margin-bottom: 10px;
}

.file-select-btn {
  background-color: #4299e1;
  color: white;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
  transition: background-color 0.3s;
  margin-top: 10px;
}

.file-select-btn:hover {
  background-color: #3182ce;
}

.file-input {
  position: absolute;
  width: 0.1px;
  height: 0.1px;
  opacity: 0;
  overflow: hidden;
  z-index: -1;
}

.image-previews {
  margin-top: 20px;
}

.image-previews h4 {
  margin-bottom: 10px;
  color: #4a5568;
}

.preview-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 15px;
}

.preview-item {
  position: relative;
  border-radius: 6px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.preview-image {
  width: 100%;
  height: 120px;
  object-fit: cover;
  display: block;
}

.preview-info {
  padding: 8px;
  background-color: white;
  font-size: 12px;
}

.preview-name {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: #2d3748;
}

.preview-size {
  color: #718096;
  font-size: 11px;
}

.file-info {
  margin-top: 20px;
}

.file-info h4 {
  margin-bottom: 10px;
  color: #4a5568;
}

.file-list {
  list-style: none;
  padding: 0;
}

.file-item {
  display: flex;
  align-items: center;
  padding: 10px;
  background-color: white;
  border-radius: 4px;
  margin-bottom: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.file-icon {
  font-size: 24px;
  margin-right: 15px;
  color: #4a5568;
}

.file-details {
  flex: 1;
}

.file-name {
  font-weight: 600;
  color: #2d3748;
  margin-bottom: 3px;
}

.file-size {
  font-size: 12px;
  color: #718096;
}

.remove-btn {
  background-color: #fc8181;
  color: white;
  border: none;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  padding: 0;
  transition: background-color 0.2s;
}

.remove-btn:hover {
  background-color: #f56565;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 30px;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  transition: background-color 0.3s;
}

.btn-primary {
  background-color: #234866;
  color: white;
}

.btn-primary:hover {
  background-color: #225d94;
}

.btn-primary:disabled {
  background-color: #a0aec0;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #e2e8f0;
  color: #4a5568;
}

.btn-secondary:hover {
  background-color: #cbd5e0;
}

.message {
  padding: 10px 15px;
  margin-bottom: 20px;
  border-radius: 4px;
  background-color: #fc8181;
  color: #fff;
}

.message.success {
  background-color: #68d391;
}

.custom-field-container {
  display: flex;
  align-items: center;
  gap: 10px;
  border-radius: px;

}

.custom-field-container select,
.custom-field-container input {
  flex: 1;
}

.toggle-btn {
  background-color: #718096;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 12px;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.toggle-btn:hover {
  background-color: #4a5568;
}

.success-notification-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1100;
  animation: fadeIn 0.3s ease-out;
}

.success-notification {
  background-color: white;
  border-radius: 8px;
  padding: 30px;
  width: 90%;
  max-width: 400px;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
  text-align: center;
  animation: slideUp 0.4s ease-out;
}

.success-icon {
  font-size: 60px;
  margin-bottom: 20px;
}

.success-notification h3 {
  color: #2d3748;
  font-size: 24px;
  margin-bottom: 15px;
}

.success-notification p {
  color: #4a5568;
  margin-bottom: 25px;
  font-size: 16px;
}

.ok-button {
  min-width: 120px;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
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

.btn-primary {
  background-color: #4299e1;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  font-weight: bold;
  cursor: pointer;
}

.btn-primary:hover {
  background-color: #3182ce;
}
</style> 
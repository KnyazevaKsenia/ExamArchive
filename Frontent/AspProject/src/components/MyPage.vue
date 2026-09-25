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
  mounted() {
    this.checkAuthentication();
    if (this.isAuthenticated) {
      this.getUserInfoFromToken();
      this.fetchUserMaterials();
      this.fetchSelectionData();
    }
  },
  watch: {
    'editForm.course': function(newCourse) {
      this.editForm.subject = '';
      this.editForm.teacherName = '';
      this.customSubject = false;
      this.customTeacher = false;
      this.updateAvailableSubjects();
    },
    'editForm.subject': function(newSubject) {
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
          
          // Extract user information from claims
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
        this.error = 'Could not load user profile information';
      }
    },
    async fetchUserMaterials() {
      this.loading = true;
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
        this.error = 'Could not load your materials';
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
      if (this.editForm.course && this.editForm.subject && 
          this.selectionData[this.editForm.course] && 
          this.selectionData[this.editForm.course].subjects[this.editForm.subject]) {
        this.availableTeachers = this.selectionData[this.editForm.course].subjects[this.editForm.subject].teachers || [];
      } else {
        this.availableTeachers = [];
      }
    },
    async deleteMaterial(materialId) {
      if (!confirm('Вы уверены, что хотите удалить этот материал?')) {
        return;
      }
      
      try {
        const response = await axios.delete(`${API_ENDPOINTS.DELETE_MATERIAL}/${materialId}`);
        if (response.status === 200) {
          // Remove the material from the list
          this.userMaterials = this.userMaterials.filter(m => m.id !== materialId);
          alert('Материал удален успешно');
        }
      } catch (error) {
        console.error('Ошибка при удалении материала:', error);
        if (error.response && error.response.status === 404) {
          alert('Материал не найден');
        } else {
          alert('Не удалось удалить материал');
        }
      }
    },
    goToLogin() {
      this.$router.push('/login');
    },
    viewMaterialDetails(materialId) {
      this.$router.push(`/materials/${materialId}`);
    },
    editMaterial(materialId) {
      this.editingMaterial = this.userMaterials.find(m => m.id === materialId);
      if (this.editingMaterial) {
        this.editForm = {
          course: this.editingMaterial.course,
          subject: this.editingMaterial.subject,
          teacherName: this.editingMaterial.teacherName,
          semester: this.editingMaterial.semester,
          description: this.editingMaterial.description,
          files: []
        };
        this.updateAvailableSubjects();
        this.updateAvailableTeachers();
        this.showEditDialog = true;
      }
    },
    handleFileChange(event) {
      const newFiles = Array.from(event.target.files);
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
        
        for (let i = 0; i < this.editForm.files.length; i++) {
          formData.append('Files', this.editForm.files[i]);
        }
        
        const response = await axios.post(`${API_ENDPOINTS.CHANGE_MATERIAL}/${this.editingMaterial.id}`, formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          },
          timeout: 30000
        });
        
        if (response.status === 200) {
          // Update the material in the list
          const index = this.userMaterials.findIndex(m => m.id === this.editingMaterial.id);
          if (index !== -1) {
            this.userMaterials[index] = {
              ...this.userMaterials[index],
              course: this.editForm.course,
              subject: this.editForm.subject,
              teacherName: this.editForm.teacherName,
              semester: this.editForm.semester,
              description: this.editForm.description
            };
          }
          
          this.closeEditDialog();
          alert('Материал успешно изменен');
          
          // Refresh materials to get updated files and images
          this.fetchUserMaterials();
        }
      } catch (error) {
        console.error('Ошибка при изменении материала:', error);
        
        let errorMessage = 'Произошла ошибка. Пожалуйста, попробуйте позже.';
        if (error.response) {
          errorMessage = `Ошибка сервера: ${error.response.status} - ${error.response.statusText}`;
        } else if (error.request) {
          errorMessage = 'Нет ответа от сервера. Пожалуйста, проверьте подключение.';
        } else if (error.message) {
          errorMessage = `Ошибка запроса: ${error.message}`;
        }
        
        this.editMessage = errorMessage;
      } finally {
        this.isSubmitting = false;
      }
    }
  }
};
</script>

<template>
  <div class="my-page">
    <div class="tittle">
        <h2>Мой профиль</h2>

    </div>
    
    <!-- Authentication Required Message -->
    <div v-if="!isAuthenticated" class="auth-required">
      <h3>Требуется авторизация</h3>
      <p>Для доступа к этой странице необходимо авторизоваться.</p>
      <p class="institute-warning">Для поиска материалов по вашему институту также требуется авторизация.</p>
      <button @click="goToLogin" class="btn btn-primary">Войти</button>
    </div>
    
    <div v-else>
      <div v-if="error" class="error-message">
        {{ error }}
      </div>
      <div class="profile">
        <div class="profile-section">
          <div class="profile-info">
            <h3>{{ user.FirstName }}</h3>
            <h3>{{ user.LastName }}</h3>
            <p>Университет: {{ user.University }}</p>
            <p>Институт: {{ user.Institute }}</p>
          </div>
        </div>
      </div>
      
    
      <div class="my-materials">
        <h3>Мои материалы</h3>
        
        <div v-if="loading" class="loading">
          Загрузка материалов...
        </div>
        
        <div v-else-if="userMaterials.length === 0" class="empty-materials">
          У вас пока нет добавленных материалов.
        </div>
        
        <div v-else class="material-cards">
          <div v-for="material in userMaterials" :key="material.id" class="material-card-container">
            <Material 
              :material="material" 
              @view-details="viewMaterialDetails"
            />
            <div class="material-actions">
              <button @click="editMaterial(material.id)" class="btn btn-sm btn-edit">Изменить</button>
              <button @click="deleteMaterial(material.id)" class="btn btn-sm btn-danger">Удалить</button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Edit Material Dialog -->
    <div v-if="showEditDialog" class="edit-dialog-overlay">
      <div class="edit-dialog">
        <div class="edit-dialog-header">
          <h3>Изменение материала</h3>
          <button @click="closeEditDialog" class="btn-close">&times;</button>
        </div>
        
        <div class="edit-dialog-body">
          <div v-if="editMessage" class="edit-message" :class="{ 'error-message': editMessage.includes('Ошибка') }">
            {{ editMessage }}
          </div>
          
          <div class="form-group">
            <label for="course">Курс:</label>
            <select id="course" v-model="editForm.course" class="form-control">
              <option v-for="course in courses" :key="course" :value="course">{{ course }}</option>
            </select>
          </div>
          
          <div class="form-group">
            <label for="subject">Предмет:</label>
            <div v-if="customSubject">
              <input type="text" id="subject" v-model="editForm.subject" class="form-control" placeholder="Введите название предмета">
              <button @click="customSubject = false" class="btn btn-link">Выбрать из списка</button>
            </div>
            <div v-else>
              <select id="subject" v-model="editForm.subject" class="form-control">
                <option value="">Выберите предмет</option>
                <option v-for="subject in availableSubjects" :key="subject" :value="subject">{{ subject }}</option>
                <option value="custom">Другой предмет...</option>
              </select>
            </div>
          </div>
          
          <div class="form-group">
            <label for="teacherName">Преподаватель:</label>
            <div v-if="customTeacher">
              <input type="text" id="teacherName" v-model="editForm.teacherName" class="form-control" placeholder="Введите имя преподавателя">
              <button @click="customTeacher = false" class="btn btn-link">Выбрать из списка</button>
            </div>
            <div v-else>
              <select id="teacherName" v-model="editForm.teacherName" class="form-control" @change="handleCustomTeacher">
                <option value="">Выберите преподавателя</option>
                <option v-for="teacher in availableTeachers" :key="teacher" :value="teacher">{{ teacher }}</option>
                <option value="custom">Другой преподаватель...</option>
              </select>
            </div>
          </div>
          
          <div class="form-group">
            <label for="semester">Семестр:</label>
            <select id="semester" v-model="editForm.semester" class="form-control">
              <option v-for="semester in semesters" :key="semester" :value="semester">{{ semester }}</option>
            </select>
          </div>
          
          <div class="form-group">
            <label for="description">Описание:</label>
            <textarea id="description" v-model="editForm.description" class="form-control" rows="4" placeholder="Опишите материал"></textarea>
          </div>
          
          <div class="form-group">
            <label>Файлы:</label>
            <div 
              class="file-drop-area" 
              :class="{ 'dragover': dragover }"
              @dragover="handleDragOver"
              @dragleave="handleDragLeave"
              @drop="handleDrop"
            >
              <p>Перетащите файлы сюда или <label for="fileInput" class="file-label">выберите файлы</label></p>
              <input 
                type="file" 
                id="fileInput" 
                multiple 
                @change="handleFileChange"
                class="file-input"
                :key="fileInputKey"
              />
            </div>
            
            <div v-if="editForm.files.length > 0" class="selected-files">
              <div v-for="(file, index) in editForm.files" :key="index" class="selected-file">
                <div class="file-info">
                  <span>{{ file.name }}</span>
                  <span class="file-size">({{ (file.size / 1024).toFixed(2) }} KB)</span>
                </div>
                <button @click="removeFile(index)" class="btn-remove-file">&times;</button>
              </div>
            </div>
            
            <div v-if="previews.length > 0" class="image-previews">
              <div v-for="(preview, index) in previews" :key="index" class="image-preview">
                <img :src="preview.src" :alt="preview.file.name" />
                <div class="preview-file-name">{{ preview.file.name }}</div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="edit-dialog-footer">
          <button @click="closeEditDialog" class="btn btn-secondary">Отмена</button>
          <button @click="submitEditForm" class="btn btn-primary" :disabled="isSubmitting">
            <span v-if="isSubmitting">Сохранение...</span>
            <span v-else>Сохранить изменения</span>
          </button>
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

.my-page {
  max-width: 1000px;
  margin: 0 auto;
}

.profile{
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
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

.institute-warning {
  font-weight: bold;
  color: #e53e3e;
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

.profile-section {
  background-color: rgb(255, 255, 255);
  border-radius: 40px;
  padding: 20px;
  margin-bottom: 30px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  width: 40vh;
}

.profile-info h3 {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin-bottom: 10px;
  color: #3f71c6;
  font-size: x-large;
}

.profile-info p {
  color: #4a5568;
  margin-bottom: 5px;
}

.my-materials {
  background-color: white;
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.my-materials h3 {
  margin-bottom: 20px;
  color: #2d3748;
}

.loading, .empty-materials, .error-message {
  padding: 20px;
  text-align: center;
  background-color: #f7fafc;
  border-radius: 6px;
  margin: 10px 0;
}

.loading {
  color: #4a5568;
}

.empty-materials {
  color: #718096;
}

.error-message {
  color: #e53e3e;
  background-color: #fff5f5;
  border: 1px solid #fed7d7;
}

.material-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.material-card-container {
  display: flex;
  flex-direction: column;
}

.material-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
}

.btn {
  padding: 6px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
  margin-right: 5px;
  font-size: 12px;
  text-decoration: none;
  display: inline-block;
}

.btn-sm {
  padding: 4px 8px;
}

.btn-edit {
  background-color: #f6ad55;
  color: white;
}

.btn-edit:hover {
  background-color: #ed8936;
}

.btn-danger {
  background-color: #fc8181;
  color: white;
}

.btn-danger:hover {
  background-color: #f56565;
}

.btn-secondary {
  background-color: #a0aec0;
  color: white;
}

.btn-secondary:hover {
  background-color: #718096;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

/* Edit Dialog Styles */
.edit-dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 100;
}

.edit-dialog {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  width: 90%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
}

.edit-dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #e2e8f0;
}

.edit-dialog-header h3 {
  margin: 0;
  font-size: 1.25rem;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #4a5568;
}

.edit-dialog-body {
  padding: 20px;
}

.edit-dialog-footer {
  padding: 16px 20px;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: bold;
  color: #4a5568;
}

.form-control {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #cbd5e0;
  border-radius: 4px;
  font-size: 16px;
}

textarea.form-control {
  resize: vertical;
}

.btn-link {
  background: none;
  border: none;
  color: #4299e1;
  padding: 0;
  font-size: 14px;
  cursor: pointer;
  text-decoration: underline;
}

.edit-message {
  margin-bottom: 16px;
  padding: 12px;
  border-radius: 4px;
  background-color: #f0fff4;
  border: 1px solid #c6f6d5;
  color: #2f855a;
}

.edit-message.error-message {
  background-color: #fff5f5;
  border: 1px solid #fed7d7;
  color: #e53e3e;
}

.file-drop-area {
  border: 2px dashed #cbd5e0;
  border-radius: 6px;
  padding: 20px;
  text-align: center;
  margin-bottom: 16px;
  transition: background-color 0.3s;
}

.file-drop-area.dragover {
  background-color: #ebf8ff;
  border-color: #4299e1;
}

.file-label {
  color: #4299e1;
  cursor: pointer;
}

.file-input {
  display: none;
}

.selected-files {
  margin-top: 12px;
}

.selected-file {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #f7fafc;
  padding: 8px 12px;
  border-radius: 4px;
  margin-bottom: 8px;
}

.file-info {
  display: flex;
  flex-direction: column;
}

.file-size {
  font-size: 12px;
  color: #718096;
}

.btn-remove-file {
  background: none;
  border: none;
  color: #f56565;
  font-size: 18px;
  cursor: pointer;
  padding: 0 8px;
}

.image-previews {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 12px;
  margin-top: 16px;
}

.image-preview {
  position: relative;
}

.image-preview img {
  width: 100%;
  height: 100px;
  object-fit: cover;
  border-radius: 4px;
}

.preview-file-name {
  font-size: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-top: 4px;
}
</style> 
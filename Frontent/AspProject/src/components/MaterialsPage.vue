<script>
import { defineComponent } from 'vue';
import axios from 'axios';
import Material from './Material.vue';
import { API_ENDPOINTS } from '../config';
import { isAuthenticated } from '../utils/auth';

export default defineComponent({
  name: 'MaterialsPage',
  components: {
    Material
  },
  data() {
    return {
      materials: [],
      filteredMaterials: [],
      filters: {
        course: null,
        subject: '',
        teacherName: '',
        semester: null
      },
      keyword: '',
      courses: [1, 2, 3, 4],
      semesters: [1, 2],
      selectionData: {},
      availableSubjects: [],
      availableTeachers: [],
      isLoading: true,
      customSubject: false,
      customTeacher: false,
      successMessage: '',
      noResultsMessage: '',
      isAuthenticated: false
    };
  },
  mounted() {
    this.checkAuthentication();
    this.fetchSelectionData();
    this.fetchMaterials();
    this.$root.$on('material-added', () => {
      this.successMessage = 'Материал успешно добавлен';
      setTimeout(() => this.successMessage = '', 5000);
      this.searchMaterials();
    });
  },
  beforeDestroy() {
    this.$root.$off('material-added');
  },
  watch: {
    'filters.course'(newCourse) {
      this.filters.subject = '';
      this.filters.teacherName = '';
      newCourse === null ? this.availableSubjects = [] : this.updateAvailableSubjects();
      this.applyFilters();
    },
    'filters.subject'(newSubject) {
      this.filters.teacherName = '';
      if (newSubject) this.updateAvailableTeachers();
      this.applyFilters();
    },
    'filters.teacherName'() { this.applyFilters(); },
    'filters.semester'() { this.applyFilters(); }
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
      } catch (error) {
        console.error('Ошибка при получении данных для выбора:', error);
      } finally {
        this.isLoading = false;
      }
    },
    async fetchMaterials() {
      await this.searchMaterials();
    },
    async searchByKeyword() {
      if (!this.keyword.trim()) {
        return this.searchMaterials();
      }

      this.isLoading = true;
      this.noResultsMessage = '';
      try {
        const searchUrl = `${API_ENDPOINTS.SEARCH_BY_KEYWORD}/${encodeURIComponent(this.keyword.trim())}`;
        console.log('Searching by keyword at URL:', searchUrl);
        
        const response = await axios.get(searchUrl);
        console.log('Search response:', response.data);
        
        this.materials = response.data.map(material => ({
          id: material.id || material.Id,
          description: material.description || material.Description,
          course: material.course || material.Course,
          subject: material.subject || material.Subject,
          teacherName: material.teacher || material.Teacher,
          semester: material.semester || material.Semester,
          date: material.createdAt || material.CreatedAt,
          imagesNameUrl: material.imagesNameUrl || material.ImagesNameUrl || {},
          fileNameUrl: material.fileNameUrl || material.FileNameUrl || {}
        }));
        
        this.filteredMaterials = [...this.materials];
        
        // Set message if no results found
        if (this.materials.length === 0) {
          this.noResultsMessage = `По запросу "${this.keyword}" ничего не найдено.`;
        }
      } catch (error) {
        console.error('Ошибка при поиске материалов по ключевому слову:', error);
        this.materials = [];
        this.filteredMaterials = [];
        this.noResultsMessage = 'Произошла ошибка при поиске. Пожалуйста, попробуйте позже.';
      } finally {
        this.isLoading = false;
      }
    },
    clearKeywordSearch() {
      this.keyword = '';
      this.noResultsMessage = '';
      this.searchMaterials();
    },
    updateAvailableSubjects() {
      const course = this.filters.course;
      this.availableSubjects = course && this.selectionData[course] ? 
        Object.keys(this.selectionData[course].subjects || {}) : [];
    },
    updateAvailableTeachers() {
      const { course, subject } = this.filters;
      this.availableTeachers = (course && subject && 
        this.selectionData[course]?.subjects[subject]?.teachers) || [];
    },
    toggleCustomSubject() {
      this.customSubject = !this.customSubject;
      if (!this.customSubject) this.filters.subject = '';
    },
    toggleCustomTeacher() {
      this.customTeacher = !this.customTeacher;
      if (!this.customTeacher) this.filters.teacherName = '';
    },
    resetFilters() {
      this.filters = { course: null, subject: '', teacherName: '', semester: null };
      this.customSubject = false;
      this.customTeacher = false;
      this.noResultsMessage = '';
      this.filteredMaterials = [...this.materials];
    },
    async searchMaterials() {
      this.isLoading = true;
      this.noResultsMessage = '';
      try {
        const params = {
          course: this.filters.course || 0,
          subject: this.filters.subject || null,
          teacherName: this.filters.teacherName || null,
          semester: this.filters.semester || 0
        };

        const response = await axios.get(API_ENDPOINTS.GET_MATERIALS, { params });

        this.materials = response.data.map(material => ({
          id: material.id || material.Id,
          description: material.description || material.Description,
          course: material.course || material.Course,
          subject: material.subject || material.Subject,
          teacherName: material.teacher || material.Teacher,
          semester: material.semester || material.Semester,
          date: material.createdAt || material.CreatedAt,
          imagesNameUrl: material.imagesNameUrl || material.ImagesNameUrl || {},
          fileNameUrl: material.fileNameUrl || material.FileNameUrl || {}
        }));
        
        this.filteredMaterials = [...this.materials];
        
        // Set message if no results found with filters
        if (this.materials.length === 0) {
          this.noResultsMessage = 'По вашему запросу ничего не найдено.';
        }
      } catch (error) {
        console.error('Ошибка при поиске материалов:', error);
        this.materials = [];
        this.filteredMaterials = [];
        this.noResultsMessage = 'Произошла ошибка при поиске. Пожалуйста, попробуйте позже.';
      } finally {
        this.isLoading = false;
      }
    },
    applyFilters() {
      const { course, subject, teacherName, semester } = this.filters;
      
      this.filteredMaterials = this.materials.filter(material => {
        return (!course || material.course === course) && 
               (!subject || material.subject === subject) && 
               (!teacherName || material.teacherName === teacherName) && 
               (!semester || material.semester === semester);
      });
    },
    viewMaterialDetails(materialId) {
      this.$router.push(`/materials/${materialId}`);
    }
  }
});
</script>

<template>
  <div class="materials-page">
    <div class="tittle">
      <h2>Доступные материалы</h2>
    </div>
    
    <div v-if="successMessage" class="success-message">{{ successMessage }}</div>
    
    <div class="search-container">
      <div class="search-input-container">
        <input 
          type="text" 
          v-model="keyword" 
          placeholder="Поиск материалов по ключевому слову..."
          class="search-input"
          @keyup.enter="searchByKeyword"
        />
        <button 
          v-if="keyword" 
          @click="clearKeywordSearch" 
          class="clear-search-btn"
          title="Очистить поиск"
        >×</button>
      </div>
      <button @click="searchByKeyword" class="btn btn-primary search-btn">Поиск</button>
    </div>
    
    <div class="filters-container" v-if="!isLoading">
      <div class="filter-header">
        <h3>Фильтр материалов</h3>
        <button @click="resetFilters" class="btn-reset">Сбросить фильтры</button>
      </div>
      
      <div class="filters">
        <div class="filter-item">
          <label for="course-filter">Курс:</label>
          <select id="course-filter" v-model="filters.course">
            <option :value="null">Все курсы</option>
            <option v-for="course in courses" :key="course" :value="course">{{ course }}</option>
          </select>
        </div>
        
        <div class="filter-item">
          <label for="subject-filter">Предмет:</label>
          <div class="custom-field-container">
            <select v-if="!customSubject" id="subject-filter" v-model="filters.subject" :disabled="filters.course === null">
              <option value="">Все предметы</option>
              <option v-for="subject in availableSubjects" :key="subject" :value="subject">{{ subject }}</option>
            </select>
            <input v-else type="text" id="customSubject" v-model="filters.subject" placeholder="Введите предмет для фильтрации" />
            <button type="button" class="toggle-btn" @click="toggleCustomSubject()" :disabled="filters.course === null">
              {{ customSubject ? 'Из списка' : 'Свой' }}
            </button>
          </div>
        </div>
        
        <div class="filter-item">
          <label for="teacher-filter">Преподаватель:</label>
          <div class="custom-field-container">
            <select v-if="!customTeacher" id="teacher-filter" v-model="filters.teacherName" :disabled="!filters.subject">
              <option value="">Все преподаватели</option>
              <option v-for="teacher in availableTeachers" :key="teacher" :value="teacher">{{ teacher }}</option>
            </select>
            <input v-else type="text" id="customTeacher" v-model="filters.teacherName" placeholder="Введите преподавателя для фильтрации" />
            <button type="button" class="toggle-btn" @click="toggleCustomTeacher()" :disabled="!filters.subject">
              {{ customTeacher ? 'Из списка' : 'Свой' }}
            </button>
          </div>
        </div>
        
        <div class="filter-item">
          <label for="semester-filter">Семестр:</label>
          <select id="semester-filter" v-model="filters.semester">
            <option :value="null">Все семестры</option>
            <option v-for="semester in semesters" :key="semester" :value="semester">{{ semester }}</option>
          </select>
        </div>
      </div>
      
      <div class="filter-actions">
        <button @click="searchMaterials" class="btn btn-search">Поиск</button>
      </div>
    </div>
    
    <div v-if="isLoading" class="loading-message">Загрузка материалов...</div>
    
    <div v-else-if="noResultsMessage" class="no-results-message">
      <p>{{ noResultsMessage }}</p>
      <button v-if="keyword || filters.course || filters.subject || filters.teacherName || filters.semester" 
              @click="resetFilters(); keyword = ''; searchMaterials();" 
              class="btn btn-reset-search">
        Сбросить все фильтры
      </button>
    </div>
    
    <div v-else class="materials-list">
      <div v-if="filteredMaterials.length === 0" class="no-materials">
        <p v-if="materials.length === 0">Нажмите кнопку Поиск для отображения материалов.</p>
        <p v-else>Нет материалов, соответствующих критериям фильтра. Попробуйте изменить фильтры.</p>
      </div>
      
      <div v-else class="material-cards">
        <Material v-for="material in filteredMaterials" :key="material.id" :material="material" @view-details="viewMaterialDetails" />
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
.materials-page { 
  max-width: 1000px; 
  margin: 0 auto; 
  display: flex; 
  flex-direction: column;
}

.filters-container { background-color: white; border-radius: 12px; padding: 20px; margin-bottom: 30px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }
.filter-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.filter-header h3 { margin: 0; color: #2d3748; }
.btn-reset { background-color: #e2e8f0; border: none; padding: 8px 12px; border-radius: 4px; font-size: 14px; color: #4a5568; cursor: pointer; transition: background-color 0.3s; }
.btn-reset:hover { background-color: #cbd5e0; }
.filters { display: flex; flex-wrap: wrap; gap: 20px; }
.filter-item { flex: 1; min-width: 200px; }
.filter-item label { display: block; margin-bottom: 8px; font-weight: bold; color: #4a5568; }
.custom-field-container { display: flex; align-items: center; gap: 10px; }
.custom-field-container select, .custom-field-container input { flex: 1; padding: 8px; border: 1px solid #cbd5e0; border-radius: 40
  px; font-size: 14px; }
.toggle-btn { background-color: #718096; color: white; border: none; border-radius: 4px; padding: 6px 10px; font-size: 12px; cursor: pointer; transition: background-color 0.3s; }
.toggle-btn:hover { background-color: #4a5568; }
.toggle-btn:disabled { background-color: #a0aec0; cursor: not-allowed; }
select { width: 100%; padding: 8px; border: 1px solid #cbd5e0; border-radius: 4px; font-size: 14px; }
select:disabled { background-color: #edf2f7; cursor: not-allowed; }
.loading-message { padding: 40px; text-align: center; background-color: #f7fafc; border-radius: 6px; color: #718096; }
.filter-actions { display: flex; justify-content: right; margin-top: 20px; }
.btn-search { background-color: #234866; color: white; padding: 10px 24px; font-size: 16px; }
.btn-search:hover { background-color: #225d94; }
.no-materials { padding: 40px; text-align: center; background-color: #f7fafc; border-radius: 6px; color: #718096; }
.material-cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; margin-top: 20px; }
.btn { padding: 8px 16px; border: none; border-radius: 12px; cursor: pointer; font-size: 14px; font-weight: bold; transition: background-color 0.3s; }
.success-message { background-color: #68d391; color: white; padding: 15px; border-radius: 6px; margin-bottom: 20px; text-align: center; animation: fadeIn 0.5s; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }

/* Стили для сообщения об отсутствии результатов */
.no-results-message {
  padding: 30px;
  text-align: center;
  background-color: #f7fafc;
  border-radius: 6px;
  color: #4a5568;
  margin-bottom: 20px;
  border: 1px solid #e2e8f0;
}

.no-results-message p {
  font-size: 18px;
  margin-bottom: 15px;
}

.btn-reset-search {
  background-color: #3182ce;
  color: white;
  padding: 10px 20px;
  border-radius: 6px;
  font-size: 14px;
  transition: background-color 0.3s;
}

.btn-reset-search:hover {
  background-color: #2b6cb0;
}

/* Стили для поисковой строки */
.search-container {
  display: flex;
  margin-bottom: 20px;
  gap: 10px;
}

.search-input-container {
  position: relative;
  flex: 1;
}

.search-input {
  width: 100%;
  padding: 12px 40px 12px 15px;
  border: 2px solid #e2e8f0;
  border-radius: 20px;
  font-size: 16px;
  transition: border-color 0.3s, box-shadow 0.3s;
}

.search-input:focus {
  outline: none;
  border-color: #4299e1;
  box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.2);
}

.clear-search-btn {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #a0aec0;
  font-size: 20px;
  font-weight: bold;
  cursor: pointer;
  padding: 5px;
  line-height: 1;
}

.clear-search-btn:hover {
  color: #718096;
}

.search-btn {
  background-color: #234866;
  color: white;
  padding: 12px 20px;
  font-size: 16px;
  border-radius: 13px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s;
}

.search-btn:hover {
  background-color: #225d94;
}
</style> 
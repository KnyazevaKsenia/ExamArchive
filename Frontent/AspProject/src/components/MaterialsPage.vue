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
      this.availableSubjects = course && this.selectionData[course]
        ? Object.keys(this.selectionData[course].subjects || {})
        : [];
    },
    updateAvailableTeachers() {
      const { course, subject } = this.filters;
      this.availableTeachers = (
        course &&
        subject &&
        this.selectionData[course]?.subjects[subject]?.teachers
      ) || [];
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
      this.filters = {
        course: null,
        subject: '',
        teacherName: '',
        semester: null
      };
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
  <div class="materials-page grid-background">
    <section class="library-hero page-container">
      <div>
        <div class="section-kicker">БИБЛИОТЕКА</div>
        <h1>найди нужный<br><span>материал</span></h1>
        <p>
          Методички, лекции, задания и другие учебные материалы
          в одном архиве.
        </p>
      </div>
    </section>

    <section class="page-container library-content">
      <div v-if="successMessage" class="success-message">
        {{ successMessage }}
      </div>

      <div class="search-panel">
        <div class="search-row">
          <div class="search-input-wrap">
            <span class="search-icon">⌕</span>
            <input
              v-model="keyword"
              type="text"
              class="search-input"
              placeholder="Поиск по ключевому слову..."
              @keyup.enter="searchByKeyword"
            />
            <button
              v-if="keyword"
              class="clear-search-btn"
              type="button"
              title="Очистить поиск"
              @click="clearKeywordSearch"
            >
              ×
            </button>
          </div>

          <button
            class="search-button"
            type="button"
            @click="searchByKeyword"
          >
            Найти
            <span>→</span>
          </button>
        </div>

        <div v-if="!isLoading" class="filters-panel">
          <div class="filters-heading">
            <div>
              <span class="filters-label">ФИЛЬТРЫ</span>
              <h2>Уточнить поиск</h2>
            </div>

            <button
              type="button"
              class="reset-button"
              @click="resetFilters"
            >
              Сбросить
            </button>
          </div>

          <div class="filters-grid">
            <div class="filter-item">
              <label for="course-filter">Курс</label>
              <select id="course-filter" v-model="filters.course">
                <option :value="null">Все курсы</option>
                <option
                  v-for="course in courses"
                  :key="course"
                  :value="course"
                >
                  {{ course }} курс
                </option>
              </select>
            </div>

            <div class="filter-item">
              <div class="filter-label-row">
                <label for="subject-filter">Предмет</label>

                <button
                  type="button"
                  class="mode-button"
                  :disabled="filters.course === null"
                  @click="toggleCustomSubject"
                >
                  {{ customSubject ? 'Выбрать из списка' : 'Ввести вручную' }}
                </button>
              </div>

              <select
                v-if="!customSubject"
                id="subject-filter"
                v-model="filters.subject"
                :disabled="filters.course === null"
              >
                <option value="">Все предметы</option>
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
                v-model="filters.subject"
                type="text"
                placeholder="Название предмета"
              />
            </div>

            <div class="filter-item">
              <div class="filter-label-row">
                <label for="teacher-filter">Преподаватель</label>

                <button
                  type="button"
                  class="mode-button"
                  :disabled="!filters.subject"
                  @click="toggleCustomTeacher"
                >
                  {{ customTeacher ? 'Выбрать из списка' : 'Ввести вручную' }}
                </button>
              </div>

              <select
                v-if="!customTeacher"
                id="teacher-filter"
                v-model="filters.teacherName"
                :disabled="!filters.subject"
              >
                <option value="">Все преподаватели</option>
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
                v-model="filters.teacherName"
                type="text"
                placeholder="Имя преподавателя"
              />
            </div>

            <div class="filter-item">
              <label for="semester-filter">Семестр</label>
              <select id="semester-filter" v-model="filters.semester">
                <option :value="null">Все семестры</option>
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

          <div class="filters-actions">
            <button
              class="apply-button"
              type="button"
              @click="searchMaterials"
            >
              Применить фильтры
              <span>→</span>
            </button>
          </div>
        </div>
      </div>

      <div class="results-heading">
        <div>
          <div class="section-kicker">РЕЗУЛЬТАТЫ</div>
          <h2>материалы</h2>
        </div>

        <span v-if="!isLoading && !noResultsMessage" class="results-count">
          {{ filteredMaterials.length }}
        </span>
      </div>

      <div v-if="isLoading" class="state-card">
        <div class="state-icon">…</div>
        <div>
          <strong>Загружаем материалы</strong>
          <p>Это займёт несколько секунд.</p>
        </div>
      </div>

      <div v-else-if="noResultsMessage" class="state-card">
        <div class="state-icon">⌕</div>
        <div>
          <strong>{{ noResultsMessage }}</strong>
          <p>Попробуй изменить запрос или параметры фильтрации.</p>

          <button
            v-if="keyword || filters.course || filters.subject || filters.teacherName || filters.semester"
            class="state-button"
            type="button"
            @click="resetFilters(); keyword = ''; searchMaterials();"
          >
            Сбросить поиск
          </button>
        </div>
      </div>

      <div v-else-if="filteredMaterials.length === 0" class="state-card">
        <div class="state-icon">□</div>
        <div>
          <strong>Материалов пока нет</strong>
          <p>Попробуй изменить параметры поиска.</p>
        </div>
      </div>

      <div v-else class="material-cards">
        <Material
          v-for="material in filteredMaterials"
          :key="material.id"
          :material="material"
          @view-details="viewMaterialDetails"
        />
      </div>
    </section>
  </div>
</template>

<style scoped>
.materials-page {
  min-height: 100vh;
  padding-bottom: 90px;
  color: var(--color-text, #161616);
}

.library-hero {
  padding-top: 62px;
  padding-bottom: 38px;
}

.section-kicker,
.filters-label {
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
  font-weight: 800;
  letter-spacing: 0.14em;
}

.library-hero h1 {
  margin: 10px 0 18px;
  font-size: clamp(56px, 7vw, 102px);
  font-weight: 900;
  line-height: 0.88;
  letter-spacing: -0.065em;
}

.library-hero h1 span {
  display: inline-block;
  margin-top: 8px;
  padding: 0 12px 7px;
  background: var(--color-blue, #cbe8f2);
  font-family: Georgia, 'Times New Roman', serif;
  font-weight: 400;
  transform: rotate(-1deg);
}

.library-hero p {
  max-width: 590px;
  margin: 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 17px;
  line-height: 1.55;
}

.library-content {
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.success-message {
  padding: 13px 16px;
  border: 1px solid #9eb38b;
  border-radius: 12px;
  background: var(--color-green, #dce8c6);
  font-size: 13px;
  font-weight: 700;
}

.search-panel {
  overflow: hidden;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 18px;
  background: rgba(255,255,255,.9);
  box-shadow: var(--shadow-card, 0 8px 30px rgba(24,24,20,.06));
}

.search-row {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 12px;
  padding: 18px;
  border-bottom: 1px solid var(--color-border, #d9d8d2);
}

.search-input-wrap {
  position: relative;
}

.search-icon {
  position: absolute;
  top: 50%;
  left: 15px;
  transform: translateY(-50%);
  color: var(--color-text-secondary, #64645f);
  font-size: 20px;
}

.search-input {
  width: 100%;
  min-height: 50px;
  padding: 0 44px 0 46px;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 999px;
  outline: none;
  background: var(--color-bg, #fbfaf6);
  color: var(--color-text, #161616);
  font-size: 15px;
}

.search-input:focus {
  border-color: #8fb9c9;
  box-shadow: 0 0 0 3px rgba(203,232,242,.55);
}

.clear-search-btn {
  position: absolute;
  top: 50%;
  right: 14px;
  width: 28px;
  height: 28px;
  transform: translateY(-50%);
  border: 0;
  border-radius: 50%;
  background: transparent;
  color: var(--color-text-secondary, #64645f);
  font-size: 20px;
}

.clear-search-btn:hover {
  background: var(--color-cream, #f4ead4);
}

.search-button,
.apply-button,
.state-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 24px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 999px;
  background: var(--color-text, #161616);
  color: #fff;
  font-weight: 800;
}

.search-button {
  min-width: 126px;
  min-height: 50px;
  padding: 0 20px;
}

.filters-panel {
  padding: 22px 18px 18px;
}

.filters-heading {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 20px;
  margin-bottom: 19px;
}

.filters-heading h2 {
  margin: 5px 0 0;
  font-size: 23px;
  letter-spacing: -0.035em;
}

.reset-button,
.mode-button {
  border: 0;
  background: transparent;
  color: var(--color-text-secondary, #64645f);
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
}

.reset-button:hover,
.mode-button:hover:not(:disabled) {
  color: var(--color-text, #161616);
  text-decoration: underline;
}

.mode-button:disabled {
  opacity: .4;
  cursor: default;
}

.filters-grid {
  display: grid;
  grid-template-columns: .7fr 1.4fr 1.4fr .7fr;
  gap: 13px;
}

.filter-item label {
  display: block;
  margin-bottom: 7px;
  color: var(--color-text, #161616);
  font-size: 12px;
  font-weight: 800;
}

.filter-label-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 10px;
}

.filter-item select,
.filter-item input {
  width: 100%;
  min-height: 44px;
  padding: 0 12px;
  border: 1px solid var(--color-border, #d9d8d2);
  border-radius: 11px;
  outline: none;
  background: var(--color-bg, #fbfaf6);
  color: var(--color-text, #161616);
  font-size: 13px;
}

.filter-item select:focus,
.filter-item input:focus {
  border-color: #8fb9c9;
  box-shadow: 0 0 0 3px rgba(203,232,242,.45);
}

.filter-item select:disabled {
  opacity: .55;
  cursor: not-allowed;
}

.filters-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 18px;
}

.apply-button {
  min-height: 43px;
  padding: 0 18px;
  font-size: 12px;
}

.results-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 20px;
  margin-top: 10px;
}

.results-heading h2 {
  margin: 6px 0 0;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 50px;
  font-weight: 400;
  line-height: .95;
  letter-spacing: -.04em;
}

.results-count {
  min-width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 50%;
  background: var(--color-green, #dce8c6);
  font-size: 13px;
  font-weight: 800;
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
}

.state-button {
  min-height: 37px;
  margin-top: 12px;
  padding: 0 14px;
  font-size: 11px;
}

.material-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
  gap: 18px;
}

@media (max-width: 1000px) {
  .filters-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 680px) {
  .library-hero {
    padding-top: 42px;
  }

  .library-hero h1 {
    font-size: clamp(48px, 15vw, 72px);
  }

  .search-row {
    grid-template-columns: 1fr;
  }

  .search-button {
    width: 100%;
  }

  .filters-grid {
    grid-template-columns: 1fr;
  }

  .filter-label-row {
    align-items: flex-start;
    flex-direction: column;
    gap: 2px;
  }

  .material-cards {
    grid-template-columns: 1fr;
  }
}
</style>

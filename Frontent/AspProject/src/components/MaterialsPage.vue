<script>
import { defineComponent } from 'vue'
import axios from 'axios'
import Material from './Material.vue'
import { API_ENDPOINTS } from '../config'
import { isAuthenticated } from '../utils/auth'

export default defineComponent({
  name: 'MaterialsPage',
  components: { Material },
  data() {
    return {
      materials: [], filteredMaterials: [], selectionData: {},
      filters: { course: null, subject: '', teacherName: '', semester: null },
      keyword: '', courses: [1,2,3,4], semesters: [1,2], availableSubjects: [], availableTeachers: [],
      isLoading: true, customSubject: false, customTeacher: false, successMessage: '', noResultsMessage: '', isAuthenticated: false
    }
  },
  mounted() {
    this.isAuthenticated = isAuthenticated()
    this.fetchSelectionData()
    this.fetchMaterials()
    this.$root.$on('material-added', () => { this.successMessage = 'Материал успешно добавлен'; this.searchMaterials() })
  },
  beforeDestroy() { this.$root.$off('material-added') },
  watch: {
    'filters.course'(course) { this.filters.subject=''; this.filters.teacherName=''; this.availableSubjects = course ? Object.keys(this.selectionData[course]?.subjects || {}) : [] },
    'filters.subject'(subject) { this.filters.teacherName=''; this.availableTeachers = subject ? (this.selectionData[this.filters.course]?.subjects?.[subject]?.teachers || []) : [] }
  },
  methods: {
    normalize(material) {
      return {
        id: material.id || material.Id,
        description: material.description || material.Description,
        course: material.course || material.Course,
        subject: material.subject || material.Subject,
        teacherName: material.teacherName || material.teacher || material.TeacherName || material.Teacher,
        semester: material.semester || material.Semester,
        date: material.createdAt || material.CreatedAt,
        imagesNameUrl: material.imagesNameUrl || material.ImagesNameUrl || {},
        fileNameUrl: material.fileNameUrl || material.FileNameUrl || {}
      }
    },
    async fetchSelectionData() {
      try { this.selectionData = (await axios.get(API_ENDPOINTS.SELECTION_LIST)).data || {} } catch (e) { console.error(e) }
    },
    async fetchMaterials() { await this.searchMaterials() },
    async searchMaterials() {
      this.isLoading = true; this.noResultsMessage = ''
      try {
        const response = await axios.get(API_ENDPOINTS.GET_MATERIALS, { params: {
          course: this.filters.course || 0,
          subject: this.filters.subject || null,
          teacherName: this.filters.teacherName || null,
          semester: this.filters.semester || 0
        }})
        this.materials = (response.data || []).map(this.normalize)
        this.filteredMaterials = [...this.materials]
        if (!this.materials.length) this.noResultsMessage = 'По вашему запросу ничего не найдено.'
      } catch (e) {
        console.error(e); this.materials=[]; this.filteredMaterials=[]; this.noResultsMessage='Не удалось загрузить материалы.'
      } finally { this.isLoading = false }
    },
    async searchByKeyword() {
      if (!this.keyword.trim()) return this.searchMaterials()
      this.isLoading = true; this.noResultsMessage=''
      try {
        const response = await axios.get(`${API_ENDPOINTS.SEARCH_BY_KEYWORD}/${encodeURIComponent(this.keyword.trim())}`)
        this.materials = (response.data || []).map(this.normalize)
        this.filteredMaterials = [...this.materials]
        if (!this.materials.length) this.noResultsMessage = `По запросу «${this.keyword}» ничего не найдено.`
      } catch (e) { this.noResultsMessage='Ошибка поиска. Попробуйте ещё раз.'; this.filteredMaterials=[] }
      finally { this.isLoading=false }
    },
    clearKeywordSearch() { this.keyword=''; this.searchMaterials() },
    toggleCustomSubject(){ this.customSubject=!this.customSubject; this.filters.subject='' },
    toggleCustomTeacher(){ this.customTeacher=!this.customTeacher; this.filters.teacherName='' },
    resetFilters(){ this.filters={course:null,subject:'',teacherName:'',semester:null}; this.customSubject=false; this.customTeacher=false; this.availableSubjects=[]; this.availableTeachers=[]; this.searchMaterials() },
    viewMaterialDetails(id){ this.$router.push(`/materials/${id}`) }
  }
})
</script>

<template>
  <div class="materials-page">
    <section class="inner-hero page-container">
      <div><span class="eyebrow">БИБЛИОТЕКА АРХИВА</span><h1 class="serif">знания под рукой.</h1><p>Методички, лекции и конспекты твоей программы. Ищи нужное и сохраняй важное, чтобы вернуться позже.</p></div>
      <div class="inner-hero-image"><img src="/assets/books.jpg" alt=""><span>учись в своём ритме ✳</span></div>
    </section>

    <section class="page-container library-body">
      <div v-if="successMessage" class="success-message">{{ successMessage }}</div>
      <div class="page-toolbar">
        <label class="page-search"><span>⌕</span><input v-model="keyword" placeholder="Найти материал или предмет" @keyup.enter="searchByKeyword"><button v-if="keyword" type="button" @click="clearKeywordSearch">×</button></label>
        <button class="pill-button pill-dark search-submit" @click="searchByKeyword">Найти →</button>
        <span v-if="!isLoading">{{ filteredMaterials.length }} материалов</span>
      </div>

      <div class="filters">
        <div class="filter"><label>Курс</label><select v-model="filters.course"><option :value="null">Все курсы</option><option v-for="course in courses" :key="course" :value="course">{{ course }} курс</option></select></div>
        <div class="filter"><div class="filter-top"><label>Предмет</label><button @click="toggleCustomSubject" :disabled="filters.course===null">{{ customSubject?'Из списка':'Ввести' }}</button></div><select v-if="!customSubject" v-model="filters.subject" :disabled="filters.course===null"><option value="">Все предметы</option><option v-for="subject in availableSubjects" :key="subject">{{ subject }}</option></select><input v-else v-model="filters.subject" placeholder="Название предмета"></div>
        <div class="filter"><div class="filter-top"><label>Преподаватель</label><button @click="toggleCustomTeacher" :disabled="!filters.subject">{{ customTeacher?'Из списка':'Ввести' }}</button></div><select v-if="!customTeacher" v-model="filters.teacherName" :disabled="!filters.subject"><option value="">Все преподаватели</option><option v-for="teacher in availableTeachers" :key="teacher">{{ teacher }}</option></select><input v-else v-model="filters.teacherName" placeholder="Имя преподавателя"></div>
        <div class="filter"><label>Семестр</label><select v-model="filters.semester"><option :value="null">Все семестры</option><option v-for="semester in semesters" :key="semester" :value="semester">{{ semester }} семестр</option></select></div>
        <button class="apply" @click="searchMaterials">Применить</button><button class="reset" @click="resetFilters">Сбросить ×</button>
      </div>

      <div class="section-title-row"><div><span class="eyebrow">ПОДБОРКА ДЛЯ ТЕБЯ</span><h2 class="serif">материалы архива</h2></div><span>ИТИС · Программная инженерия</span></div>

      <div v-if="isLoading" class="page-empty"><div class="loader">•••</div><h3>Загружаем материалы</h3><p>Получаем данные из архива.</p></div>
      <div v-else-if="noResultsMessage" class="page-empty"><span class="empty-icon">⌕</span><h3>{{ noResultsMessage }}</h3><p>Попробуй изменить запрос или параметры фильтрации.</p><button class="pill-button" @click="resetFilters">Сбросить фильтры</button></div>
      <div v-else class="library-grid"><Material v-for="material in filteredMaterials" :key="material.id" :material="material" @view-details="viewMaterialDetails" /></div>
    </section>
  </div>
</template>

<style scoped>
.materials-page{padding:26px 0 80px}.inner-hero{min-height:268px;background:#e0f0f4;border:1px solid #c9e1e6;border-radius:17px;display:flex;align-items:center;justify-content:space-between;gap:30px;padding:40px clamp(24px,5vw,70px);position:relative;overflow:hidden}.inner-hero:after{content:'✳';position:absolute;top:19px;left:49%;font:30px Georgia}.inner-hero>div:first-child{max-width:630px}.inner-hero h1{font-size:clamp(42px,5.7vw,78px);line-height:1.03;margin:10px 0 14px}.inner-hero p{font-size:15px;line-height:1.45;max-width:520px;margin:0}.inner-hero-image{width:240px;height:198px;flex:none;padding:7px 7px 28px;background:#fffefa;transform:rotate(7deg);box-shadow:4px 10px 20px #1c3b4430}.inner-hero-image img{width:100%;height:100%;object-fit:cover}.inner-hero-image span{display:block;font:italic 13px Georgia;padding:4px 2px}.library-body{padding-top:32px}.success-message{margin-bottom:14px;padding:12px 15px;background:var(--color-green);border-radius:10px;font-size:12px;font-weight:700}.page-toolbar{display:flex;align-items:center;gap:12px}.page-toolbar>span{margin-left:auto;color:#777;font-size:12px}.page-search{background:white;border:1px solid #dbded8;border-radius:30px;height:48px;width:min(480px,100%);display:flex;align-items:center;gap:10px;padding:0 17px}.page-search input{border:0;outline:0;flex:1;min-width:0;background:transparent;font-size:13px}.page-search button{border:0;background:transparent;font-size:18px}.search-submit{height:48px}.filters{display:flex;align-items:end;gap:8px;flex-wrap:wrap;margin-top:12px;padding:13px;border:1px solid #e3e5e0;background:#ffffff8c;border-radius:14px}.filter{display:grid;gap:5px;min-width:145px;flex:1}.filter label{font-size:9px;font-weight:800;text-transform:uppercase;color:#777}.filter select,.filter input{height:38px;border:1px solid var(--color-border);border-radius:20px;background:white;padding:0 12px;font-size:11px;min-width:0}.filter-top{display:flex;justify-content:space-between;align-items:center}.filter-top button{border:0;background:transparent;font-size:9px;text-decoration:underline}.apply,.reset{height:38px;border-radius:20px;padding:0 14px;font-size:11px;font-weight:700}.apply{border:0;background:#252729;color:white}.reset{border:0;background:transparent}.section-title-row{display:flex;justify-content:space-between;align-items:end;margin:34px 0 17px}.section-title-row h2{font-size:clamp(34px,4vw,53px);margin:1px 0 0}.section-title-row>span{font-size:12px;color:#777}.library-grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:15px}.page-empty{min-height:270px;border:1px dashed #cbd4d0;background:#ffffff9c;border-radius:14px;display:flex;flex-direction:column;justify-content:center;align-items:center;text-align:center;padding:30px}.page-empty h3{font:600 29px 'Playfair Display',Georgia,serif;margin:13px 0 3px}.page-empty p{color:#666;font-size:13px}.empty-icon{font-size:36px}.loader{letter-spacing:4px;font-size:24px}@media(max-width:900px){.library-grid{grid-template-columns:repeat(2,minmax(0,1fr))}.inner-hero-image{width:190px;height:165px}}@media(max-width:700px){.materials-page{padding-top:12px}.inner-hero{min-height:240px;padding:30px 23px}.inner-hero-image{display:none}.page-toolbar{flex-wrap:wrap}.page-search{width:100%}.page-toolbar>span{margin-left:0}.filters{align-items:stretch}.filter{min-width:calc(50% - 8px)}.section-title-row>span{display:none}}@media(max-width:420px){.library-grid{grid-template-columns:1fr}.filter{min-width:100%}}
</style>

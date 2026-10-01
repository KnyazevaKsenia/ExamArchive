<script>
import axios from 'axios'
import { API_ENDPOINTS } from '../config'
import { isAuthenticated } from '../utils/auth'

export default {
  name: 'Material',
  props: { material: { type: Object, required: true } },
  data: () => ({ isLiked: false, isLiking: false }),
  computed: {
    m() { const x=this.material; return { id:x.id||x.Id||x.MaterialId, description:x.description||x.Description||'', course:x.course||x.Course, subject:x.subject||x.Subject||'Без предмета', teacherName:x.teacherName||x.teacher||x.TeacherName||x.Teacher||'', semester:x.semester||x.Semester, date:x.date||x.CreatedAt||x.createdAt, imagesNameUrl:x.imagesNameUrl||x.ImagesNameUrl||{}, fileNameUrl:x.fileNameUrl||x.FileNameUrl||{} } },
    filesCount(){ return Object.keys(this.m.fileNameUrl||{}).length },
    imagesCount(){ return Object.keys(this.m.imagesNameUrl||{}).length },
    description(){ return this.m.description.length>105 ? this.m.description.slice(0,105)+'…' : this.m.description },
    kind(){ if(this.filesCount) return 'Материал'; if(this.imagesCount) return 'Изображения'; return 'Учебный материал' },
    cover(){ const s=(this.m.subject||'').toLowerCase(); if(s.includes('баз'))return'/assets/books.jpg'; if(s.includes('маш')||s.includes('програм'))return'/assets/code.jpg'; return'/assets/notes.jpg' }
  },
  mounted(){ if(isAuthenticated()) this.checkIfLiked() },
  methods:{
    async checkIfLiked(){ try{ const r=await axios.get(API_ENDPOINTS.GET_STUDENT_FAVORITES_IDS); this.isLiked=Array.isArray(r.data)&&r.data.includes(this.m.id) }catch(e){console.error(e)} },
    async toggleLike(){ if(!isAuthenticated()){ this.$notify?.error('Войдите, чтобы добавлять материалы в избранное'); return } if(this.isLiking)return; this.isLiking=true; try{ if(this.isLiked){ await axios.get(`${API_ENDPOINTS.REMOVE_FAVORITE}/${this.m.id}`); this.isLiked=false }else{ await axios.post(`${API_ENDPOINTS.ADD_FAVORITE}/${this.m.id}`); this.isLiked=true } }catch(e){ this.$notify?.error('Не удалось изменить избранное') }finally{this.isLiking=false} },
    open(){ this.$emit('view-details',this.m.id) }
  }
}
</script>

<template>
  <article class="library-card" @click="open">
    <div class="library-card-image"><img :src="cover" alt=""><span>{{ kind }}</span></div>
    <div class="library-card-body">
      <span class="eyebrow">{{ (m.subject || '').toUpperCase() }}</span>
      <h3>{{ m.description || m.subject }}</h3>
      <p>{{ m.teacherName || 'Преподаватель не указан' }} · {{ m.course ? `${m.course} курс` : 'курс не указан' }}<br>ИТИС · Программная инженерия</p>
      <div class="card-footer">
        <button class="save-button" :class="{'is-saved':isLiked}" :disabled="isLiking" @click.stop="toggleLike">{{ isLiked ? '♥ Сохранено' : '♡ В избранное' }}</button>
        <button class="open-button" @click.stop="open">→</button>
      </div>
    </div>
  </article>
</template>

<style scoped>
.library-card{background:#fff;border:1px solid #e0e2de;border-radius:13px;overflow:hidden;display:flex;flex-direction:column;min-width:0;transition:.2s}.library-card:hover{transform:translateY(-3px);box-shadow:var(--shadow-card)}.library-card-image{height:160px;position:relative;overflow:hidden;background:#e5eee7}.library-card-image img{width:100%;height:100%;object-fit:cover}.library-card-image span{position:absolute;left:11px;bottom:11px;padding:6px 10px;border-radius:15px;background:#fffef4;font-size:10px;font-weight:700}.library-card-body{padding:18px;display:flex;flex-direction:column;align-items:start;flex:1}.library-card-body .eyebrow{font-size:9px}.library-card-body h3{font-size:18px;letter-spacing:-.04em;margin:7px 0;word-break:break-word;line-height:1.16}.library-card-body p{color:#717171;font-size:11px;line-height:1.35;margin:0 0 20px}.card-footer{margin-top:auto;width:100%;display:flex;justify-content:space-between;gap:8px;align-items:center}.save-button{border:1px solid #d6dad8;border-radius:20px;background:white;padding:8px 11px;font-size:11px;font-weight:700}.save-button.is-saved{background:#e8f0da;border-color:#d7e4bf}.open-button{width:34px;height:34px;border:1px solid #ddd;border-radius:50%;background:white}.save-button:disabled{opacity:.55}@media(max-width:420px){.library-card-image{height:145px}.library-card-body{padding:13px}}
</style>

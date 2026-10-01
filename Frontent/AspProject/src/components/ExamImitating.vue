<script>
import { defineComponent } from 'vue';
import axios from 'axios';

export default defineComponent({
  name: 'ExamImitating',
  data() {
    return {
      ticketSets: [],
      loading: false,
      error: null,
      showCreateForm: false,
      showTicketsModal: false,
      selectedSet: null,
      activeTicket: null,
      timerInterval: null,
      remainingTime: 0,
      newSet: {
        subject: '',
        hours: 1,
        minutes: 0,
        questionsText: ''
      }
    };
  },
  mounted() {
    this.fetchTicketSets();
  },
  beforeDestroy() {
    this.stopTimer();
  },
  methods: {
    fetchTicketSets() {
      this.loading = true;
      this.error = null;
      axios.get('/examImitation/getTicketsSets')
        .then(response => {
          this.ticketSets = response.data;
        })
        .catch(error => {
          this.error = 'Не удалось загрузить наборы билетов';
          console.error('Ошибка при загрузке наборов билетов:', error);
        })
        .finally(() => {
          this.loading = false;
        });
    },
    deleteTicketSet(id, event) {
      event.stopPropagation();
      if (confirm('Вы уверены, что хотите удалить этот набор билетов?')) {
        this.loading = true;
        axios.get(`/examImitation/delete-ticket-set/${id}`)
          .then(() => this.fetchTicketSets())
          .catch(error => {
            this.error = 'Не удалось удалить набор билетов';
            console.error('Ошибка при удалении набора билетов:', error);
          })
          .finally(() => {
            this.loading = false;
          });
      }
    },
    toggleCreateForm() {
      this.showCreateForm = !this.showCreateForm;
      this.error = null;
      if (!this.showCreateForm) this.resetNewSet();
    },
    resetNewSet() {
      this.newSet = { subject: '', hours: 1, minutes: 0, questionsText: '' };
    },
    formatDuration(hours, minutes) {
      return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    },
    parseQuestions(questionsText) {
      if (!questionsText.trim()) return [];
      return questionsText.split('\n').map(q => q.trim()).filter(q => q.length > 0);
    },
    createTicketSet() {
      if (!this.newSet.subject.trim()) {
        this.error = 'Пожалуйста, укажите предмет для набора билетов';
        return;
      }
      if (!this.newSet.questionsText.trim()) {
        this.error = 'Пожалуйста, введите хотя бы один вопрос';
        return;
      }
      const questions = this.parseQuestions(this.newSet.questionsText);
      if (questions.length === 0) {
        this.error = 'Пожалуйста, введите хотя бы один корректный вопрос';
        return;
      }
      const ticketSetData = {
        subjectName: this.newSet.subject,
        duration: this.formatDuration(this.newSet.hours, this.newSet.minutes),
        questions
      };
      this.loading = true;
      this.error = null;
      axios.post('/examImitation/createTicketSet', ticketSetData)
        .then(() => {
          this.toggleCreateForm();
          this.fetchTicketSets();
        })
        .catch(error => {
          this.error = 'Не удалось создать набор билетов';
          console.error('Ошибка при создании набора билетов:', error);
        })
        .finally(() => {
          this.loading = false;
        });
    },
    openTicketsModal(set) {
      this.selectedSet = set;
      this.showTicketsModal = true;
      this.activeTicket = null;
      this.stopTimer();
    },
    closeTicketsModal() {
      this.showTicketsModal = false;
      this.selectedSet = null;
      this.activeTicket = null;
      this.stopTimer();
    },
    showTicket(ticket) {
      this.activeTicket = ticket;
      this.startTimer();
    },
    backToTickets() {
      this.activeTicket = null;
      this.stopTimer();
    },
    startTimer() {
      this.stopTimer();
      const [hours, minutes] = this.selectedSet.duration.split(':').map(Number);
      this.remainingTime = hours * 3600 + minutes * 60;
      this.timerInterval = setInterval(() => {
        if (this.remainingTime > 0) {
          this.remainingTime--;
        } else {
          this.stopTimer();
          alert('Время истекло!');
        }
      }, 1000);
    },
    stopTimer() {
      if (this.timerInterval) {
        clearInterval(this.timerInterval);
        this.timerInterval = null;
      }
    },
    formatTime(seconds) {
      const hours = Math.floor(seconds / 3600);
      const minutes = Math.floor((seconds % 3600) / 60);
      const secs = seconds % 60;
      return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
  }
});
</script>

<template>
  <div class="exam-page grid-background">
    <section class="page-container exam-hero">
      <div class="eyebrow">ПОДГОТОВКА К ЭКЗАМЕНУ</div>
      <h1>экзамен?<br><em>ты справишься.</em></h1>
      <p>Создавай наборы вопросов преподавателей и тренируйся по билетам в своём темпе.</p>
      <button class="primary-btn hero-create" type="button" @click="toggleCreateForm">
        {{ showCreateForm ? 'Отменить создание' : 'Создать набор вопросов' }} <span>{{ showCreateForm ? '×' : '+' }}</span>
      </button>
    </section>

    <section class="page-container body-section">
      <div v-if="error" role="alert" class="error-panel">{{ error }}</div>

      <div v-if="showCreateForm" class="create-panel">
        <div class="panel-heading">
          <div class="eyebrow">НОВЫЙ НАБОР</div>
          <h2>вопросы к экзамену</h2>
          <p>Введи список вопросов: каждый вопрос с новой строки.</p>
        </div>
        <form @submit.prevent="createTicketSet">
          <div class="field">
            <label for="exam-subject">Предмет</label>
            <input id="exam-subject" v-model="newSet.subject" type="text" placeholder="Название предмета" required>
          </div>
          <div class="field">
            <label>Время на билет</label>
            <div class="duration-row">
              <div><input v-model.number="newSet.hours" aria-label="Часы" type="number" min="0" max="23"><small>часов</small></div>
              <div><input v-model.number="newSet.minutes" aria-label="Минуты" type="number" min="0" max="59"><small>минут</small></div>
            </div>
          </div>
          <div class="field">
            <label for="exam-questions">Экзаменационные вопросы</label>
            <textarea id="exam-questions" v-model="newSet.questionsText" rows="9" placeholder="Вопрос 1&#10;Вопрос 2&#10;Вопрос 3" required></textarea>
          </div>
          <div class="form-actions">
            <button class="secondary-btn" type="button" @click="toggleCreateForm">Отмена</button>
            <button class="primary-btn" type="submit" :disabled="loading">{{ loading ? 'Сохраняем…' : 'Создать набор' }} →</button>
          </div>
        </form>
      </div>

      <div class="section-title">
        <div><div class="eyebrow">ТВОЯ ПОДГОТОВКА</div><h2>наборы вопросов</h2></div>
        <span v-if="!loading && !error" class="count">{{ ticketSets.length }}</span>
      </div>

      <div v-if="loading" class="empty-state">Загружаем наборы вопросов…</div>
      <div v-else-if="!error && ticketSets.length === 0" class="empty-state">
        <div class="empty-icon">✳</div>
        <h3>пока нет наборов</h3>
        <p>Добавь вопросы своего преподавателя, чтобы начать подготовку.</p>
        <button class="secondary-btn" type="button" @click="showCreateForm = true">Создать первый набор →</button>
      </div>
      <div v-else-if="!error" class="sets-grid">
        <article v-for="set in ticketSets" :key="set.id" class="set-card" @click="openTicketsModal(set)">
          <div class="card-top"><span class="tag">ЭКЗАМЕН</span><button class="delete-btn" type="button" title="Удалить набор" @click="deleteTicketSet(set.id, $event)">×</button></div>
          <h3>{{ set.subject }}</h3>
          <div class="set-meta"><span>{{ set.tickets ? set.tickets.length : 0 }} билетов</span><span>{{ set.duration }}</span></div>
          <button class="open-btn" type="button">Открыть набор <span>→</span></button>
        </article>
      </div>
    </section>

    <div v-if="showTicketsModal && selectedSet" class="modal-backdrop" @click.self="closeTicketsModal">
      <div class="tickets-modal" role="dialog" aria-modal="true" :aria-label="selectedSet.subject">
        <div class="modal-header">
          <div><div class="eyebrow">ТРЕНИРОВКА</div><h2>{{ selectedSet.subject }}</h2></div>
          <button type="button" class="close-btn" aria-label="Закрыть" @click="closeTicketsModal">×</button>
        </div>
        <div v-if="activeTicket" class="timer-row"><span>Время осталось</span><strong>{{ formatTime(remainingTime) }}</strong></div>
        <div v-if="!activeTicket" class="modal-body">
          <p class="modal-intro">Выбери билет, чтобы начать тренировку.</p>
          <div class="tickets-grid"><button v-for="(ticket, index) in selectedSet.tickets" :key="index" class="ticket-btn" type="button" @click="showTicket(ticket)"><small>БИЛЕТ</small>{{ index + 1 }}</button></div>
        </div>
        <div v-else class="modal-body">
          <div class="eyebrow">ВОПРОСЫ БИЛЕТА</div>
          <h3 class="active-heading">Билет {{ selectedSet.tickets.indexOf(activeTicket) + 1 }}</h3>
          <div class="question-card"><small>ВОПРОС 1</small><p>{{ activeTicket.firstQuestion }}</p></div>
          <div class="question-card"><small>ВОПРОС 2</small><p>{{ activeTicket.secondQuestion }}</p></div>
          <button class="secondary-btn" type="button" @click="backToTickets">← К списку билетов</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.exam-page { min-height:100vh; padding-bottom:96px; color:var(--color-text,#161616); }
.page-container { width:min(calc(100% - 64px),var(--page-width,1440px)); margin:0 auto; }
.exam-hero { padding-top:62px; padding-bottom:60px; }
.eyebrow { color:var(--color-text-secondary,#64645f); font-size:10px; font-weight:800; letter-spacing:.14em; }
.exam-hero h1 { margin:12px 0 22px; font-size:clamp(56px,7vw,100px); font-weight:900; line-height:.91; letter-spacing:-.065em; }
.exam-hero h1 em { display:inline-block; padding:0 14px 8px; background:var(--color-green,#dce8c6); font-family:Georgia,serif; font-weight:400; font-style:normal; transform:rotate(-1deg); }
.exam-hero p { max-width:600px; color:var(--color-text-secondary,#64645f); font-size:17px; line-height:1.55; margin:0 0 25px; }
.primary-btn,.secondary-btn { min-height:44px; display:inline-flex; align-items:center; justify-content:center; gap:20px; padding:0 18px; border:1px solid #202020; border-radius:999px; font-size:13px; font-weight:800; cursor:pointer; }
.primary-btn { background:#161616; color:#fff; } .secondary-btn { background:#fff; color:#161616; } button:disabled { opacity:.6; cursor:wait; }
.body-section { padding-top:35px; border-top:1px solid var(--color-border,#d9d8d2); }
.error-panel { padding:15px 18px; margin-bottom:24px; background:#f7e4e1; border:1px solid #d9a9a1; border-radius:12px; color:#922b22; }
.create-panel { padding:30px; margin-bottom:45px; border:1px solid #202020; border-radius:20px; background:#fff; box-shadow:var(--shadow-card,0 8px 30px rgba(24,24,20,.06)); }
.panel-heading h2,.section-title h2 { margin:7px 0 0; font:normal clamp(34px,4.5vw,54px)/1 Georgia,serif; letter-spacing:-.04em; }
.panel-heading p { margin:12px 0 28px; color:#64645f; font-size:14px; }
.field { margin-bottom:20px; } .field label { display:block; margin-bottom:8px; font-size:13px; font-weight:800; }
.field input,.field textarea { width:100%; min-height:45px; padding:12px 14px; border:1px solid #d9d8d2; border-radius:12px; background:#fbfaf6; font:inherit; font-size:14px; outline:none; }
.field input:focus,.field textarea:focus { border-color:#8fb9c9; box-shadow:0 0 0 3px #cbe8f2; }
.field textarea { resize:vertical; line-height:1.5; }
.duration-row { display:flex; gap:14px; } .duration-row > div { max-width:130px; } .duration-row small { display:block; margin-top:6px; color:#64645f; }
.form-actions { display:flex; justify-content:flex-end; gap:10px; }
.section-title { display:flex; align-items:flex-end; justify-content:space-between; margin-bottom:25px; }
.count { width:42px; height:42px; display:grid; place-items:center; border:1px solid #202020; border-radius:50%; background:var(--color-blue,#cbe8f2); font-weight:800; }
.empty-state { padding:60px 24px; text-align:center; background:#fff; border:1px dashed #202020; border-radius:18px; color:#64645f; }
.empty-icon { font-size:38px; color:#76977b; } .empty-state h3 { margin:12px 0; font:normal 38px Georgia,serif; color:#161616; } .empty-state p { margin:0 0 22px; }
.sets-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(290px,1fr)); gap:18px; }
.set-card { min-height:250px; padding:23px; display:flex; flex-direction:column; background:#fff; border:1px solid #202020; border-radius:16px; cursor:pointer; transition:transform .18s,box-shadow .18s; }
.set-card:hover { transform:translateY(-3px); box-shadow:var(--shadow-card,0 8px 30px rgba(24,24,20,.06)); }
.card-top { display:flex; justify-content:space-between; align-items:center; }
.tag { padding:8px 10px; border-radius:999px; background:var(--color-blue,#cbe8f2); font-size:10px; font-weight:800; letter-spacing:.08em; }
.delete-btn { width:34px; height:34px; border:1px solid #d9d8d2; border-radius:50%; background:#fff; font-size:22px; cursor:pointer; }
.set-card h3 { margin:29px 0 15px; font-size:24px; letter-spacing:-.03em; overflow-wrap:anywhere; }
.set-meta { display:flex; flex-wrap:wrap; gap:12px; margin-bottom:22px; color:#64645f; font-size:12px; }
.open-btn { width:100%; min-height:42px; margin-top:auto; display:flex; justify-content:space-between; align-items:center; padding:0 14px; background:#161616; color:#fff; border:0; border-radius:999px; font-weight:800; cursor:pointer; }
.modal-backdrop { position:fixed; inset:0; z-index:2000; display:flex; align-items:center; justify-content:center; padding:20px; background:rgba(18,18,17,.48); }
.tickets-modal { width:min(730px,100%); max-height:90vh; overflow-y:auto; background:#fbfaf6; border:1px solid #202020; border-radius:20px; }
.modal-header { display:flex; align-items:flex-start; justify-content:space-between; gap:20px; padding:25px 28px; border-bottom:1px solid #d9d8d2; }
.modal-header h2 { margin:6px 0 0; font-size:28px; letter-spacing:-.04em; }
.close-btn { width:37px; height:37px; flex:0 0 37px; background:#fff; border:1px solid #d9d8d2; border-radius:50%; font-size:23px; cursor:pointer; }
.modal-body { padding:25px 28px 30px; }
.modal-intro { margin:0 0 20px; color:#64645f; }
.tickets-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(110px,1fr)); gap:12px; }
.ticket-btn { min-height:100px; display:flex; flex-direction:column; justify-content:center; align-items:center; gap:7px; border:1px solid #202020; border-radius:13px; background:var(--color-blue,#cbe8f2); font-size:30px; font-weight:800; cursor:pointer; }
.ticket-btn:hover { background:var(--color-green,#dce8c6); }
.ticket-btn small,.question-card small { font-size:9px; font-weight:800; letter-spacing:.12em; }
.timer-row { display:flex; align-items:center; justify-content:space-between; padding:12px 28px; background:var(--color-green,#dce8c6); font-size:12px; font-weight:800; }
.timer-row strong { font-size:21px; font-variant-numeric:tabular-nums; }
.active-heading { margin:8px 0 24px; font:normal 40px Georgia,serif; }
.question-card { padding:20px; margin-bottom:12px; border:1px solid #d9d8d2; border-radius:13px; background:#fff; }
.question-card p { margin:9px 0 0; line-height:1.6; white-space:pre-line; }
.question-card + .secondary-btn { margin-top:17px; }
@media(max-width:680px) { .page-container { width:calc(100% - 32px); } .exam-hero { padding-top:42px; } .exam-hero h1 { font-size:clamp(46px,12vw,72px); } .create-panel { padding:19px; } .form-actions { flex-direction:column-reverse; } .modal-body,.modal-header { padding-left:18px; padding-right:18px; } }
</style>

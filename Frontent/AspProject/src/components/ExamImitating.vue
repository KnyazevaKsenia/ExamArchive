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
  methods: {
    fetchTicketSets() {
      this.loading = true;
      axios.get('/examImitation/getTicketsSets')
        .then(response => {
          this.ticketSets = response.data;
          this.loading = false;
        })
        .catch(error => {
          this.error = 'Не удалось загрузить наборы билетов';
          this.loading = false;
          console.error('Ошибка при загрузке наборов билетов:', error);
        });
    },
    deleteTicketSet(id, event) {
      // Предотвращаем всплытие события клика, чтобы не открывалось модальное окно
      event.stopPropagation();
      
      if (confirm('Вы уверены, что хотите удалить этот набор билетов?')) {
        this.loading = true;
        axios.get(`/examImitation/delete-ticket-set/${id}`)
          .then(() => {
            this.fetchTicketSets();
            this.loading = false;
          })
          .catch(error => {
            this.error = 'Не удалось удалить набор билетов';
            this.loading = false;
            console.error('Ошибка при удалении набора билетов:', error);
          });
      }
    },
    toggleCreateForm() {
      this.showCreateForm = !this.showCreateForm;
      if (!this.showCreateForm) {
        this.resetNewSet();
      }
    },
    resetNewSet() {
      this.newSet = {
        subject: '',
        hours: 1,
        minutes: 0,
        questionsText: ''
      };
    },
    formatDuration(hours, minutes) {
      const formattedHours = hours.toString().padStart(2, '0');
      const formattedMinutes = minutes.toString().padStart(2, '0');
      return `${formattedHours}:${formattedMinutes}`;
    },
    parseQuestions(questionsText) {
      if (!questionsText.trim()) {
        return [];
      }
      
      // Разделение по строкам и фильтрация пустых строк
      return questionsText.split('\n')
        .map(q => q.trim())
        .filter(q => q.length > 0);
    },
    createTicketSet() {
      if (!this.newSet.subject) {
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
        questions: questions
      };

      this.loading = true;
      axios.post('/examImitation/createTicketSet', ticketSetData)
        .then(() => {
          this.fetchTicketSets();
          this.toggleCreateForm();
          this.loading = false;
        })
        .catch(error => {
          this.error = 'Не удалось создать набор билетов';
          this.loading = false;
          console.error('Ошибка при создании набора билетов:', error);
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
    startTimer() {
      // Остановить предыдущий таймер, если он был запущен
      this.stopTimer();
      
      // Парсим строку продолжительности (формат "HH:MM")
      const [hours, minutes] = this.selectedSet.duration.split(':').map(Number);
      
      // Вычисляем общее время в секундах
      this.remainingTime = hours * 3600 + minutes * 60;
      
      // Запускаем таймер, обновляя каждую секунду
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
  <div class="exam-imitating">
    <div class="tittle">
        <h2>Наборы экзаменационных билетов</h2>
    </div>
    
    <div v-if="error" class="error-message">{{ error }}</div>
    
    

    <div v-if="showCreateForm" class="create-form">
      <h3>Создание нового набора билетов</h3>
      
      <div class="form-group">
        <label>Предмет:</label>
        <input 
          type="text" 
          v-model="newSet.subject" 
          placeholder="Введите название предмета"
          class="form-control"
        />
      </div>
      
      <div class="form-group">
        <label>Продолжительность:</label>
        <div class="duration-inputs">
          <div class="duration-input">
            <input 
              type="number" 
              v-model="newSet.hours" 
              min="0"
              max="23"
              class="form-control"
            />
            <span>часов</span>
          </div>
          <div class="duration-input">
            <input 
              type="number" 
              v-model="newSet.minutes" 
              min="0"
              max="59"
              class="form-control"
            />
            <span>минут</span>
          </div>
        </div>
      </div>
      
      <div class="form-group">
        <label>Вопросы (каждый вопрос с новой строки):</label>
        <textarea 
          v-model="newSet.questionsText" 
          placeholder="Введите ваши вопросы здесь, по одному на строку 
Пример:
Какая столица Франции?
Какая формула воды?"
          class="form-control questions-textarea"
          rows="10"
        ></textarea>
      </div>
      
      <button 
        type="button" 
        class="btn btn-primary" 
        @click="createTicketSet"
        :disabled="loading"
      >
        {{ loading ? 'Создание...' : 'Создать набор билетов' }}
      </button>
    </div>

    
    
    <div v-if="loading && !showCreateForm" class="loading">
      Загрузка наборов билетов...
    </div>
    
    <div v-if="!loading && !showCreateForm" class="ticket-sets-list">
      <h3>Доступные наборы билетов</h3>
      
      <div v-if="ticketSets.length === 0" class="no-sets">
        Нет доступных наборов билетов. Создайте свой первый набор!
      </div>
      
      <div v-else class="sets-grid">
        <div 
          v-for="set in ticketSets" 
          :key="set.id" 
          class="set-card"
          @click="openTicketsModal(set)"
        >
          <div class="set-card-header">
            <h4>{{ set.subject }}</h4>
            <button 
              class="btn-delete" 
              @click="(event) => deleteTicketSet(set.id, event)"
              title="Удалить набор билетов"
            >
              &times;
            </button>
          </div>
          <p>Количество билетов: {{ set.tickets.length || 0 }}</p>
          <p>Продолжительность: {{ set.duration }}</p>
        </div>
      </div>
    </div>
    
    <!-- Модальное окно с билетами -->
    <div v-if="showTicketsModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ selectedSet.subject }} - Билеты</h3>
          <button class="btn-close" @click="closeTicketsModal">&times;</button>
        </div>
        
        <div v-if="activeTicket" class="timer">
          Оставшееся время: {{ formatTime(remainingTime) }}
        </div>
        
        <div class="modal-body">
          <div v-if="!activeTicket" class="tickets-grid">
            <div 
              v-for="(ticket, index) in selectedSet.tickets" 
              :key="index"
              class="ticket-card"
              @click="showTicket(ticket)"
            >
              <span class="ticket-number">{{ index + 1 }}</span>
            </div>
          </div>
          
          <div v-else class="active-ticket">
            <h4>Билет {{ selectedSet.tickets.indexOf(activeTicket) + 1 }}</h4>
            <div class="ticket-content">
              <div class="question">
                <h5>Первый вопрос:</h5>
                <p>{{ activeTicket.firstQuestion }}</p>
              </div>
              <div class="question">
                <h5>Второй вопрос:</h5>
                <p>{{ activeTicket.secondQuestion }}</p>
              </div>
            </div>
            <button class="btn btn-secondary" @click="activeTicket = null">
              Вернуться к списку билетов
            </button>
          </div>
        </div>
      </div>
    </div>
    <div class="controls">
      <button 
        class="btn btn-primary" 
        @click="toggleCreateForm"
      >
        {{ showCreateForm ? 'Отмена' : 'Создать новый набор билетов' }}
      </button>
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
.exam-imitating {
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
}

.controls {
  margin-top: 20px;
  margin-bottom: 20px;
  display: flex;
  justify-content: flex-end;
}

.error-message {
  background-color: #fed7d7;
  color: #c53030;
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 20px;
}

.loading {
  text-align: center;
  padding: 20px;
  color: #4a5568;
}

.create-form {
  background-color: white;
  border-radius: 20px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
  color: #4a5568;
}

.duration-inputs {
  display: flex;
  gap: 15px;
}

.duration-input {
  display: flex;
  align-items: center;
  gap: 5px;
}

.duration-input input {
  width: 70px;
}

.duration-input span {
  color: #4a5568;
}

.form-control {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  font-size: 16px;
}

.questions-textarea {
  min-height: 200px;
  font-family: monospace;
}

.ticket-sets-list {
  background-color: white;
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.no-sets {
  text-align: center;
  padding: 20px;
  color: #718096;
}

.sets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.set-card {
  background-color: #f7fafc;
  border-radius: 6px;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s;
  cursor: pointer;
}

.set-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.set-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.btn-delete {
  background: none;
  border: none;
  border-radius: 15px;
  color: #e53e3e;
  font-size: 24px;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  opacity: 0.7;
  transition: opacity 0.2s;
}

.btn-delete:hover {
  opacity: 1;
}

.set-card h4 {
  margin: 0;
  color: #2d3748;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 15px;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  transition: background-color 0.3s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background-color: #4299e1;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #3182ce;
}

.btn-secondary {
  background-color: #e2e8f0;
  color: #4a5568;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #cbd5e0;
}

/* Стили для модального окна */
.modal-overlay {
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

.modal-content {
  background-color: white;
  border-radius: 8px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h3 {
  margin: 0;
  color: #2d3748;
}

.btn-close {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #718096;
}

.modal-body {
  padding: 20px;
}

.timer {
  background-color: #ebf8ff;
  color: #2b6cb0;
  padding: 10px 20px;
  text-align: center;
  font-size: 18px;
  font-weight: bold;
}

.tickets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 15px;
  padding: 20px 0;
}

.ticket-card {
  background-color: #edf2f7;
  border-radius: 6px;
  height: 100px;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  transition: transform 0.2s, background-color 0.2s;
}

.ticket-card:hover {
  background-color: #e2e8f0;
  transform: translateY(-2px);
}

.ticket-number {
  font-size: 24px;
  font-weight: bold;
  color: #4a5568;
}

.active-ticket {
  padding: 20px 0;
}

.active-ticket h4 {
  margin-bottom: 15px;
  color: #2d3748;
}

.ticket-content {
  background-color: #f7fafc;
  border-radius: 6px;
  padding: 20px;
  margin-bottom: 20px;
  white-space: pre-wrap;
  font-size: 16px;
  line-height: 1.6;
}

.question {
  margin-bottom: 20px;
}

.question h5 {
  color: #2d3748;
  margin-bottom: 8px;
  font-size: 18px;
}

.question p {
  margin: 0;
  padding: 10px;
  background-color: #edf2f7;
  border-radius: 4px;
}
</style> 
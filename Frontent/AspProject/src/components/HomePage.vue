<script>
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'HomePage',

  methods: {
    go(path) {
      this.$router.push(path);
    }
  }
});
</script>

<template>
  <div class="home-page grid-background">
    <section class="hero page-container">
      <div class="hero-copy">
        <div class="hero-kicker">EXAMARCHIVE · КФУ</div>

        <h1 class="hero-title">
          <span class="highlight">всё для твоей учёбы</span>
          <span class="serif-line">в одном архиве</span>
        </h1>

        <p class="hero-description">
          Методички, лекции, задания, билеты и подготовка к экзаменам
          в одном месте.
        </p>

        <div class="student-chip">
          <span class="student-dot"></span>
          <span>ИТИС</span>
          <span class="chip-divider">•</span>
          <span>Программная инженерия</span>
          <span class="chip-divider">•</span>
          <span>4 курс</span>
        </div>

        <div class="hero-actions">
          <button class="btn btn-dark" @click="go('/materials')">
            Найти материал
            <span>→</span>
          </button>

          <button class="btn btn-light" @click="go('/exam')">
            Подготовка к экзамену
            <span>→</span>
          </button>
        </div>
      </div>

      <div class="hero-visual" aria-hidden="true">
        <div class="paper paper-back"></div>

        <div class="photo-card">
          <div class="photo-placeholder">
            <div class="building">
              <div class="building-top"></div>
              <div class="columns">
                <span></span><span></span><span></span><span></span><span></span>
              </div>
            </div>
          </div>
          <div class="photo-caption">Казанский федеральный университет</div>
        </div>

        <div class="note-card">
          <strong>больше,</strong>
          <span>чем просто</span>
          <span>конспекты</span>
        </div>

        <div class="scribble">↘</div>
      </div>
    </section>

    <div class="ticker">
      <div class="ticker-track">
        <span>КФУ</span><b>•</b>
        <span>ВСЕ МАТЕРИАЛЫ В ОДНОМ МЕСТЕ</span><b>•</b>
        <span>УЧИСЬ ПРОЩЕ</span><b>•</b>
        <span>ГОТОВЬСЯ К ЭКЗАМЕНАМ</span><b>•</b>
        <span>КФУ</span>
      </div>
    </div>

    <section class="quick-section page-container">
      <div class="quick-grid">
        <article class="quick-card quick-card--blue" @click="go('/materials')">
          <div class="card-icon">▤</div>
          <h2>Библиотека</h2>
          <p>Методички, лекции, презентации, задания и другие материалы.</p>
          <button class="small-link">Перейти <span>→</span></button>
        </article>

        <article class="quick-card quick-card--green" @click="go('/exam')">
          <div class="card-icon">✓</div>
          <h2>Подготовка к экзамену</h2>
          <p>Вопросы преподавателей, карточки и тренировочные билеты.</p>
          <button class="small-link">Начать подготовку <span>→</span></button>
        </article>

        <article class="quick-card quick-card--cream" @click="go('/favorite')">
          <div class="card-icon">♡</div>
          <h2>Избранное</h2>
          <p>Сохраняй полезные материалы и возвращайся к ним позже.</p>
          <button class="small-link">Открыть <span>→</span></button>
        </article>

        <article class="quick-card quick-card--yellow">
          <div class="card-icon">✦</div>
          <h2>AI-ассистент</h2>
          <p>Ответы на вопросы по материалам архива со ссылками на источники.</p>
          <button class="small-link small-link--disabled">Скоро</button>
        </article>
      </div>
    </section>

    <section class="subjects-section">
      <div class="page-container">
        <div class="section-heading">
          <div>
            <div class="section-kicker">ТВОЯ ПРОГРАММА</div>
            <h2>твои предметы</h2>
          </div>

          <p>
            Быстрый доступ к предметам твоей образовательной программы.
          </p>
        </div>

        <div class="subjects-grid">
          <article
            v-for="subject in subjects"
            :key="subject.title"
            class="subject-card"
            :class="`subject-card--${subject.tone}`"
            @click="go('/materials')"
          >
            <div class="subject-pattern">
              <span class="subject-mark">↗</span>
            </div>

            <div class="subject-body">
              <h3>{{ subject.title }}</h3>
              <p>{{ subject.meta }}</p>
              <div class="subject-footer">
                <span>{{ subject.count }} материалов</span>
                <button>→</button>
              </div>
            </div>
          </article>
        </div>
      </div>
    </section>

    <section class="exam-section page-container">
      <div class="exam-copy">
        <div class="section-kicker">ПОДГОТОВКА К ЭКЗАМЕНАМ</div>
        <h2>экзамен?<br />ты справишься.</h2>

        <p>
          Создай набор вопросов преподавателя и тренируйся перед экзаменом
          в удобном формате.
        </p>

        <button class="btn btn-light" @click="go('/exam')">
          Открыть тренажёр
          <span>→</span>
        </button>
      </div>

      <div class="exam-panel">
        <div class="exam-panel-top">
          <div>
            <div class="exam-subject">Теория вероятностей</div>
            <div class="exam-meta">45 вопросов · экзамен</div>
          </div>
          <div class="progress-number">58%</div>
        </div>

        <div class="progress-bar">
          <span class="progress-main"></span>
          <span class="progress-mid"></span>
          <span class="progress-end"></span>
        </div>

        <div class="progress-stats">
          <div>
            <strong>32</strong>
            <span>не изучены</span>
          </div>
          <div>
            <strong>8</strong>
            <span>повторить</span>
          </div>
          <div>
            <strong>5</strong>
            <span>освоены</span>
          </div>
        </div>

        <div class="exam-modes">
          <div class="mode-card">
            <span class="mode-icon">▤</span>
            <div>
              <strong>Карточки</strong>
              <small>Учи вопросы по темам</small>
            </div>
          </div>

          <div class="mode-card">
            <span class="mode-icon">✦</span>
            <div>
              <strong>AI-экзаменатор</strong>
              <small>Отвечай и получай разбор</small>
            </div>
          </div>

          <div class="mode-card">
            <span class="mode-icon">◫</span>
            <div>
              <strong>Пробный экзамен</strong>
              <small>Случайная тренировочная сессия</small>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="ai-section">
      <div class="page-container ai-grid">
        <div class="ai-copy">
          <div class="section-kicker">AI-АССИСТЕНТ</div>
          <h2>спроси у архива</h2>
          <p>
            Получай ответы по материалам архива и сразу переходи
            к использованным источникам.
          </p>

          <button class="btn btn-light" disabled>
            Скоро
          </button>
        </div>

        <div class="chat-preview">
          <div class="chat-question">
            Объясни метод максимального правдоподобия по материалам моего курса
          </div>

          <div class="chat-answer">
            <div class="bot-avatar">✦</div>
            <div>
              <p>
                Метод максимального правдоподобия — это способ оценки параметров
                модели, при котором выбираются значения, максимально согласующиеся
                с наблюдаемыми данными.
              </p>

              <div class="sources">
                <span>Методичка_ТВ.pdf · стр. 14</span>
                <span>Лекция_7.pdf · стр. 3</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="continue-section page-container">
      <div class="section-heading compact-heading">
        <div>
          <div class="section-kicker">НЕДАВНО ОТКРЫТЫЕ</div>
          <h2>продолжить обучение</h2>
        </div>
      </div>

      <div class="continue-grid">
        <div class="continue-card">
          <span class="file-icon">PDF</span>
          <div>
            <strong>Методичка_ТВ.pdf</strong>
            <small>Теория вероятностей</small>
          </div>
        </div>

        <div class="continue-card">
          <span class="file-icon file-icon--blue">PDF</span>
          <div>
            <strong>Лекция_3.pdf</strong>
            <small>Базы данных</small>
          </div>
        </div>

        <div class="continue-card">
          <span class="file-icon file-icon--green">&lt;/&gt;</span>
          <div>
            <strong>Практика_ML.ipynb</strong>
            <small>Машинное обучение</small>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
  overflow: hidden;
  color: var(--color-text, #161616);
}

.hero {
  min-height: 560px;
  display: grid;
  grid-template-columns: minmax(0, 1.25fr) minmax(330px, 0.75fr);
  align-items: center;
  gap: 64px;
  padding-top: 76px;
  padding-bottom: 70px;
}

.hero-kicker,
.section-kicker {
  margin-bottom: 14px;
  color: var(--color-text-secondary, #64645f);
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.14em;
}

.hero-title {
  margin: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  color: var(--color-text, #161616);
  letter-spacing: -0.06em;
}

.highlight {
  display: inline-block;
  padding: 0 15px 5px;
  background: var(--color-blue, #cbe8f2);
  font-size: clamp(48px, 6.3vw, 94px);
  font-weight: 900;
  line-height: 0.98;
  transform: rotate(-1deg);
}

.serif-line {
  margin-top: 8px;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(52px, 6.7vw, 100px);
  font-weight: 400;
  line-height: 0.92;
}

.hero-description {
  max-width: 650px;
  margin: 30px 0 18px;
  font-size: 20px;
  line-height: 1.45;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.btn {
  min-height: 48px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 24px;
  padding: 0 20px;
  border-radius: 999px;
  font-size: 14px;
  font-weight: 800;
}

.btn-dark {
  border: 1px solid #171717;
  background: #171717;
  color: #fff;
}

.btn-light {
  border: 1px solid #171717;
  background: #fff;
  color: #171717;
}

.btn:disabled {
  opacity: 0.5;
  cursor: default;
}

.hero-visual {
  position: relative;
  min-height: 390px;
}

.paper {
  position: absolute;
  border-radius: 4px;
}

.paper-back {
  top: 20px;
  right: 18px;
  width: 76%;
  height: 310px;
  background: var(--color-cream, #f4ead4);
  transform: rotate(4deg);
}

.photo-card {
  position: absolute;
  top: 40px;
  right: 42px;
  width: min(350px, 84%);
  padding: 10px 10px 20px;
  background: #fff;
  border: 1px solid #deddd7;
  box-shadow: 0 18px 50px rgba(20, 20, 18, 0.12);
  transform: rotate(4deg);
}

.photo-placeholder {
  height: 260px;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  overflow: hidden;
  background:
    linear-gradient(to bottom, #cfe9f2 0 46%, #d8d7ca 46% 47%, #9fb07e 47% 100%);
}

.building {
  width: 82%;
  height: 67%;
  background: #ded6c5;
  border: 2px solid #2e2e2a;
  box-shadow: inset 0 -22px rgba(0, 0, 0, 0.05);
}

.building-top {
  height: 30px;
  margin: -22px auto 0;
  width: 74%;
  background: #c9bfac;
  border: 2px solid #2e2e2a;
}

.columns {
  height: calc(100% - 10px);
  display: flex;
  justify-content: space-around;
  padding: 17px 22px 0;
}

.columns span {
  width: 12px;
  height: 100%;
  background: #f4f0e7;
  border-left: 1px solid #908a7d;
  border-right: 1px solid #908a7d;
}

.photo-caption {
  margin-top: 12px;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 15px;
  font-style: italic;
}

.note-card {
  position: absolute;
  top: 7px;
  right: -5px;
  z-index: 3;
  width: 142px;
  min-height: 125px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 16px;
  background: var(--color-green, #dce8c6);
  border: 1px solid rgba(0, 0, 0, 0.06);
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 16px;
  font-style: italic;
  transform: rotate(8deg);
}

.note-card strong {
  font-size: 20px;
}

.scribble {
  position: absolute;
  left: 18px;
  bottom: 18px;
  font-size: 56px;
  transform: rotate(15deg);
}

.ticker {
  overflow: hidden;
  border-top: 1px solid rgba(0, 0, 0, 0.04);
  border-bottom: 1px solid rgba(0, 0, 0, 0.04);
  background: var(--color-green, #dce8c6);
}

.ticker-track {
  min-height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 22px;
  padding: 0 18px;
  white-space: nowrap;
  font-size: 10px;
  font-weight: 800;
  letter-spacing: 0.07em;
}

.quick-section {
  padding-top: 60px;
  padding-bottom: 65px;
}

.quick-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 18px;
}

.quick-card {
  min-height: 310px;
  display: flex;
  flex-direction: column;
  padding: 24px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 16px;
  cursor: pointer;
  transition: transform 0.18s ease, box-shadow 0.18s ease;
}

.quick-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-card, 0 8px 30px rgba(24, 24, 20, 0.06));
}

.quick-card--blue { background: var(--color-blue, #cbe8f2); }
.quick-card--green { background: var(--color-green, #dce8c6); }
.quick-card--cream { background: var(--color-cream, #f4ead4); }
.quick-card--yellow { background: #f5edcf; }

.card-icon {
  width: 48px;
  height: 48px;
  display: grid;
  place-items: center;
  margin-left: auto;
  border-radius: 13px;
  background: rgba(255, 255, 255, 0.78);
  font-size: 24px;
}

.quick-card h2 {
  margin: auto 0 10px;
  font-size: 28px;
  letter-spacing: -0.04em;
}

.quick-card p {
  min-height: 64px;
  margin: 0 0 20px;
  color: #33332f;
  font-size: 14px;
  line-height: 1.45;
}

.small-link {
  width: fit-content;
  min-height: 39px;
  display: inline-flex;
  align-items: center;
  gap: 17px;
  padding: 0 15px;
  border: 1px solid #202020;
  border-radius: 999px;
  background: #fff;
  color: #171717;
  font-size: 12px;
  font-weight: 800;
}

.small-link--disabled {
  opacity: 0.55;
}

.section-heading {
  display: grid;
  grid-template-columns: 1fr minmax(250px, 0.7fr);
  align-items: end;
  gap: 30px;
  margin-bottom: 28px;
}

.section-heading h2,
.exam-copy h2,
.ai-copy h2 {
  margin: 0;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(42px, 5.2vw, 72px);
  font-weight: 400;
  line-height: 0.92;
  letter-spacing: -0.04em;
}

.section-heading > p {
  margin: 0 0 6px;
  color: var(--color-text-secondary, #64645f);
  font-size: 14px;
  line-height: 1.5;
}

.subject-card--blue .subject-card--green .subject-card--cream .subject-card--yellow .exam-section {
  display: grid;
  grid-template-columns: 0.9fr 1.1fr;
  gap: 65px;
  align-items: center;
  padding-top: 78px;
  padding-bottom: 78px;
}

.exam-copy p,
.ai-copy p {
  max-width: 540px;
  margin: 24px 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 16px;
  line-height: 1.55;
}

.ai-section {
  padding: 72px 0;
  background: var(--color-blue, #cbe8f2);
}

.ai-grid {
  display: grid;
  grid-template-columns: .85fr 1.15fr;
  gap: 65px;
  align-items: center;
}


.exam-placeholder,
.ai-placeholder {
  min-height: 220px;
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 28px;
  border: 1px dashed var(--color-border-dark, #202020);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.72);
}

.exam-placeholder-mark,
.ai-placeholder-icon {
  width: 54px;
  height: 54px;
  flex: 0 0 54px;
  display: grid;
  place-items: center;
  border-radius: 16px;
  background: var(--color-green, #dce8c6);
  font-size: 25px;
  font-weight: 800;
}

.ai-placeholder-icon {
  background: var(--color-cream, #f4ead4);
}

.exam-placeholder strong,
.ai-placeholder strong {
  display: block;
  margin-bottom: 8px;
  font-size: 18px;
}

.exam-placeholder p,
.ai-placeholder p {
  margin: 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 13px;
  line-height: 1.5;
}

@media (max-width: 1050px) {
  .hero,
  .exam-section,
  .ai-grid {
    grid-template-columns: 1fr;
  }

  .hero-visual {
    min-height: 360px;
  }

  .quick-grid,
  }

@media (max-width: 700px) {
  .hero {
    gap: 25px;
    padding-top: 52px;
  }

  .highlight {
    font-size: clamp(42px, 13vw, 64px);
  }

  .serif-line {
    font-size: clamp(46px, 14vw, 70px);
  }

  .hero-description {
    font-size: 17px;
  }

  .quick-grid,
  .subjects-grid,
  .section-heading {
    grid-template-columns: 1fr;
  }

  .photo-card {
    right: 20px;
  }

  .note-card {
    right: 0;
  }
}
</style>

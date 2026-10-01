<script>
export default {
  name: 'HomePage',
  data() {
    return {
      features: [
        { title: 'Библиотека', text: 'Методички, лекции, презентации, задания и другие материалы.', action: 'Перейти в библиотеку', path: '/materials', image: 'books.jpg', tone: 'sky', icon: '▤' },
        { title: 'Подготовка к экзамену', text: 'Вопросы преподавателей, карточки, пробные экзамены и статистика.', action: 'Начать подготовку', path: '/exam', image: 'notes.jpg', tone: 'sage', icon: '✓' },
        { title: 'Избранное', text: 'Сохраняй важные материалы и возвращайся к ним в любой момент.', action: 'Открыть избранное', path: '/favorite', image: 'planning.jpg', tone: 'sand', icon: '♡' },
        { title: 'AI-ассистент', text: 'Задавай вопросы и получай ответы на основе материалов архива.', action: 'Спросить AI', path: '/assistant', image: '', tone: 'sky', icon: '✦' }
      ],
      subjects: [
        { title: 'Машинное обучение', meta: 'ИТИС · 5 семестр', count: 42, image: 'code.jpg', tone: 'pink' },
        { title: 'Веб-разработка', meta: 'ИТИС · 4 семестр', count: 36, image: 'code.jpg', tone: 'blue' },
        { title: 'Базы данных', meta: 'ИТИС · 3 семестр', count: 28, image: 'books.jpg', tone: 'cream' },
        { title: 'Управление проектами', meta: 'ИТИС · 6 семестр', count: 31, image: 'planning.jpg', tone: 'blue' }
      ]
    }
  },
  methods: {
    go(path) { this.$router.push(path) }
  }
}
</script>

<template>
  <div class="home-page">
    <section class="hero page-container">
      <div class="hero-copy">
        <span class="scribble rays">✳</span>
        <h1>
          <span class="highlight-line">всё для твоей учёбы</span>
          <span class="serif hero-second">в одном архиве</span>
        </h1>
        <p class="hero-description">Методички, лекции, билеты и AI-помощник для студентов КФУ.<br>Ищи материалы по своей программе или тренируйся к экзамену.</p>
        <button class="program-pill" type="button" @click="go('/mypage')">
          <img src="/assets/avatar.jpg" alt=""><span>ИТИС</span><b>·</b><span>Программная инженерия</span><b>·</b><span>4 курс</span><b>·</b><span>7 семестр</span><span>✎</span>
        </button>
        <div class="hero-buttons">
          <button class="pill-button pill-dark" @click="go('/materials')">⌕ Найти материал</button>
          <button class="pill-button" @click="go('/assistant')">✦ Спросить AI</button>
        </div>
      </div>

      <div class="hero-collage">
        <div class="polaroid campus"><img src="/assets/university.jpg" alt="Корпус университета"><p>Казанский<br>федеральный университет ♡</p></div>
        <div class="polaroid desk"><img src="/assets/study-desk.jpg" alt="Рабочее место"></div>
        <div class="sticky-note">Больше,<br>чем просто<br>конспекты<br>&lt;3</div>
        <div class="little-note">учись<br>проще ♡</div>
      </div>
    </section>

    <section class="feature-section page-container">
      <div class="feature-grid">
        <article v-for="feature in features" :key="feature.title" class="feature-card" :class="`feature-${feature.tone}`">
          <div class="feature-art">
            <img v-if="feature.image" :src="`/assets/${feature.image}`" alt="">
            <div v-else class="robot-art"><span>✦</span><div class="robot">AI</div><span>✦</span></div>
            <span class="feature-icon">{{ feature.icon }}</span>
          </div>
          <div class="feature-content">
            <h2>{{ feature.title }}</h2><p>{{ feature.text }}</p>
            <button class="pill-button feature-link" @click="go(feature.path)">{{ feature.action }} →</button>
          </div>
        </article>
      </div>
    </section>

    <section class="subjects-band">
      <div class="page-container">
        <div class="subjects-heading">
          <div><span class="eyebrow">ТВОЯ ПРОГРАММА</span><h2 class="serif section-title">твои предметы</h2></div>
          <p>Здесь собраны предметы твоей образовательной программы.<br>Открывай материалы и готовься к занятиям и экзаменам.</p>
          <button class="text-link" @click="go('/materials')">Все предметы →</button>
        </div>
        <div class="subject-grid">
          <button v-for="subject in subjects" :key="subject.title" class="subject-card" @click="go('/materials')">
            <div class="subject-image"><img :src="`/assets/${subject.image}`" alt=""><span class="subject-icon" :class="`icon-${subject.tone}`">↗</span></div>
            <div class="subject-info"><strong>{{ subject.title }}</strong><span>{{ subject.meta }}</span><small>{{ subject.count }} материалов</small><span class="subject-arrow">→</span></div>
          </button>
        </div>
      </div>
    </section>

    <section class="exam-section page-container">
      <div class="exam-intro">
        <span class="eyebrow">ПОДГОТОВКА К ЭКЗАМЕНАМ</span>
        <h2 class="serif section-title">экзамен?<br>ты справишься.</h2>
        <p>Тренируйся по реальным вопросам преподавателей, изучай темы и отслеживай прогресс.</p>
        <button class="pill-button" @click="go('/exam')">Открыть подготовку →</button>
      </div>
      <div class="exam-panel">
        <div class="exam-panel-top"><span class="exam-icon">↗</span><div><h3>Теория вероятностей</h3><small>ИТИС · 3 семестр · Экзамен</small></div><button @click="go('/exam')">→</button></div>
        <div class="progress-line"><span/><span/><span/></div>
        <div class="exam-stats"><div><b>32</b><small>не изучены</small></div><div><b>8</b><small>повторить</small></div><div><b>5</b><small>освоены</small></div></div>
        <div class="exam-tools"><button @click="go('/exam')"><span>▤</span><b>Карточки</b></button><button @click="go('/assistant')"><span>✦</span><b>AI-экзаменатор</b></button><button @click="go('/exam')"><span>◫</span><b>Пробный экзамен</b></button></div>
      </div>
    </section>

    <section class="ai-band">
      <div class="page-container ai-layout">
        <div class="ai-intro"><span class="eyebrow">AI-АССИСТЕНТ</span><h2 class="serif section-title">спроси у архива ✧</h2><p>Получай понятные ответы со ссылками на реальные материалы из архива.</p><button class="pill-button" @click="go('/assistant')">Задать вопрос →</button></div>
        <div class="chat-preview"><div class="chat-question">Объясни, что такое формула полной вероятности?<img src="/assets/avatar.jpg" alt=""></div><div class="chat-answer"><span class="bot">AI</span><div><p>Формула полной вероятности помогает найти вероятность события через несколько взаимоисключающих гипотез.</p><div class="sources"><span>▤ Методичка_ТВ.pdf · стр. 14</span><span>▤ Лекция_7.pdf · стр. 3</span></div></div></div><div class="chat-input">Спроси что-нибудь об учёбе... <button @click="go('/assistant')">↑</button></div></div>
      </div>
    </section>

    <section class="recent-section page-container">
      <div><span class="eyebrow">НЕДАВНО ОТКРЫТЫЕ</span><h2 class="serif">продолжить обучение →</h2></div>
      <div class="recent-list"><button @click="go('/materials')">▤ <span><b>Методичка_ТВ.pdf</b><small>Теория вероятностей</small></span></button><button @click="go('/materials')">▤ <span><b>Лекция_3.pdf</b><small>Базы данных</small></span></button><button @click="go('/materials')">&lt;/&gt; <span><b>Практика_ML.ipynb</b><small>Машинное обучение</small></span></button></div>
    </section>
  </div>
</template>

<style scoped>
.home-page { overflow: hidden; }
.hero { height: 400px; display: grid; grid-template-columns: minmax(0,65%) minmax(0,35%); position: relative; margin-bottom: 5%; }
.hero-copy { padding: 58px 0 0 52px; position: relative; z-index: 2; }
.hero h1 { margin:0 0 13px; display:flex; flex-direction:column; white-space:nowrap; letter-spacing:-.055em; line-height:.99; }
.highlight-line { position:relative; z-index:0; width:max-content; font-weight:800; font-size:clamp(42px,5.5vw,79px); transform:rotate(-2deg); line-height:1.11; }
.highlight-line:before { content:''; position:absolute; z-index:-1; inset:2px -12px 2px -8px; background:#cceef8; clip-path:polygon(0 8%,100% 0,100% 92%,0 100%); }
.hero-second { font-size:clamp(49px,6.4vw,93px); margin:-1px 0 0 21px; }
.hero-description { margin:0 0 15px 21px; font-size:clamp(14px,1.55vw,18px); line-height:1.25; letter-spacing:-.035em; }
.program-pill { margin-left:21px; border:1px solid #d4d5d1; background:#ffffffaa; border-radius:22px; padding:3px 12px 3px 3px; display:inline-flex; align-items:center; gap:9px; height:39px; font-size:12px; white-space:nowrap; }
.program-pill img { width:31px;height:31px;border-radius:50%;object-fit:cover}.program-pill b{font-weight:400;color:#999}.hero-buttons{display:flex;gap:12px;margin:14px 0 0 21px}.scribble{position:absolute;font-family:Georgia,serif}.rays{top:15px;left:112px;font-size:40px;transform:rotate(-17deg)}
.hero-collage{position:relative;height:100%}.polaroid{position:absolute;background:#fffefa;box-shadow:0 2px 10px #332c1b1c;padding:5px 5px 7px}.polaroid img{display:block;width:100%;object-fit:cover}.campus{width:64%;height:305px;left:4%;top:22px;transform:rotate(5deg);z-index:1}.campus img{height:223px}.campus p{font:italic 15px/1.05 Georgia,serif;margin:8px 8px 0}.desk{width:42%;height:215px;right:0;top:116px;transform:rotate(-8deg);z-index:2}.desk img{height:100%}.sticky-note{position:absolute;z-index:3;right:6%;top:13px;width:92px;height:112px;background:#e8f0cd;transform:rotate(12deg);padding:20px 10px 8px;font:italic 14px/1.2 Georgia,serif;box-shadow:2px 4px 8px #3331}.little-note{position:absolute;z-index:3;right:0;bottom:6px;background:#ddf1fa;padding:10px 11px;font:italic 14px/1 Georgia,serif;transform:rotate(-10deg)}
.feature-section{padding-bottom:17px}.feature-grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:10px}.feature-card{border:1px solid #d0d6d3;border-radius:10px;min-width:0;height:278px;overflow:hidden;display:flex;flex-direction:column}.feature-sky{background:#d8f0f9}.feature-sage{background:#e6efd9}.feature-sand{background:#f7f2e2}.feature-art{height:128px;position:relative;flex:none;overflow:hidden}.feature-art>img{width:84%;height:110px;position:absolute;bottom:-10px;left:3%;object-fit:cover;border-radius:5px;transform:rotate(-5deg);filter:saturate(.8);box-shadow:0 6px 10px #2222}.feature-icon{position:absolute;right:15px;top:15px;height:43px;width:43px;border-radius:12px;display:grid;place-items:center;background:#ffffffd9;font-size:20px}.feature-content{padding:6px 17px 13px;display:flex;flex-direction:column;flex:1}.feature-content h2{margin:0 0 4px;font-size:clamp(18px,1.8vw,24px);letter-spacing:-.065em;line-height:1.05}.feature-content p{margin:0;font-size:12px;line-height:1.33}.feature-link{margin-top:auto;align-self:flex-start;min-height:34px;font-size:11px;padding:0 13px}.robot-art{height:100%;display:flex;align-items:center;justify-content:center;gap:15px;font:28px Georgia}.robot{width:80px;height:67px;border-radius:31px;background:#e9eef2;border:2px solid #c7d0d7;display:grid;place-items:center;font:800 18px/1 'DM Sans';color:#86bce0;box-shadow:inset 0 0 0 11px #162440}
.subjects-band{background:#eaf2e1cc;padding:16px 0 15px}.subjects-heading{display:flex;align-items:end;gap:32px;margin:0 8px 10px}.section-title{font-size:clamp(38px,4.2vw,61px);line-height:1.04;margin:2px 0 0}.subjects-heading p{margin:0 0 7px auto;font-size:11px;line-height:1.4}.text-link{border:0;background:none;padding:0 0 8px;font-size:11px;font-weight:700}.subject-grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:9px}
.subject-card{
  min-height: 235px;
  padding:0;
  border:1px solid #d9d9d5;
  background:#fff;
  text-align:left;
  border-radius:9px;
  overflow:hidden;
  transition:.2s

}
.subject-card:hover
{
  transform:translateY(-3px);
  box-shadow:0 6px 15px #14141416
}
.subject-image{height:135px;position:relative;overflow:hidden}.subject-image img{width:100%;height:100%;object-fit:cover;filter:brightness(.78) saturate(.75)}.subject-icon{position:absolute;right:11px;top:10px;width:41px;height:41px;border-radius:10px;display:grid;place-items:center}.icon-pink{background:#f6d7de}.icon-blue{background:#dceffa}.icon-cream{background:#f5e9cf}.subject-info{height:100px;padding:12px 16px 14px;position:relative;display:flex;flex-direction:column;gap:4px}.subject-info strong{font-size:13px}.subject-info span,.subject-info small{font-size:10px;color:#777}.subject-arrow{position:absolute;right:10px;bottom:10px;width:32px;height:32px;border:1px solid #e2e2e2;background:white;border-radius:50%;display:grid;place-items:center;color:#111!important}
.exam-section{min-height:245px;display:grid;grid-template-columns:38% 62%;gap:5px;padding:14px 0 10px}.exam-intro,.exam-panel{border:1px solid #e9e8e2;border-radius:9px;padding:14px 16px;background:#ffffff88}.exam-intro .section-title{font-size:clamp(37px,4.2vw,58px);line-height:.97;margin:12px 0 10px}.exam-intro p{font-size:12px;max-width:350px}.exam-intro .pill-button{min-height:34px;font-size:10px}.exam-panel-top{display:flex;align-items:center;gap:12px}.exam-icon{width:40px;height:40px;border-radius:10px;background:#d9eff8;display:grid;place-items:center}.exam-panel-top h3{margin:0;font-size:17px}.exam-panel-top small{color:#888}.exam-panel-top button{margin-left:auto;width:30px;height:30px;border:1px solid #ddd;border-radius:50%;background:white}.progress-line{display:flex;gap:3px;margin:20px 0 8px;height:13px}.progress-line span:first-child{background:#f5cbd7;width:59%}.progress-line span:nth-child(2){background:#fae1a5;width:22%}.progress-line span:last-child{background:#cfe0b9;flex:1}.exam-stats{display:flex;gap:16%;margin:8px 0 14px}.exam-stats div{display:flex;gap:5px;align-items:baseline}.exam-stats small{color:#888}.exam-tools{display:grid;grid-template-columns:repeat(3,1fr);gap:6px}.exam-tools button{background:#fff;border:1px solid #ebebe8;border-radius:9px;display:flex;align-items:center;gap:9px;padding:10px;text-align:left}.exam-tools span{width:30px;height:30px;border-radius:7px;background:#dff2f9;display:grid;place-items:center}
.ai-band{background:#d8f2f8db;padding:13px 0}.ai-layout{display:grid;grid-template-columns:40% 60%;gap:8px;min-height:190px}.ai-intro{padding:10px}.ai-intro p{font-size:12px;max-width:430px}.ai-intro .pill-button{min-height:36px;font-size:11px}.chat-preview{background:#f7f8f8;border-radius:10px;padding:10px 12px;position:relative}.chat-question{background:#d8f0fb;border-radius:9px;max-width:330px;min-height:38px;margin-left:auto;display:flex;align-items:center;padding:4px 5px 4px 12px;gap:15px;font-size:10px}.chat-question img{width:30px;height:30px;border-radius:50%;object-fit:cover}.chat-answer{display:flex;gap:9px;margin-top:8px}.bot{width:32px;height:32px;border-radius:50%;background:#17254b;color:#8ed1ff;display:grid;place-items:center;font-size:10px}.chat-answer>div{background:white;border:1px solid #ececec;border-radius:7px;padding:8px 10px;flex:1}.chat-answer p{font-size:10px;margin:0 0 8px}.sources{display:flex;gap:8px;font-size:9px;color:#4d6777}.chat-input{position:absolute;bottom:8px;left:53px;right:12px;height:31px;border:1px solid #e4e9ec;border-radius:8px;background:white;color:#999;font-size:10px;padding:8px 9px}.chat-input button{position:absolute;right:0;top:0;height:100%;width:35px;border:0;background:#d8f0fb}
.recent-section{min-height:92px;display:flex;align-items:center;gap:18px;padding:12px 0}.recent-section h2{font-size:25px;margin:0;white-space:nowrap}.recent-list{display:flex;gap:8px;flex:1;overflow:hidden}.recent-list button{min-width:190px;flex:1;border:1px solid #eee;background:#ffffffb0;border-radius:8px;padding:8px;display:flex;gap:8px;text-align:left}.recent-list span{display:flex;flex-direction:column}.recent-list b{font-size:10px}.recent-list small{font-size:9px;color:#999}
@media(max-width:700px){.hero{height:auto;display:flex;flex-direction:column}.hero-copy{padding:45px 0 0 7px}.hero h1{white-space:normal}.highlight-line{font-size:clamp(32px,7.6vw,53px)}.hero-second{font-size:clamp(43px,9vw,64px);margin-left:10px}.hero-description{margin-left:10px;font-size:13px}.hero-description br{display:none}.program-pill{margin-left:10px;max-width:calc(100% - 10px);overflow:hidden;font-size:10px}.hero-buttons{margin-left:10px}.hero-collage{height:270px;width:min(100%,420px);margin:18px auto 0}.campus{width:57%;left:14%;top:5px;height:240px}.campus img{height:175px}.desk{width:41%;right:3%;top:75px;height:175px}.sticky-note{right:6%;top:0}.feature-grid,.subject-grid{grid-template-columns:repeat(2,minmax(0,1fr))}.subjects-heading{flex-wrap:wrap;gap:4px}.subjects-heading>div{width:100%}.subjects-heading p{margin:0 auto 6px 0}.exam-section,.ai-layout{grid-template-columns:1fr}.recent-section{flex-wrap:wrap}.recent-list{width:100%}}
@media(max-width:420px){.feature-grid,.subject-grid{grid-template-columns:1fr}.feature-card{height:270px}.program-pill span:nth-of-type(4),.program-pill b:nth-of-type(3){display:none}.hero-buttons{flex-wrap:wrap}}
</style>

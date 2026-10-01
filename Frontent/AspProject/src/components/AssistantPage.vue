<script>
export default {
  name: 'AssistantPage',
  data() { return { message: '', conversation: [] } },
  methods: {
    submit(text) {
      const value = (text || this.message).trim()
      if (!value) return
      this.conversation.push({ role: 'user', text: value })
      this.conversation.push({ role: 'assistant', text: 'Это демонстрационный ответ интерфейса. Когда подключим RAG/backend, здесь будет ответ по материалам архива со ссылками на источники.' })
      this.message = ''
    }
  }
}
</script>

<template>
  <div class="assistant-page page-container">
    <section class="assistant-hero">
      <div><span class="eyebrow">AI-АССИСТЕНТ</span><h1 class="serif">спроси у архива.</h1><p>Разбирайся в сложных темах проще. Сейчас это UI-заглушка; позже сюда подключается RAG.</p></div>
    </section>

    <div class="assistant-layout">
      <aside class="assistant-sidebar">
        <div class="robot"><div>AI</div><span>✦</span></div>
        <h2 class="serif">с чего начнём?</h2>
        <p>Выбери вопрос или напиши свой. После подключения backend ответы будут строиться по материалам архива.</p>
        <div class="suggestions">
          <button @click="submit('Объясни формулу полной вероятности')">✦ Объясни формулу полной вероятности <span>→</span></button>
          <button @click="submit('Как подготовиться к экзамену?')">✦ Как подготовиться к экзамену? <span>→</span></button>
          <button @click="submit('Что такое машинное обучение?')">✦ Что такое машинное обучение? <span>→</span></button>
        </div>
      </aside>

      <section class="assistant-chat">
        <header><span class="chat-avatar">AI</span><div><strong>AI-ассистент</strong><small>Ответы на основе материалов архива</small></div><span class="online">на связи</span></header>
        <div class="messages">
          <div class="bubble assistant">Привет! Я помогу разобраться в материалах и подготовиться к экзамену. О чём хочешь спросить?</div>
          <div v-for="(item,index) in conversation" :key="index" class="bubble" :class="item.role">{{ item.text }}<span v-if="item.role==='assistant'" class="source">▤ Источник появится после подключения RAG</span></div>
        </div>
        <form class="compose" @submit.prevent="submit()"><input v-model="message" placeholder="Задай свой вопрос об учёбе..."><button type="submit">↑</button></form>
      </section>
    </div>
  </div>
</template>

<style scoped>
.assistant-page{padding:26px 0 80px}.assistant-hero{min-height:220px;background:#e0f0f4;border:1px solid #c9e1e6;border-radius:17px;padding:40px clamp(24px,5vw,70px);display:flex;align-items:center}.assistant-hero h1{font-size:clamp(45px,6vw,80px);margin:8px 0 12px;line-height:1}.assistant-hero p{max-width:620px;color:#555}.assistant-layout{display:grid;grid-template-columns:minmax(250px,34%) 1fr;gap:20px;margin-top:24px}.assistant-sidebar{background:#e7efda;border:1px solid #d5dfc7;border-radius:15px;padding:25px}.robot{height:130px;display:flex;align-items:center;justify-content:center;gap:10px}.robot div{width:105px;height:78px;border-radius:35px;background:#e8edf1;border:2px solid #c8d0d6;box-shadow:inset 0 0 0 13px #152441;color:#8ed1ff;display:grid;place-items:center;font-weight:800}.robot span{font-size:34px}.assistant-sidebar h2{font-size:35px;margin:5px 0 8px}.assistant-sidebar p{font-size:12px;line-height:1.45;margin:0 0 20px}.suggestions{display:grid;gap:8px}.suggestions button{min-height:48px;border:1px solid #d7dfcf;background:#ffffffc9;border-radius:9px;padding:10px;display:flex;align-items:center;gap:9px;text-align:left;font-size:11px}.suggestions button span{margin-left:auto}.assistant-chat{min-height:520px;background:#fff;border:1px solid #dfe4e3;border-radius:15px;display:flex;flex-direction:column;overflow:hidden}.assistant-chat header{display:flex;align-items:center;gap:10px;padding:13px 18px;border-bottom:1px solid #e9ebeb}.chat-avatar{width:36px;height:36px;background:#17254b;color:#8ed1ff;border-radius:50%;display:grid;place-items:center;font-size:10px}.assistant-chat header strong,.assistant-chat header small{display:block}.assistant-chat header strong{font-size:13px}.assistant-chat header small{font-size:10px;color:#888}.online{margin-left:auto;background:#e8f2e6;color:#50795b;border-radius:14px;padding:6px 9px;font-size:10px}.messages{flex:1;background:#fbfcfa;padding:22px;display:flex;flex-direction:column;gap:13px;overflow:auto;max-height:500px}.bubble{max-width:min(85%,460px);padding:13px 16px;font-size:12px;line-height:1.45;border-radius:13px}.bubble.assistant{background:white;border:1px solid #e9ecea;align-self:flex-start}.bubble.user{background:#d9f0fa;align-self:flex-end}.source{display:block;color:#31617b;margin-top:9px;font-size:10px}.compose{display:flex;gap:8px;padding:14px;border-top:1px solid #e9ebeb}.compose input{border:1px solid #d9e0df;border-radius:24px;padding:0 17px;height:43px;flex:1;outline:none;font-size:12px}.compose button{width:43px;height:43px;border:0;border-radius:50%;background:#cfeaf4}@media(max-width:700px){.assistant-layout{grid-template-columns:1fr}.assistant-sidebar{padding:18px}.assistant-chat{min-height:470px}}
</style>

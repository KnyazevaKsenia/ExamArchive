<script>
import { defineComponent } from 'vue';
import Navbar from './components/Navbar.vue';

export default defineComponent({
  name: 'App',
  components: {
    Navbar
  }
});
</script>

<template>
  <div id="app">
    <Navbar />
    
    <main class="content">
      <router-view />
    </main>
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: Arial, sans-serif;
}

body {
  background-color: #f8fafc;
}

#app {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  padding-top: 0;
}

.content {
  background-color: #f7fafc;
  border-radius: 6px;
  padding: 20px;
  min-height: 400px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

h2 {
  margin-bottom: 20px;
  color: #2d3748;
}
</style>

<script>
import { defineComponent } from 'vue';
import Navbar from './components/Navbar.vue';

export default defineComponent({
  name: 'App',
  components: { Navbar }
});
</script>

<template>
  <div id="app" class="app-shell">
    <Navbar />
    <main class="app-content">
      <router-view />
    </main>
  </div>
</template>

<style>
#app { min-height: 100vh; }
.app-content { min-height: calc(100vh - 66px); }
</style>

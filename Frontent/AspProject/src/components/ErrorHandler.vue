<template>
  <transition name="slide-down">
    <div v-if="errorMessage" class="error-container">
      <div class="error-message">
        <span class="error-text">{{ errorMessage }}</span>
        <div class="error-actions">
        
          <button class="error-close" @click="closeError">×</button>
        </div>
      </div>
    </div>
  </transition>
</template>

<script>
import {ref} from 'vue'

export default {
  name: 'ErrorHandler',

  setup(props, {emit}) {
    const errorMessage = ref(null)
    let timeoutId = null

    const handleError = (message) => {
      errorMessage.value = message

      if (timeoutId) clearTimeout(timeoutId)
      timeoutId = setTimeout(() => {
        errorMessage.value = null
      }, 15000)
    }

    const closeError = () => {
      if (timeoutId) clearTimeout(timeoutId)
      errorMessage.value = null
    }

    return {
      errorMessage,
      handleError,
      closeError
    }
  }
}
</script>

<style scoped>
.error-container {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 9999;
  width: 80%;
  max-width: 500px;
}

.error-message {
  background-color: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  color: #4a5568;
  padding: 12px 18px;
  border-radius: 16px;
  display: flex;
  justify-content: space-between;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  align-items: center;
  border: 1px solid rgba(203, 213, 224, 0.5);
  font-weight: 500;
}

html.light-theme .error-message {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}

.error-text {
  flex-grow: 1;
  text-align: left;
  font-size: 14px;
  margin-right: 10px;
  color: #e53e3e;
}

.error-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.error-ok {
  background: rgba(229, 62, 62, 0.1);
  border: 1px solid rgba(229, 62, 62, 0.3);
  color: #e53e3e;
  padding: 4px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s ease;
}

.error-ok:hover {
  background: rgba(229, 62, 62, 0.2);
}

.error-close {
  background: none;
  border: none;
  color: #e53e3e;
  font-size: 22px;
  cursor: pointer;
  padding: 0 5px;
  transition: all 0.3s ease;
}

.error-close:hover {
  opacity: 0.6;
}

/* Slide-down animation */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.4s ease;
}

.slide-down-enter-from {
  transform: translate(-50%, -100%);
  opacity: 0;
}

.slide-down-leave-to {
  transform: translate(-50%, -100%);
  opacity: 0;
}
</style> 
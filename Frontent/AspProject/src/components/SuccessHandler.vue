<template>
  <transition name="slide-down">
    <div v-if="successMessage" class="success-container">
      <div class="success-message">
        <span class="success-text">{{ successMessage }}</span>
        <div class="success-actions">
    
          <button class="success-close" @click="closeSuccess">×</button>
        </div>
      </div>
    </div>
  </transition>
</template>

<script>
import {ref} from 'vue'

export default {
  name: 'SuccessHandler',

  setup(props, {emit}) {
    const successMessage = ref(null)
    let timeoutId = null

    const handleSuccess = (message) => {
      successMessage.value = message
      // Auto-close after 15 seconds
      if (timeoutId) clearTimeout(timeoutId)
      timeoutId = setTimeout(() => {
        successMessage.value = null
      }, 15000)
    }

    const closeSuccess = () => {
      if (timeoutId) clearTimeout(timeoutId)
      successMessage.value = null
    }

    return {
      successMessage,
      handleSuccess,
      closeSuccess
    }
  }
}
</script>

<style scoped>
.success-container {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 9999;
  width: 80%;
  max-width: 500px;
}

.success-message {
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

.success-text {
  flex-grow: 1;
  text-align: left;
  font-size: 14px;
  margin-right: 10px;
  color: #3182ce;
}

.success-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.success-ok {
  background: rgba(49, 130, 206, 0.1);
  border: 1px solid rgba(49, 130, 206, 0.3);
  color: #3182ce;
  padding: 4px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s ease;
}

.success-ok:hover {
  background: rgba(49, 130, 206, 0.2);
}

.success-close {
  background: none;
  border: none;
  color: #3182ce;
  font-size: 22px;
  cursor: pointer;
  padding: 0 5px;
  transition: all 0.3s ease;
}

.success-close:hover {
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
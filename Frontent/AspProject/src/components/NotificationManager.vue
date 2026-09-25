<template>
  <div class="notification-manager">
    <transition-group name="slide-down">
      <div v-for="notification in notifications" 
           :key="notification.id" 
           class="notification-container"
           :class="{ 'error': notification.type === 'error', 'success': notification.type === 'success' }">
        <div class="notification-message">
          <span class="notification-text">{{ notification.message }}</span>
          <button class="notification-close" @click="removeNotification(notification.id)">×</button>
        </div>
      </div>
    </transition-group>
  </div>
</template>

<script>
export default {
  name: 'NotificationManager',
  
  data() {
    return {
      notifications: [],
      nextId: 1
    }
  },

  methods: {
    addNotification(message, type = 'success') {
      const id = this.nextId++
      this.notifications.push({ id, message, type })
      
      // Auto-remove after 15 seconds
      setTimeout(() => {
        this.removeNotification(id)
      }, 15000)
    },

    removeNotification(id) {
      const index = this.notifications.findIndex(n => n.id === id)
      if (index !== -1) {
        this.notifications.splice(index, 1)
      }
    }
  }
}
</script>

<style scoped>
.notification-manager {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 9999;
  width: 80%;
  max-width: 500px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.notification-container {
  width: 100%;
}

.notification-message {
  background-color: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  padding: 12px 18px;
  border-radius: 16px;
  display: flex;
  justify-content: space-between;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  align-items: center;
  border: 1px solid rgba(203, 213, 224, 0.5);
  font-weight: 500;
}

.notification-text {
  flex-grow: 1;
  text-align: left;
  font-size: 14px;
  margin-right: 10px;
}

.notification-close {
  background: none;
  border: none;
  font-size: 22px;
  cursor: pointer;
  padding: 0 5px;
  transition: all 0.3s ease;
}

.notification-close:hover {
  opacity: 0.6;
}

/* Error notification styles */
.notification-container.error .notification-message {
  border-color: rgba(229, 62, 62, 0.3);
}

.notification-container.error .notification-text {
  color: #e53e3e;
}

.notification-container.error .notification-close {
  color: #e53e3e;
}

/* Success notification styles */
.notification-container.success .notification-message {
  border-color: rgba(49, 130, 206, 0.3);
}

.notification-container.success .notification-text {
  color: #3182ce;
}

.notification-container.success .notification-close {
  color: #3182ce;
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
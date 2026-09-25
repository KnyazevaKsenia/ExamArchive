import Vue from 'vue'
import NotificationManager from '@/components/NotificationManager.vue'

export default {
  install(Vue) {
    // Create a div for notifications
    const notificationDiv = document.createElement('div')
    document.body.appendChild(notificationDiv)

    // Create notification manager instance
    const NotificationManagerConstructor = Vue.extend(NotificationManager)
    const notificationInstance = new NotificationManagerConstructor().$mount(notificationDiv)

    // Add notification methods to the global properties
    Vue.prototype.$notify = {
      success: (message) => {
        notificationInstance.addNotification(message, 'success')
      },
      error: (message) => {
        notificationInstance.addNotification(message, 'error')
      }
    }
  }
} 
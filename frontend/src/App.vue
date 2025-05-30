<script setup lang="ts">
import { ref, watchEffect, provide, onMounted } from 'vue'
import { RouterView } from 'vue-router'
import ThemeToggle from './components/common/ThemeToggle.vue'
import { useRouter } from 'vue-router'
import api from './utils/api'
const isDarkMode = ref(false)
const isLoading = ref(false)
const router = useRouter()
import AlertComponent from '@/components/common/AlertComponent.vue'
import { useAlertStore } from '@/stores/alertStore'

const alertStore = useAlertStore()
// 检查系统颜色模式
const checkSystemColorScheme = () => {
  return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
}

// 从本地存储获取用户之前的选择
const getUserPreference = () => {
  const storedPreference = localStorage.getItem('colorMode')
  if (storedPreference) {
    return storedPreference === 'dark'
  }
  return null
}

// 设置颜色模式
const setColorMode = (isDark: boolean) => {
  isDarkMode.value = isDark
  localStorage.setItem('colorMode', isDark ? 'dark' : 'light')
}

onMounted(() => {
  const userPreference = getUserPreference()
  if (userPreference !== null) {
    setColorMode(userPreference)
  } else {
    setColorMode(checkSystemColorScheme())
  }
  api.post('/', {}).then((res: any) => {
    if (res.code === 200) {
      localStorage.setItem('config', JSON.stringify(res.detail))
      if (
        res.detail.notify_title &&
        res.detail.notify_content &&
        localStorage.getItem('notify') !== res.detail.notify_title + res.detail.notify_content
      ) {
        localStorage.setItem('notify', res.detail.notify_title + res.detail.notify_content)
        alertStore.showAlert(res.detail.notify_title + ': ' + res.detail.notify_content, 'success')
      }
    }
  })
})

watchEffect(() => {
  document.documentElement.classList.toggle('dark', isDarkMode.value)
})

router.beforeEach((_, __, next) => {
  isLoading.value = true
  next()
})

router.afterEach(() => {
  setTimeout(() => {
    isLoading.value = false
  }, 500) // 增加延迟以确保组件已加载
})

provide('isDarkMode', isDarkMode)
provide('setColorMode', setColorMode)
provide('isLoading', isLoading)
</script>

<template>
  <div :class="['app-container', isDarkMode ? 'dark' : 'light']">
    <ThemeToggle v-model="isDarkMode" />
    <div v-if="isLoading" class="loading-overlay">
      <div class="loading-spinner"></div>
    </div>
    
    <!-- 简化的RouterView使用方式，避免transition嵌套问题 -->
    <RouterView />

    <AlertComponent />
  </div>
</template>

<style lang="postcss">

.app-container {
  min-height: 100vh;
  width: 100%;
  transition: background-color 0.5s ease;
}

.light {
/* 移除 @apply 指令，手动实现渐变样式 */
background-image: linear-gradient(to bottom right, #eff6ff, #eef2ff, #ffffff);
}

.dark {
/* 移除 @apply 指令，手动实现渐变样式 */
background-image: linear-gradient(to bottom right, #111827, #312e81, #000000);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 3px solid #fff;
  border-top: 3px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}
</style>

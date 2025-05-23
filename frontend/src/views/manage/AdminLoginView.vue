<template>
  <div :class="[
    'min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-200 relative overflow-hidden',
    isDarkMode ? 'bg-gray-900' : 'bg-gray-50'
  ]">
    <div class="absolute inset-0 z-0">
      <div class="cyber-grid"></div>
      <div class="floating-particles"></div>
    </div>
    <div class="max-w-md w-full space-y-8 backdrop-blur-lg bg-opacity-20 p-8 rounded-xl border border-opacity-20"
      :class="[isDarkMode ? 'bg-gray-800 border-gray-600' : 'bg-white/70 border-gray-200']">
      <div>
        <div class="mx-auto h-16 w-16 relative">
          <div
            class="absolute inset-0 bg-gradient-to-r from-cyan-500 via-purple-500 to-pink-500 rounded-full animate-spin-slow">
          </div>
          <div
            class="absolute -inset-2 bg-gradient-to-r from-cyan-500 via-purple-500 to-pink-500 rounded-full opacity-50 blur-md animate-pulse">
          </div>
          <div :class="[
            'absolute inset-1 rounded-full flex items-center justify-center',
            isDarkMode ? 'bg-gray-800' : 'bg-white'
          ]">
            <ShieldIcon :class="['h-8 w-8', isDarkMode ? 'text-cyan-400' : 'text-cyan-600']" />
          </div>
        </div>
        <h2 :class="[
          'mt-6 text-center text-3xl font-extrabold',
          isDarkMode ? 'text-white' : 'text-gray-900'
        ]">
          管理员登录
        </h2>
      </div>
      <form class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <input type="hidden" name="remember" value="true" />
        <div class="rounded-md shadow-sm -space-y-px">
          <div>
            <label for="email" class="sr-only">邮箱</label>
            <input id="email" name="email" type="email" autocomplete="email" required
              v-model="email" :class="[
                'appearance-none rounded-t-md relative block w-full px-4 py-3 border transition-all duration-200 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-cyan-500 focus:border-cyan-500 focus:z-10 sm:text-sm backdrop-blur-sm',
                isDarkMode
                  ? 'bg-gray-800/50 border-gray-600 text-white placeholder-gray-400 hover:border-gray-500'
                  : 'bg-white/50 border-gray-300 text-gray-900 hover:border-gray-400'
              ]" placeholder="管理员邮箱" />
          </div>
          <div>
            <label for="password" class="sr-only">密码</label>
            <div class="relative">
              <input id="password" name="password" :type="showPassword ? 'text' : 'password'" autocomplete="current-password" required
                v-model="password" :class="[
                  'appearance-none rounded-b-md relative block w-full px-4 py-3 border transition-all duration-200 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-cyan-500 focus:border-cyan-500 focus:z-10 sm:text-sm backdrop-blur-sm',
                  isDarkMode
                    ? 'bg-gray-800/50 border-gray-600 text-white placeholder-gray-400 hover:border-gray-500'
                    : 'bg-white/50 border-gray-300 text-gray-900 hover:border-gray-400'
                ]" placeholder="管理员密码" />
              <button 
                type="button" 
                @click="showPassword = !showPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center focus:outline-none"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                <EyeIcon v-if="!showPassword" class="h-5 w-5" />
                <EyeOffIcon v-else class="h-5 w-5" />
              </button>
            </div>
          </div>
        </div>
        <div>
          <button type="submit" :class="[
            'group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-md text-white transition-all duration-300 transform hover:scale-[1.02] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-cyan-500 shadow-lg hover:shadow-cyan-500/50',
            isDarkMode
              ? 'bg-gradient-to-r from-cyan-500 to-purple-500 hover:from-cyan-600 hover:to-purple-600'
              : 'bg-gradient-to-r from-cyan-600 to-purple-600 hover:from-cyan-700 hover:to-purple-700',
            isLoading ? 'opacity-75 cursor-not-allowed' : ''
          ]" :disabled="isLoading">
            <span class="absolute left-0 inset-y-0 flex items-center pl-3"> </span>
            {{ isLoading ? '登录中...' : '管理员登录' }}
          </button>
        </div>
        
        <div class="flex items-center justify-center">
          <div :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">
            <router-link to="/login" class="font-medium text-cyan-500 hover:text-cyan-400">
              返回用户登录
            </router-link>
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject } from 'vue'
import { ShieldIcon, EyeIcon, EyeOffIcon } from 'lucide-vue-next'
import { AxiosError } from 'axios'
import api from '@/utils/api'
import { useAlertStore } from '@/stores/alertStore'
import { useRouter } from 'vue-router'

const alertStore = useAlertStore()
const email = ref('')
const password = ref('')
const isLoading = ref(false)
const isDarkMode = inject('isDarkMode')
const router = useRouter()
const showPassword = ref(false)

const validateEmail = (email: string) => {
  const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  return emailRegex.test(email);
};

const validateForm = () => {
  let isValid = true
  
  if (!email.value) {
    alertStore.showAlert('请输入有效的邮箱', 'error')
    isValid = false
  } else if (!validateEmail(email.value)) {
    alertStore.showAlert('请输入有效的邮箱地址', 'error')
    isValid = false
  }
  
  if (!password.value) {
    alertStore.showAlert('请输入密码', 'error')
    isValid = false
  }
  
  return isValid
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return;
  }
  
  isLoading.value = true;
  
  try {
    // 清除可能存在的旧凭据
    localStorage.removeItem('adminToken');
    localStorage.removeItem('isAdminLoggedIn');
    localStorage.removeItem('token');
    
    // 使用 api 实例发送管理员登录请求
    const response = await api.post('/api/admin/login', {
      email: email.value,
      password: password.value
    })
    
    if (response.success) {
      // 登录成功
      alertStore.showAlert('管理员登录成功', 'success')
      
      // 存储管理员登录状态
      localStorage.setItem('adminToken', response.data?.token || 'adminLoggedIn')
      localStorage.setItem('isAdminLoggedIn', 'true')
      
      // 验证管理员权限
      try {
        // 验证本地存储中的登录状态
      const isAdmin = localStorage.getItem('isAdminLoggedIn') === 'true';
        if (!isAdmin) {
          throw new Error('管理员登录状态无效');
        }
        // 跳转到管理面板
        window.location.href = '/#/admin';
      } catch (validationError) {
        console.error('验证管理员权限失败:', validationError);
        alertStore.showAlert('管理员权限验证失败，请重新登录', 'error');
        localStorage.removeItem('adminToken');
        localStorage.removeItem('isAdminLoggedIn');
      }
    } else {
      // 登录失败
      alertStore.showAlert(response.message || '管理员登录失败，请检查凭据', 'error')
    }
  } catch (error) {
    console.error('管理员登录请求出错:', error)
    
    const axiosError = error as AxiosError
    
    if (axiosError.response) {
      // 处理错误响应
      const errorData = axiosError.response.data as any
      const errorMessage = errorData?.message || '管理员登录失败，请检查凭据'
      alertStore.showAlert(errorMessage, 'error')
    } else if (axiosError.request) {
      // 网络错误
      alertStore.showAlert('网络错误，无法连接到服务器', 'error')
    } else {
      // 其他错误
      alertStore.showAlert('请求失败: ' + axiosError.message, 'error')
    }
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
@keyframes spin {
  from {
    transform: rotate(0deg);
  }

  to {
    transform: rotate(360deg);
  }
}

.animate-spin-slow {
  animation: spin 8s linear infinite;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

input:focus {
  box-shadow: 0 0 15px rgba(99, 102, 241, 0.3);
}

button:active:not(:disabled) {
  transform: scale(0.98);
}

.cyber-grid {
  background-image: linear-gradient(transparent 95%, rgba(99, 102, 241, 0.1) 50%),
    linear-gradient(90deg, transparent 95%, rgba(99, 102, 241, 0.1) 50%);
  background-size: 30px 30px;
  width: 100%;
  height: 100%;
  position: absolute;
  opacity: 0.5;
}

.floating-particles {
  position: absolute;
  width: 100%;
  height: 100%;
  background: radial-gradient(circle at center, transparent 0%, transparent 100%);
  filter: url(#gooey);
}

.floating-particles::before,
.floating-particles::after {
  content: '';
  position: absolute;
  width: 100%;
  height: 100%;
  background-image: radial-gradient(circle at center, rgba(99, 102, 241, 0.1) 0%, transparent 50%);
  animation: float 20s infinite linear;
}

.floating-particles::after {
  animation-delay: -10s;
  opacity: 0.5;
}

@keyframes float {
  0% {
    transform: translate(0, 0) scale(1);
  }

  50% {
    transform: translate(50px, 50px) scale(1.5);
  }

  100% {
    transform: translate(0, 0) scale(1);
  }
}

button:hover:not(:disabled) {
  box-shadow: 0 0 25px rgba(99, 102, 241, 0.5);
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}
</style> 
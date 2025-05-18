<template>
  <div :class="[
    'min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-200 relative overflow-hidden',
    isDarkMode ? 'bg-gray-900' : 'bg-gray-50'
  ]" @click="handleBackgroundClick">
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
            <BoxIcon :class="['h-8 w-8', isDarkMode ? 'text-cyan-400' : 'text-cyan-600']" />
          </div>
        </div>
        <h2 :class="[
          'mt-6 text-center text-3xl font-extrabold',
          isDarkMode ? 'text-white' : 'text-gray-900'
        ]">
          登录
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
              ]" placeholder="邮箱" />
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
                ]" placeholder="密码" />
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
            {{ isLoading ? '登录中...' : '登录' }}
          </button>
        </div>
        
        <!-- 游客访问按钮 -->
        <div>
          <button type="button" @click="guestAccess" :class="[
            'group relative w-full flex justify-center py-3 px-4 border text-sm font-medium rounded-md transition-all duration-300 transform hover:scale-[1.02] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400 shadow-md',
            isDarkMode
              ? 'bg-gray-700 hover:bg-gray-600 text-gray-200 border-gray-600'
              : 'bg-gray-100 hover:bg-gray-200 text-gray-700 border-gray-300'
          ]">
            游客访问（直接取件）
          </button>
        </div>
        
        <div class="flex items-center justify-center">
          <div :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">
            还没有账号？
            <router-link to="/register" class="font-medium text-cyan-500 hover:text-cyan-400">
              立即注册
            </router-link>
          </div>
        </div>
      </form>
    </div>
  </div>

  <!-- 管理员登录弹窗 -->
  <transition name="fade">
    <div v-if="showAdminDialog" class="fixed inset-0 flex items-center justify-center z-50 p-4">
      <!-- 背景遮罩层，单独设置模糊效果 -->
      <div class="absolute inset-0 bg-black/50 backdrop-filter backdrop-blur-sm" @click="showAdminDialog = false"></div>
      
      <!-- 弹窗容器 -->
      <div class="relative w-full max-w-sm rounded-xl shadow-2xl border transform transition-all duration-300 overflow-hidden"
           :class="[isDarkMode 
              ? 'bg-gradient-to-br from-gray-800 to-gray-900 border-indigo-500/30' 
              : 'bg-white/95 border-gray-200']">
        <!-- 顶部装饰条 -->
        <div class="h-1.5 w-full bg-gradient-to-r from-cyan-400 via-purple-500 to-pink-400"></div>
        
        <!-- 弹窗内容 -->
        <div class="p-6">
          <h3 class="text-xl font-bold mb-5 text-center" 
              :class="[isDarkMode ? 'text-transparent bg-clip-text bg-gradient-to-r from-cyan-300 to-purple-300' : 'text-gray-900']">
            管理员登录
          </h3>
          <form @submit.prevent="handleAdminLogin" class="space-y-4">
            <div>
              <label for="adminEmail" class="block text-sm font-medium mb-1" 
                     :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">邮箱</label>
              <div class="relative">
                <input
                  id="adminEmail"
                  v-model="adminEmail"
                  type="email"
                  placeholder="管理员邮箱"
                  class="w-full px-4 py-2.5 rounded-lg border focus:ring-2 focus:ring-cyan-500 focus:outline-none transition-colors"
                  :class="[isDarkMode 
                    ? 'bg-gray-700/70 border-gray-600 text-white placeholder-gray-400' 
                    : 'bg-white border-gray-300 text-gray-900 placeholder-gray-500']"
                  required
                />
              </div>
            </div>
            <div>
              <label for="adminPassword" class="block text-sm font-medium mb-1"
                     :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">密码</label>
              <div class="relative">
                <input
                  id="adminPassword"
                  v-model="adminPassword"
                  :type="showAdminPassword ? 'text' : 'password'"
                  placeholder="管理员密码"
                  class="w-full px-4 py-2.5 rounded-lg border focus:ring-2 focus:ring-cyan-500 focus:outline-none transition-colors"
                  :class="[isDarkMode 
                    ? 'bg-gray-700/70 border-gray-600 text-white placeholder-gray-400' 
                    : 'bg-white border-gray-300 text-gray-900 placeholder-gray-500']"
                  required
                />
                <button 
                  type="button" 
                  @click="showAdminPassword = !showAdminPassword"
                  class="absolute inset-y-0 right-0 pr-3 flex items-center focus:outline-none"
                  :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                  <EyeIcon v-if="!showAdminPassword" class="h-5 w-5" />
                  <EyeOffIcon v-else class="h-5 w-5" />
                </button>
              </div>
            </div>
            <div class="pt-3 space-y-3">
              <button
                type="submit"
                :disabled="adminLoading"
                class="w-full py-2.5 px-4 rounded-lg font-medium shadow-lg transition-all duration-300 transform hover:translate-y-[-1px] focus:outline-none focus:ring-2 focus:ring-offset-2 text-white"
                :class="[
                  isDarkMode 
                    ? 'bg-gradient-to-r from-cyan-500 to-indigo-500 hover:from-cyan-400 hover:to-indigo-400 focus:ring-cyan-500/50 focus:ring-offset-gray-900' 
                    : 'bg-gradient-to-r from-cyan-600 to-indigo-600 hover:from-cyan-500 hover:to-indigo-500 focus:ring-cyan-500/50 focus:ring-offset-white',
                  adminLoading ? 'opacity-70 cursor-not-allowed' : ''
                ]"
              >
                <span class="flex items-center justify-center">
                  <span v-if="adminLoading" class="mr-2">
                    <svg class="animate-spin h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                  </span>
                  <span>{{ adminLoading ? '登录中...' : '登录' }}</span>
                </span>
              </button>
              <button
                type="button"
                @click="showAdminDialog = false"
                class="w-full py-2.5 px-4 rounded-lg font-medium transition-colors"
                :class="[isDarkMode 
                  ? 'bg-gray-700/80 hover:bg-gray-600 text-gray-300' 
                  : 'bg-gray-100 hover:bg-gray-200 text-gray-600']"
              >取消</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { ref, inject } from 'vue'
import { BoxIcon, EyeIcon, EyeOffIcon } from 'lucide-vue-next'
import axios, { AxiosError } from 'axios'
import api from '@/utils/api'
import { useAlertStore } from '@/stores/alertStore'
import { useAdminData } from '@/stores/adminStore'
import { useRouter } from 'vue-router'

const alertStore = useAlertStore()
const email = ref('')
const password = ref('')
const isLoading = ref(false)
const isDarkMode = inject('isDarkMode')
const adminStore = useAdminData()
const router = useRouter()
const showPassword = ref(false)

// 游客访问函数
const guestAccess = () => {
  router.push('/retrieve')
}

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
    alertStore.showAlert('无效的密码', 'error')
    isValid = false
  } else if (password.value.length < 6) {
    alertStore.showAlert('密码长度至少为6位', 'error')
    isValid = false
  }
  return isValid
}
const handleSubmit = async () => {
  if (!validateForm()) {
    console.log('表单验证未通过');
    return;
  }
  isLoading.value = true;
  try {
    console.log('开始发送登录请求');
    
    // 构建请求数据
    const requestData = {
      email: email.value,
      password: password.value
    };
    console.log('请求数据:', requestData);
    
    // 使用 api 实例发送请求
    const response = await api.post('/api/user/login', requestData);
    
    console.log('登录请求已发送，完整响应:', response);
    console.log('响应数据:', response);

    if (response.success) {
      // 请求成功，显示成功信息并跳转
      alertStore.showAlert('登录成功', 'success');
      console.log('登录请求成功，准备跳转到 /send 页面');
      
      // 如果需要保存用户数据
      if (response.data?.user) {
        // 存储用户数据
        localStorage.setItem('userData', JSON.stringify(response.data.user));
      }
      
      // 添加登录标志
      localStorage.setItem('token', 'loggedIn');
      localStorage.setItem('isLoggedIn', 'true');
      
      // 使用window.location.href进行硬跳转，避免Vue路由问题
      window.location.href = '/#/send';
    } else {
      // 请求未成功，显示错误信息
      console.log('登录请求未成功');
      alertStore.showAlert(response.message || '登录请求未成功，请稍后重试', 'error');
    }
  } catch (error) {
    console.error('登录请求出错:', error);
    
    const axiosError = error as AxiosError;
    
    if (axiosError.response) {
      console.log('错误响应状态码:', axiosError.response.status);
      console.log('错误响应数据:', axiosError.response.data);
      // 提取并显示具体的错误信息
      const errorData = axiosError.response.data as any;
      const errorMessage = errorData?.message || 
                          (typeof errorData === 'object' ? JSON.stringify(errorData) : '登录请求出错，请稍后重试');
      alertStore.showAlert(errorMessage, 'error');
    } else if (axiosError.request) {
      // 请求已发出但未收到响应
      console.error('未收到服务器响应，请检查后端服务是否正在运行', axiosError.request);
      alertStore.showAlert('网络错误，无法连接到服务器，请确保后端服务正在运行', 'error');
    } else {
      // 请求设置有误
      console.error('请求配置错误:', axiosError.message);
      alertStore.showAlert(`请求配置错误: ${axiosError.message}`, 'error');
    }
  } finally {
    isLoading.value = false;
  }
};

// 管理员登录相关
const showAdminDialog = ref(false)
const adminEmail = ref('')
const adminPassword = ref('')
const adminLoading = ref(false)
const showAdminPassword = ref(false)

// 点击次数跟踪
const clickCount = ref(0)
let clickTimer: any = null

// 处理背景点击事件
const handleBackgroundClick = (e: MouseEvent) => {
  // 获取目标元素和登录容器
  const target = e.target as HTMLElement;
  const loginContainer = document.querySelector('.max-w-md');
  
  // 检查点击是否在登录容器内，如果是则不触发计数
  if (loginContainer && (loginContainer === target || loginContainer.contains(target))) {
    return;
  }
  
  // 确保点击是在背景元素上 - 检查类名中是否包含cyber-grid或floating-particles
  const isBackgroundElement = 
    target.classList.contains('cyber-grid') || 
    target.classList.contains('floating-particles') ||
    (target.classList.contains('min-h-screen') && e.target === e.currentTarget);
  
  if (!isBackgroundElement) {
    return;
  }
  
  // 增加计数
  clickCount.value++;
  console.log(`点击计数: ${clickCount.value}`); // 调试信息
  
  // 清除之前的定时器
  if (clickTimer) clearTimeout(clickTimer);
  
  // 设置新的定时器，5秒后重置计数
  clickTimer = setTimeout(() => {
    console.log('计数重置'); // 调试信息
    clickCount.value = 0;
  }, 300);
  
  // 当点击次数达到3次时显示管理员登录弹窗
  if (clickCount.value === 3) {
    console.log('触发管理员登录弹窗'); // 调试信息
    showAdminDialog.value = true;
    clickCount.value = 0;
  }
}

// 管理员登录处理
const handleAdminLogin = async () => {
  if (!adminEmail.value || !adminPassword.value) {
    alertStore.showAlert('请输入管理员邮箱和密码', 'error')
    return
  }
  
  adminLoading.value = true
  
  try {
    // 使用 api 实例发送管理员登录请求
    const response = await api.post('/api/admin/login', {
      email: adminEmail.value,
      password: adminPassword.value
    })
    
    console.log('管理员登录响应:', response)
    
    if (response.success) {
      // 登录成功
      alertStore.showAlert('管理员登录成功', 'success')
      
      // 存储管理员登录状态
      localStorage.setItem('adminToken', 'adminLoggedIn')
      localStorage.setItem('isAdminLoggedIn', 'true')
      
      // 关闭对话框并跳转到管理页面
      showAdminDialog.value = false
      
      // 使用window.location.href进行硬跳转，避免Vue路由问题
      window.location.href = '/#/admin';
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
    adminLoading.value = false
  }
}
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
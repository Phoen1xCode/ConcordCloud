<template>
  <div
    class="min-h-screen flex flex-col lg:flex-row transition-colors duration-300 relative"
    :class="[isDarkMode ? 'bg-gray-900' : 'bg-gray-50']"
  >
        <!-- Sidebar -->    <aside      class="fixed inset-y-0 left-0 z-50 w-64 transform transition-all duration-300 ease-in-out lg:relative lg:translate-x-0 border-r flex flex-col lg:min-h-screen"      :class="[        isDarkMode          ? 'bg-gray-800 bg-opacity-90 backdrop-filter backdrop-blur-xl border-gray-700'          : 'bg-white border-gray-200',        { '-translate-x-full': !isSidebarOpen }      ]"
    >
      <!-- Logo区域 -->
      <div
        class="flex items-center justify-between h-16 px-4 border-b"
        :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']"
      >
        <div class="flex items-center">
          <div
            class="rounded-full bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 p-1 animate-spin-slow"
          >
            <div class="rounded-full p-1" :class="[isDarkMode ? 'bg-gray-800' : 'bg-white']">
              <HomeIcon
                class="w-6 h-6"
                :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-600']"
              />
            </div>
          </div>
          <h1
            class="ml-2 text-xl font-semibold"
            :class="[isDarkMode ? 'text-white' : 'text-gray-800']"
          >
            ConcordCloud
          </h1>
        </div>
        <button @click="toggleSidebar" class="lg:hidden">
          <XIcon class="w-6 h-6" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']" />
        </button>
      </div>

      <!-- 导航菜单 -->
      <nav class="flex-1 overflow-y-auto">
        <ul class="p-4 space-y-2">
          <li v-for="item in menuItems" :key="item.id">
            <a
              @click="router.push(item.redirect)"
              class="flex items-center p-2 rounded-lg transition-colors duration-200"
              :class="[
                router.currentRoute.value.name === item.id
                  ? isDarkMode
                    ? 'bg-indigo-900 text-indigo-400'
                    : 'bg-indigo-100 text-indigo-600'
                  : isDarkMode
                    ? 'text-gray-400 hover:bg-gray-700'
                    : 'text-gray-600 hover:bg-gray-100'
              ]"
            >
              <component :is="item.icon" class="w-5 h-5 mr-3" />
              {{ item.name }}
            </a>
          </li>
        </ul>
      </nav>

      <!-- 退出登录按钮 (位于侧边栏底部) -->
      <div class="mt-auto border-t p-4" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
        <button 
          @click="handleLogout" 
          class="flex items-center w-full p-2 rounded-lg transition-colors duration-200"
          :class="[
            isDarkMode 
              ? 'text-red-400 hover:bg-gray-700' 
              : 'text-red-600 hover:bg-gray-100'
          ]"
        >
          <LogOutIcon class="w-5 h-5 mr-3" />
          退出登录
        </button>
      </div>
    </aside>

        <!-- Main Content -->    <div class="flex-1 flex flex-col">      <!-- Header -->      <header        class="shadow-md border-b transition-colors duration-300"        :class="[isDarkMode ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200']"      >        <div class="flex items-center justify-between h-16 px-4">          <button @click="toggleSidebar" class="lg:hidden">            <MenuIcon class="w-6 h-6" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']" />          </button>        </div>      </header>      <!-- Content -->      <main        class="flex-1 p-6 transition-colors duration-300"        :class="[isDarkMode ? 'bg-gray-900' : 'bg-gray-50']"      >        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, onUnmounted } from 'vue'
import { HomeIcon, MenuIcon, XIcon, FolderIcon, CogIcon, LayoutDashboardIcon, UsersIcon, LogOutIcon } from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import api from '@/utils/api'
import { useAlertStore } from '@/stores/alertStore'

interface MenuItem {
  id: string
  name: string
  icon: any
  redirect: string
}

const router = useRouter()
const isDarkMode = inject('isDarkMode')
const alertStore = useAlertStore()
const menuItems: MenuItem[] = [
  { id: 'Dashboard', name: '仪表盘', icon: LayoutDashboardIcon, redirect: '/admin/dashboard' },
  { id: 'FileManage', name: '文件管理', icon: FolderIcon, redirect: '/admin/files' },
  { id: 'UserManage', name: '用户管理', icon: UsersIcon, redirect: '/admin/users' },
  { id: 'Settings', name: '系统设置', icon: CogIcon, redirect: '/admin/settings' }
]

const isSidebarOpen = ref(true)
const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

// 响应式处理
const handleResize = () => {
  if (window.innerWidth >= 1024) {
    isSidebarOpen.value = true
  } else {
    isSidebarOpen.value = false
  }
}

// 分页参数
const params = ref({
  page: 1,
  size: 10,
  total: 0
})

// 加载文件列表
const loadFiles = async () => {
  try {
    params.value.total = 85
    // 更新文件列表数据...
    console.log('管理员界面: 文件数据加载完成');
  } catch (error) {
    console.error('加载文件列表失败:', error)
    // 处理错误...
  }
}

// 组件挂载时执行
onMounted(() => {
  console.log('管理员布局组件已挂载');
  
  // 检查管理员登录状态
  const isAdmin = localStorage.getItem('isAdminLoggedIn') === 'true';
  if (!isAdmin) {
    console.log('检测到未登录状态，但使用硬刷新方式处理');
  }
  
  // 初始化界面
  handleResize();
  window.addEventListener('resize', handleResize);
  
  // 加载数据
  loadFiles();
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})

const handleLogout = async () => {
  // 添加确认对话框
  if (!confirm('确定要退出登录吗？')) {
    return;
  }
  
  try {
    // 调用退出登录API
    await api({
      url: '/api/admin/logout',
      method: 'post'
    });
    
    // 清除登录状态
    localStorage.removeItem('isAdminLoggedIn');
    localStorage.removeItem('token');
    
    // 提示信息
    alertStore.showAlert('退出登录成功', 'success');
    
    // 重定向到登录页面
    router.push('/login');
  } catch (error) {
    console.error('退出登录失败:', error);
    alertStore.showAlert('退出操作已完成', 'info');
    
    // 即使API失败，也强制退出
    localStorage.removeItem('isAdminLoggedIn');
    localStorage.removeItem('token');
    router.push('/login');
  }
}
</script>

<style>
.switch {
  position: relative;
  display: inline-block;
  width: 60px;
  height: 34px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #e5e7eb;
  transition: 0.4s;
}

.dark .slider {
  background-color: #4b5563;
}

input:checked + .slider {
  background-color: #4f46e5;
}

.dark input:checked + .slider {
  background-color: #4f46e5;
}

.slider:before {
  position: absolute;
  content: '';
  height: 26px;
  width: 26px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: 0.4s;
}

.dark .slider:before {
  background-color: #e5e7eb;
}

.slider.round {
  border-radius: 34px;
}

.slider.round:before {
  border-radius: 50%;
}

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

.transition-colors {
  transition-property: background-color, border-color, color, fill, stroke;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  transition-duration: 300ms;
}

.custom-scrollbar {
  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: transparent;
  }

  &::-webkit-scrollbar-thumb {
    background-color: #cbd5e0;
    border-radius: 4px;

    &:hover {
      background-color: #a0aec0;
    }
  }

  /* 适配暗黑模式 */
  :deep(.dark &::-webkit-scrollbar-thumb) {
    background-color: #4a5568;

    &:hover {
      background-color: #2d3748;
    }
  }
}

/* 确保内容区域不会被截断 */
.space-y-6 {
  margin-bottom: 5rem;
}
</style>

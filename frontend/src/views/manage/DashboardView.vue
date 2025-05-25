<template>  <div class="p-6 min-h-fit custom-scrollbar">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
        仪表盘
      </h2>
      <button @click="openAdminModal" class="px-4 py-2 rounded-md transition-colors duration-300 mr-14"
        :class="[isDarkMode ? 'bg-blue-600 hover:bg-blue-700 text-white' : 'bg-blue-500 hover:bg-blue-600 text-white']">
        管理员账户
      </button>
    </div>

    <!-- 统计卡片区域 -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="p-6 rounded-lg shadow-md transition-colors duration-300"
        :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              总文件数
            </p>
            <h3 class="text-2xl font-bold mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
              {{ dashboardData.totalFiles }}
            </h3>
          </div>
          <div class="p-3 rounded-full" :class="[isDarkMode ? 'bg-indigo-900' : 'bg-indigo-100']">
            <FileIcon class="w-6 h-6" :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-600']" />
          </div>
        </div>
        <p class="text-sm mt-2" :class="[isDarkMode ? 'text-green-400' : 'text-green-600']">
          <span :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">上周新增：</span>
          <span>{{ dashboardData.newFilesLastWeek }}</span>
        </p>
      </div>

      <div class="p-6 rounded-lg shadow-md transition-colors duration-300"
        :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              存储空间
            </p>
            <h3 class="text-2xl font-bold mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
              {{ dashboardData.storageUsed }}
            </h3>
          </div>
          <div class="p-3 rounded-full" :class="[isDarkMode ? 'bg-purple-900' : 'bg-purple-100']">
            <HardDriveIcon class="w-6 h-6" :class="[isDarkMode ? 'text-purple-400' : 'text-purple-600']" />
          </div>
        </div>
      </div>

      <div class="p-6 rounded-lg shadow-md transition-colors duration-300"
        :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              用户总数
            </p>
            <h3 class="text-2xl font-bold mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
              {{ dashboardData.totalUsers }}
            </h3>
          </div>
          <div class="p-3 rounded-full" :class="[isDarkMode ? 'bg-green-900' : 'bg-green-100']">
            <UsersIcon class="w-6 h-6" :class="[isDarkMode ? 'text-green-400' : 'text-green-600']" />
          </div>
        </div>
        <p class="text-sm mt-2" :class="[isDarkMode ? 'text-green-400' : 'text-green-600']">
          <span :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">上周新增：</span>
          <span>{{ dashboardData.newUsersLastWeek }}</span>
        </p>
      </div>

      <div class="p-6 rounded-lg shadow-md transition-colors duration-300"
        :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              系统状态
            </p>
            <h3 class="text-2xl font-bold mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
              正常
            </h3>
          </div>
          <div class="p-3 rounded-full" :class="[isDarkMode ? 'bg-blue-900' : 'bg-blue-100']">
            <ActivityIcon class="w-6 h-6" :class="[isDarkMode ? 'text-blue-400' : 'text-blue-600']" />
          </div>
        </div>
        <p class="text-sm mt-2" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
          网页运行时间: {{ dashboardData.sysUptime }}
        </p>
      </div>
    </div>

    <!-- 添加用户列表 -->
    <div class="mb-8 rounded-lg shadow-md transition-colors duration-300 overflow-hidden"
      :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
      <div class="p-6 border-b" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
        <h3 class="text-xl font-bold" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
          用户排名
        </h3>
      </div>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y" :class="[isDarkMode ? 'divide-gray-700' : 'divide-gray-200']">
          <thead :class="[isDarkMode ? 'bg-gray-800' : 'bg-gray-50']">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                用户
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                角色
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                文件数
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                存储空间
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                最后登录
              </th>
            </tr>
          </thead>
          <tbody :class="[isDarkMode ? 'divide-y divide-gray-700' : 'divide-y divide-gray-200']">
            <tr v-for="user in dashboardData.topUsers" :key="user.id"
              :class="[isDarkMode ? 'bg-gray-800' : 'bg-white']">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="ml-4">
                    <div class="text-sm font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
                      {{ user.email }}
                    </div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-500']">{{ user.role }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-500']">{{ user.filesCount }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-500']">
                  {{ getLocalstorageUsed(user.totalStorageUsed) }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm"
                :class="[isDarkMode ? 'text-gray-300' : 'text-gray-500']">
                {{ formatDate(user.lastLoginAt) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 添加版本和版权信息 -->
    <div class="mt-auto text-center py-4" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
      <p class="text-sm">
        ConcordCloud v3.0.0
      </p>
      <p class="text-sm mt-1">
        © {{ new Date().getFullYear() }} ConcordCloud
      </p>
    </div>
  </div>

  <!-- 管理员账户弹窗 -->
  <div v-if="showAdminModal" class="fixed inset-0 flex items-center justify-center z-50">
    <!-- 背景遮罩层，单独设置模糊效果 -->
    <div class="absolute inset-0 bg-black/50 backdrop-filter backdrop-blur-sm" @click="closeAdminModal"></div>
    
    <div class="relative p-6 rounded-lg shadow-xl w-full max-w-md"
      :class="[isDarkMode ? 'bg-gray-800' : 'bg-white']">
      <button @click="closeAdminModal" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
        <XIcon class="w-5 h-5" />
      </button>
      
      <h3 class="text-xl font-bold mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">管理员账户</h3>
      
      <div v-if="adminProfile">
        <div class="mb-4">
          <p class="text-sm font-medium" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">邮箱</p>
          <p class="mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">{{ adminProfile.email }}</p>
        </div>
        
        <div class="mb-4">
          <p class="text-sm font-medium" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">角色</p>
          <p class="mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">{{ adminProfile.role }}</p>
        </div>
        
        <div class="mb-4">
          <p class="text-sm font-medium" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">上次登录</p>
          <p class="mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
            {{ adminProfile.lastLoginAt ? formatDate(adminProfile.lastLoginAt) : '未知' }}
          </p>
        </div>
        
        <div class="border-t" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
          <h4 class="text-lg font-medium mt-4 mb-3" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">修改密码</h4>
          
          <div class="mb-4">
            <label class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
              当前密码
            </label>
            <div class="relative">
              <input :type="showCurrentPassword ? 'text' : 'password'" v-model="passwordForm.currentPassword"
                class="w-full px-3 py-2 border rounded-md"
                :class="['appearance-none rounded-b-md relative block w-full px-4 py-3 border transition-all duration-200 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-cyan-500 focus:border-cyan-500 focus:z-10 sm:text-sm backdrop-blur-sm',
                isDarkMode ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-800']">
              <button 
                type="button" 
                @click="showCurrentPassword = !showCurrentPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center focus:outline-none"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                <EyeIcon v-if="!showCurrentPassword" class="h-5 w-5" />
                <EyeOffIcon v-else class="h-5 w-5" />
              </button>
            </div>
          </div>
          
          <div class="mb-4">
            <label class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
              新密码
            </label>
            <div class="relative">
              <input :type="showNewPassword ? 'text' : 'password'" v-model="passwordForm.newPassword"
                class="w-full px-3 py-2 border rounded-md"
                :class="['appearance-none rounded-b-md relative block w-full px-4 py-3 border transition-all duration-200 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-cyan-500 focus:border-cyan-500 focus:z-10 sm:text-sm backdrop-blur-sm',
                isDarkMode ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-800']">
              <button 
                type="button" 
                @click="showNewPassword = !showNewPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center focus:outline-none"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                <EyeIcon v-if="!showNewPassword" class="h-5 w-5" />
                <EyeOffIcon v-else class="h-5 w-5" />
              </button>
            </div>
          </div>
          
          <div class="mb-4">
            <label class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
              确认新密码
            </label>
            <div class="relative">
              <input :type="showConfirmPassword ? 'text' : 'password'" v-model="passwordForm.confirmPassword"
                class="w-full px-3 py-2 border rounded-md"
                :class="['appearance-none rounded-b-md relative block w-full px-4 py-3 border transition-all duration-200 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-cyan-500 focus:border-cyan-500 focus:z-10 sm:text-sm backdrop-blur-sm',
                isDarkMode ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-800']">
              <button 
                type="button" 
                @click="showConfirmPassword = !showConfirmPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center focus:outline-none"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                <EyeIcon v-if="!showConfirmPassword" class="h-5 w-5" />
                <EyeOffIcon v-else class="h-5 w-5" />
              </button>
            </div>
          </div>
          
          <button @click="changePassword" class="w-full px-4 py-2 rounded-md font-medium transition-colors duration-300"
            :class="[isDarkMode ? 'bg-blue-600 hover:bg-blue-700 text-white' : 'bg-blue-500 hover:bg-blue-600 text-white']"
            :disabled="isLoading">
            <span v-if="isLoading">处理中...</span>
            <span v-else>更新密码</span>
          </button>
        </div>
      </div>
      
      <div v-else class="flex justify-center items-center h-40">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2" :class="[isDarkMode ? 'border-blue-400' : 'border-blue-600']"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { inject, onMounted, reactive, onUnmounted, ref } from 'vue'
import {
  FileIcon,
  HardDriveIcon,
  UsersIcon,
  ActivityIcon,
  XIcon,
  EyeIcon,
  EyeOffIcon
} from 'lucide-vue-next'
const isDarkMode = inject('isDarkMode')
import api from '@/utils/api'
import { useAlertStore } from '@/stores/alertStore'

const alertStore = useAlertStore()
const dashboardData: any = reactive({
  totalFiles: 0,
  totalUsers: 0,
  storageUsed: 0,
  newUsersLastWeek: 0,
  newFilesLastWeek: 0,
  topUsers: [],
  sysUptime: 0,
  pageLoadTime: Date.now()
})

// 管理员账户相关状态
interface AdminProfile {
  email: string;
  role: string;
  lastLoginAt?: string;
  [key: string]: any;
}

const showAdminModal = ref(false)
const adminProfile = ref<AdminProfile | null>(null)
const isLoading = ref(false)

// 密码显示状态控制
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// 处理时间差的函数
const formatUptime = (milliseconds: number) => {
  const seconds = Math.floor(milliseconds / 1000)
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)
  const days = Math.floor(hours / 24)
  
  const remainingHours = hours % 24
  const remainingMinutes = minutes % 60
  
  return `${days}天${remainingHours}小时${remainingMinutes}分`
}

// 旧函数保留用于其他地方
const getSysUptime = (startTimestamp: number) => {
  const now = new Date().getTime()
  const uptime = now - startTimestamp
  const days = Math.floor(uptime / (24 * 60 * 60 * 1000))
  const hours = Math.floor((uptime % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000))
  return `${days}天${hours}小时`
}

const getLocalstorageUsed = (nowUsedBit: string) => {
  const kb = parseInt(nowUsedBit) / 1024
  const mb = kb / 1024
  const gb = mb / 1024
  const tb = gb / 1024
  // 根据大小选择合适的单位
  if (tb > 1) {
    return `${tb.toFixed(2)}TB`
  } else if (gb > 1) {
    return `${gb.toFixed(2)}GB`
  } else if (mb > 1) {
    return `${mb.toFixed(2)}MB`
  } else if (kb > 1) {
    return `${kb.toFixed(2)}KB`
  } else {
    return `${nowUsedBit}B`
  }
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', { 
    year: 'numeric', 
    month: '2-digit', 
    day: '2-digit',
    hour: '2-digit', 
    minute: '2-digit'
  })
}

const updatePageUptime = () => {
  const now = Date.now()
  const uptime = now - dashboardData.pageLoadTime
  dashboardData.sysUptime = formatUptime(uptime)
}

const getDashboardData = async () => {
  try {
    const response: any = await api.get('api/admin/statistics')
    if (response.success && response.data) {
      dashboardData.totalFiles = response.data.totalFiles
      dashboardData.totalUsers = response.data.totalUsers
      dashboardData.storageUsed = getLocalstorageUsed(response.data.totalStorageUsed)
      dashboardData.newFilesLastWeek = response.data.newFilesLastWeek
      dashboardData.newUsersLastWeek = response.data.newUsersLastWeek
      dashboardData.topUsers = response.data.topUsers?.$values || []
      
      // 更新页面运行时间
      updatePageUptime()
    }
  } catch (error) {
    console.error('Error fetching dashboard data:', error)
    updatePageUptime()
  }
}

// 打开管理员弹窗
const openAdminModal = async () => {
  showAdminModal.value = true
  await getAdminProfile()
}

// 关闭管理员弹窗
const closeAdminModal = () => {
  showAdminModal.value = false
  passwordForm.currentPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
}

// 获取管理员资料
const getAdminProfile = async () => {
  try {
    const response = await api.get('api/admin/profile')
    if (response.success) {
      adminProfile.value = response.data
    } else {
      alertStore.showAlert(response.message || '获取管理员资料失败', 'error')
    }
  } catch (error) {
    console.error('获取管理员资料错误:', error)
    alertStore.showAlert('获取管理员资料失败', 'error')
  }
}

// 修改密码
const changePassword = async () => {
  // 验证密码
  if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
    alertStore.showAlert('请填写所有密码字段', 'error')
    return
  }
  
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    alertStore.showAlert('两次输入的新密码不一致', 'error')
    return
  }
  
  if (passwordForm.newPassword.length < 6) {
    alertStore.showAlert('新密码长度至少为6位', 'error')
    return
  }
  
  isLoading.value = true
  
  try {
    const response = await api.post('api/admin/change-password', {
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
      confirmPassword: passwordForm.confirmPassword
    })
    
    if (response.success) {
      alertStore.showAlert('密码修改成功', 'success')
      // 清空表单
      passwordForm.currentPassword = ''
      passwordForm.newPassword = ''
      passwordForm.confirmPassword = ''
    } else {
      // 根据可能的错误码或错误信息匹配更详细的错误提示
      if (response.message?.toLowerCase().includes('current password')) {
        alertStore.showAlert('当前密码不正确，请重新输入', 'error')
      } else if (response.message?.toLowerCase().includes('complexity')) {
        alertStore.showAlert('新密码不符合复杂度要求，请包含大小写字母、数字和特殊符号', 'error')
      } else if (response.message?.toLowerCase().includes('same')) {
        alertStore.showAlert('新密码不能与当前密码相同', 'error')
      } else {
        alertStore.showAlert(response.message || '密码修改失败', 'error')
      }
    }
  } catch (error: any) {
    console.error('修改密码错误:', error)
    
    // 尝试从错误对象中获取更详细的信息
    if (error.response?.data?.message) {
      if (error.response.status === 400) {
        alertStore.showAlert('密码验证失败，请检查当前密码是否正确', 'error')
      } else if (error.response.status === 401) {
        alertStore.showAlert('登录已过期，请重新登录后再尝试', 'error')
      } else if (error.response.status === 403) {
        alertStore.showAlert('没有权限执行此操作', 'error')
      } else {
        alertStore.showAlert(error.response.data.message, 'error')
      }
    } else {
      alertStore.showAlert('修改密码失败，请稍后重试', 'error')
    }
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  getDashboardData()
  
  // 设置定时器每秒更新一次页面运行时间
  const uptimeInterval = setInterval(updatePageUptime, 1000)
  
  // 在组件卸载时清除定时器
  onUnmounted(() => {
    clearInterval(uptimeInterval)
  })
})
</script>

<style scoped>
/* 防止弹窗关闭按钮与暗黑模式切换按钮的重叠 */
</style>

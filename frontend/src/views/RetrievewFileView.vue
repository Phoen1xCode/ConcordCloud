<template>
  <div class="min-h-screen flex items-center justify-center p-4 overflow-hidden transition-colors duration-300"
    :class="[isDarkMode ? 'bg-gray-900' : 'bg-gray-50']">
    <div class="w-full max-w-md relative z-10">
      <div class="rounded-3xl shadow-2xl overflow-hidden border transform transition-all duration-300" :class="[
        isDarkMode
          ? 'bg-gray-800 bg-opacity-50 backdrop-filter backdrop-blur-xl border-gray-700'
          : 'bg-white border-gray-200'
      ]">
        <div class="p-8">
          <div class="flex justify-center mb-8">
            <div class="rounded-full bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 p-1 animate-spin-slow">
              <div :class="['rounded-full p-2', isDarkMode ? 'bg-gray-900' : 'bg-white']">
                <FileIcon class="w-8 h-8" :class="[isDarkMode ? 'text-white' : 'text-indigo-600']" />
              </div>
            </div>
          </div>
          <h2 class="text-3xl font-extrabold text-center mb-6" :class="[
            isDarkMode
              ? 'text-transparent bg-clip-text bg-gradient-to-r from-indigo-300 via-purple-300 to-pink-300'
              : 'text-indigo-600'
          ]">
            文件下载
          </h2>
          
          <div v-if="!selectedFile">
            <div class="mb-6 relative">
              <label for="shareCode" class="block text-sm font-medium mb-2"
                :class="[isDarkMode ? 'text-gray-300' : 'text-gray-800']">分享码</label>
              <div class="relative">
                <input id="shareCode" v-model="shareCode" type="text"
                  class="w-full px-4 py-3 rounded-lg placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition duration-300"
                  :class="[
                    isDarkMode ? 'bg-gray-700 bg-opacity-50 text-gray-300' : 'bg-gray-100 text-gray-800',
                    { 'ring-2 ring-red-500': error }
                  ]" placeholder="请输入分享码" required :readonly="inputStatus.loading"
                  @focus="isInputFocused = true" @blur="isInputFocused = false" />
                <div v-if="inputStatus.loading" class="absolute inset-y-0 right-0 flex items-center pr-3">
                  <span class="animate-spin rounded-full h-5 w-5 border-b-2 border-indigo-500"></span>
                </div>
              </div>
              <div
                class="absolute -bottom-0.5 left-2 h-0.5 bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 transition-all duration-300 ease-in-out"
                :class="{ 'w-97-100': isInputFocused, 'w-0': !isInputFocused }"></div>
            </div>
            <button @click="handleSubmit"
              class="w-full bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 text-white font-bold py-3 px-4 rounded-lg hover:from-indigo-600 hover:via-purple-600 hover:to-pink-600 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-opacity-50 transition duration-300 transform hover:scale-105 hover:shadow-lg relative overflow-hidden group"
              :disabled="inputStatus.loading || !shareCode">
              <span class="flex items-center justify-center relative z-10">
                <span>{{ inputStatus.loading ? '处理中...' : '下载文件' }}</span>
                <DownloadIcon class="w-5 h-5 ml-2 transition-transform duration-300 transform group-hover:translate-y-1" />
              </span>
              <span
                class="absolute top-0 left-0 w-full h-full bg-gradient-to-r from-pink-500 via-purple-500 to-indigo-500 opacity-0 group-hover:opacity-100 transition-opacity duration-300"></span>
            </button>
          </div>
          
          <div v-else class="mt-6 space-y-6">
            <div class="bg-opacity-10 rounded-lg p-4" 
              :class="[isDarkMode ? 'bg-indigo-900' : 'bg-indigo-50']">
              <h3 class="text-xl font-semibold mb-2" 
                :class="[isDarkMode ? 'text-indigo-300' : 'text-indigo-700']">文件信息</h3>
              <div class="space-y-2">
                <div class="flex items-center">
                  <FileIcon class="w-5 h-5 mr-3" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']" />
                  <p class="truncate" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
                    {{ selectedFile.fileName }}
                  </p>
                </div>
                <div class="flex items-center">
                  <HardDriveIcon class="w-5 h-5 mr-3" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']" />
                  <p :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
                    {{ formatFileSize(selectedFile.fileSize) }}
                  </p>
                </div>
                <div class="flex items-center">
                  <CalendarIcon class="w-5 h-5 mr-3" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']" />
                  <p :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
                    有效期至: {{ formatDate(selectedFile.expiresAt) }}
                  </p>
                </div>
              </div>
            </div>
            
            <button @click="downloadFile" 
              class="w-full flex items-center justify-center py-3 px-4 rounded-lg text-white font-medium shadow-md hover:shadow-lg transition duration-300 transform hover:scale-105 bg-gradient-to-r from-indigo-500 to-purple-600 hover:from-indigo-600 hover:to-purple-700"
              :disabled="isDownloading">
              <span v-if="isDownloading" class="mr-2">
                <span class="animate-spin rounded-full h-5 w-5 border-b-2 border-white inline-block"></span>
              </span>
              <DownloadIcon v-if="!isDownloading" class="w-5 h-5 mr-2" />
              {{ isDownloading ? '下载中...' : '下载文件' }}
            </button>
            
            <button @click="resetView" 
              class="w-full py-2 px-4 rounded-lg font-medium transition duration-300 border"
              :class="[
                isDarkMode 
                  ? 'border-gray-700 text-gray-300 hover:bg-gray-700' 
                  : 'border-gray-300 text-gray-700 hover:bg-gray-100'
              ]">
              返回
            </button>
          </div>
          
          <div v-if="!selectedFile" class="mt-6 text-center">
            <router-link to="/login" class="text-indigo-400 hover:text-indigo-300 transition duration-300">
              返回登录
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, inject, computed } from 'vue'
import { 
  FileIcon, 
  DownloadIcon,
  CalendarIcon,
  HardDriveIcon
} from 'lucide-vue-next'
import { useRouter, useRoute } from 'vue-router'
import { useAlertStore } from '@/stores/alertStore'
import axios from 'axios'

const router = useRouter()
const route = useRoute()
const isDarkMode = inject('isDarkMode')
const alertStore = useAlertStore()

// 状态管理
const shareCode = ref('')
const inputStatus = ref({ loading: false })
const isInputFocused = ref(false)
const error = ref(false)
const selectedFile = ref(null)
const isDownloading = ref(false)

// 初始化时检查URL参数
const initFromQuery = () => {
  const queryShareCode = route.query.code
  if (queryShareCode) {
    shareCode.value = queryShareCode
    handleSubmit()
  }
}

// 格式化日期
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 格式化文件大小
const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

// 验证分享码
const handleSubmit = async () => {
  if (!shareCode.value) {
    alertStore.showAlert('请输入分享码', 'error')
    return
  }

  inputStatus.value.loading = true
  error.value = false

  try {
    console.log('Retrieving file info for share code:', shareCode.value)
    // 使用正确的API URL获取分享文件的元数据信息
    const response = await axios.get(`https://localhost:5001/api/File/shared/${shareCode.value}/info`, {
      validateStatus: status => status < 500,
      withCredentials: true
    })

    console.log('File info response:', response)

    if (response.status === 200 && response.data.success) {
      selectedFile.value = response.data.file
      alertStore.showAlert('文件找到，可以开始下载', 'success')
    } else {
      error.value = true
      alertStore.showAlert(response.data?.message || '无效的分享码', 'error')
    }
  } catch (err) {
    console.error('获取文件信息失败:', err)
    error.value = true
    if (err.message && err.message.includes('Network Error')) {
      alertStore.showAlert('网络错误，请检查后端API是否正在运行', 'error')
    } else {
      alertStore.showAlert('获取文件信息失败，请检查分享码是否正确', 'error')
    }
  } finally {
    inputStatus.value.loading = false
  }
}

// 下载文件
const downloadFile = async () => {
  if (!selectedFile.value) return
  
  isDownloading.value = true
  
  try {
    console.log('Downloading file with share code:', shareCode.value)
    // 使用正确的API URL下载文件
    const response = await axios({
      url: `https://localhost:5001/api/File/shared/${shareCode.value}`,
      method: 'GET',
      responseType: 'blob',
      withCredentials: true
    })
    
    console.log('File download successful')
    
    // 创建Blob链接并触发下载
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', selectedFile.value.fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
    
    alertStore.showAlert('文件下载成功', 'success')
  } catch (err) {
    console.error('文件下载失败:', err)
    if (err.message && err.message.includes('Network Error')) {
      alertStore.showAlert('网络错误，请检查后端API是否正在运行', 'error')
    } else if (err.response && err.response.status === 401) {
      alertStore.showAlert('下载需要登录', 'error')
    } else {
      alertStore.showAlert('文件下载失败，请稍后重试', 'error')
    }
  } finally {
    isDownloading.value = false
  }
}

// 重置视图
const resetView = () => {
  selectedFile.value = null
  shareCode.value = ''
  error.value = false
}

// 在组件挂载时执行初始化
initFromQuery()
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

.w-97-100 {
  width: 97%;
}

/* 自定义滚动条样式 */
.custom-scrollbar {
  scrollbar-width: thin;
  scrollbar-color: rgba(156, 163, 175, 0.3) transparent;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(156, 163, 175, 0.3);
  border-radius: 3px;
  transition: background-color 0.3s;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background-color: rgba(156, 163, 175, 0.5);
}
</style>

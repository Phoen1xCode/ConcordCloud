<template>
  <div class="min-h-screen flex items-center justify-center p-4 overflow-hidden transition-colors duration-300"
    :class="[isDarkMode ? 'bg-gray-900' : 'bg-gray-50']">
    <!-- Add user profile button -->
    <button @click="openUserProfile" 
      class="fixed bottom-4 left-4 z-30 flex items-center gap-2 px-4 py-2 rounded-full shadow-lg transition-all duration-300 hover:scale-105"
      :class="[isDarkMode ? 'bg-gray-800 text-indigo-400 hover:bg-gray-700' : 'bg-white text-indigo-600 hover:bg-gray-100']"
      v-if="isLoggedIn">
      <span>个人资料</span>
      <UserIcon class="w-5 h-5" />
    </button>
    
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
            <button @click="downloadFileByShareCode"
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
            <router-link v-if="isLoggedIn" to="/send" class="text-indigo-400 hover:text-indigo-300 transition duration-300 flex items-center justify-center">
              去上传文件
              <UploadIcon class="w-4 h-4 ml-1" />
            </router-link>
            <div v-if="!isLoggedIn" class="mt-4 flex flex-col gap-3">
              <button
                @click="router.push('/login')"
                class="w-full bg-gradient-to-r from-indigo-500 to-pink-500 text-white font-bold py-3 px-4 rounded-lg hover:from-indigo-600 hover:to-pink-600 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-opacity-50 transition duration-300 transform hover:scale-105"
              >
                登录账户
              </button>
              <p class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                登录后可以上传和管理您的文件
              </p>
            </div>
          </div>
        </div>
        
        <div class="px-8 py-4 bg-opacity-50 flex justify-between items-center"
          :class="[isDarkMode ? 'bg-gray-800' : 'bg-gray-100']">
          <span class="text-sm flex items-center" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-800']">
            <ShieldCheckIcon class="w-4 h-4 mr-1 text-green-400" />
            安全加密
          </span>
          <button @click="toggleDrawer" class="text-sm hover:text-indigo-300 transition duration-300 flex items-center"
            :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-600']">
            文件管理
            <ClipboardListIcon class="w-4 h-4 ml-1" />
          </button>
        </div>
      </div>
    </div>
    
    <!-- 抽屉式文件管理 -->
    <transition name="drawer">
      <div v-if="showDrawer"
        class="fixed inset-y-0 right-0 w-full sm:w-120 bg-opacity-70 backdrop-filter backdrop-blur-xl shadow-2xl z-50 overflow-hidden flex flex-col"
        :class="[isDarkMode ? 'bg-gray-900' : 'bg-white']">
        <div class="flex justify-between items-center p-6 border-b"
          :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
          <h3 class="text-2xl font-bold" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
            文件管理
          </h3>
          <button @click="toggleDrawer" class="hover:text-white transition duration-300"
            :class="[isDarkMode ? 'text-gray-400' : 'text-gray-800']">
            <XIcon class="w-6 h-6" />
          </button>
        </div>
        <div class="px-6 pb-4">
          <div class="relative">
            <input 
              type="text" 
              v-model="searchQuery" 
              placeholder="搜索文件..." 
              class="w-full px-4 py-2 rounded-lg border transition-all duration-200 focus:outline-none focus:ring-2"
              :class="[
                isDarkMode 
                  ? 'bg-gray-800 border-gray-700 text-white placeholder-gray-400 focus:ring-indigo-500' 
                  : 'bg-white border-gray-300 text-gray-900 placeholder-gray-500 focus:ring-indigo-500'
              ]"
            />
            <SearchIcon class="w-5 h-5 absolute right-3 top-1/2 transform -translate-y-1/2" 
              :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']" />
          </div>
        </div>
        <div class="flex-grow overflow-y-auto p-6">
          <div v-if="isLoadingFiles" class="flex justify-center items-center h-full">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2" 
              :class="[isDarkMode ? 'border-indigo-400' : 'border-indigo-600']"></div>
          </div>
          <div v-else-if="!isLoggedIn" class="flex flex-col items-center justify-center h-full">
            <LockIcon class="w-16 h-16 mb-4" :class="[isDarkMode ? 'text-gray-500' : 'text-gray-400']" />
            <p class="text-center" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              请先登录以查看您的文件
            </p>
            <router-link to="/login" 
              class="mt-4 px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition duration-300">
              去登录
            </router-link>
          </div>
          <div v-else-if="userFiles.length === 0" class="flex flex-col items-center justify-center h-full">
            <FileIcon class="w-16 h-16 mb-4" :class="[isDarkMode ? 'text-gray-500' : 'text-gray-400']" />
            <p class="text-center" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              你还没有上传任何文件
            </p>
          </div>
          <transition-group name="list" tag="div" class="space-y-4">
            <div v-for="file in paginatedFiles" :key="file.id"
              class="bg-opacity-50 rounded-lg p-4 flex items-center shadow-md hover:shadow-lg transition duration-300 transform hover:scale-102"
              :class="[isDarkMode ? 'bg-gray-800 hover:bg-gray-700' : 'bg-gray-100 hover:bg-white']">
              <div class="flex-shrink-0 mr-4">
                <FileIcon class="w-10 h-10" :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-600']" />
              </div>
              <div class="flex-grow min-w-0 mr-4">
                <div class="flex items-center">
                  <p class="font-medium text-lg truncate" 
                    :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
                    {{ file.fileName }}
                  </p>
                </div>
                <p class="text-sm truncate" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                  {{ formatFileSize(file.fileSize) }} · {{ formatDate(file.uploadedAt) }}
                </p>
              </div>
              <div class="flex-shrink-0">
                <button @click="downloadFileById(file.id, file.fileName)"
                  class="p-2 rounded-full hover:bg-opacity-20 transition duration-300" :class="[
                    isDarkMode
                      ? 'hover:bg-blue-400 text-blue-400'
                      : 'hover:bg-blue-100 text-blue-600'
                  ]" :title="'下载'">
                  <DownloadIcon class="w-5 h-5" />
                </button>
              </div>
            </div>
          </transition-group>
          <div class="mt-4 text-center">
            <button @click="changePage(filesPagination.currentPage - 1)"
              :disabled="filesPagination.currentPage === 1"
              class="px-4 py-2 rounded-lg font-medium transition duration-300 border"
              :class="[
                isDarkMode 
                  ? 'border-gray-700 text-gray-300 hover:bg-gray-700' 
                  : 'border-gray-300 text-gray-700 hover:bg-gray-100'
              ]">
              上一页
            </button>
            <span class="px-4 py-2">
              {{ filesPagination.currentPage }} / {{ totalFilePages }}
            </span>
            <button @click="changePage(filesPagination.currentPage + 1)"
              :disabled="filesPagination.currentPage === totalFilePages"
              class="px-4 py-2 rounded-lg font-medium transition duration-300 border"
              :class="[
                isDarkMode 
                  ? 'border-gray-700 text-gray-300 hover:bg-gray-700' 
                  : 'border-gray-300 text-gray-700 hover:bg-gray-100'
              ]">
              下一页
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- 右下角退出登录按钮 -->
    <button @click="logout" 
      class="fixed bottom-4 right-4 z-30 flex items-center gap-2 px-4 py-2 rounded-full shadow-lg transition-all duration-300 hover:scale-105"
      :class="[isDarkMode ? 'bg-gray-800 text-red-400 hover:bg-gray-700' : 'bg-white text-red-600 hover:bg-gray-100']"
      v-if="isLoggedIn">
      <span>退出登录</span>
      <LogOutIcon class="w-5 h-5" />
    </button>

    <!-- User Profile Modal -->
    <transition name="fade">
      <div v-if="showUserProfileModal" class="fixed inset-0 flex items-center justify-center z-50">
        <!-- Background overlay -->
        <div class="absolute inset-0 bg-black/60 backdrop-filter backdrop-blur-sm" @click="closeUserProfileModal"></div>
        
        <!-- Modal content -->
        <div class="relative p-6 rounded-lg shadow-xl w-full max-w-md"
          :class="[isDarkMode ? 'bg-gray-800' : 'bg-white']">
          <button @click="closeUserProfileModal" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
            <XIcon class="w-5 h-5" />
          </button>
          
          <h3 class="text-xl font-bold mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">个人资料</h3>
          
          <div v-if="userProfile">
            <div class="mb-4">
              <p class="text-sm font-medium" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">用户名</p>
              <p class="mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">{{ userProfile.username }}</p>
            </div>
            
            <div class="mb-4">
              <p class="text-sm font-medium" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">邮箱</p>
              <p class="mt-1" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">{{ userProfile.email }}</p>
            </div>
            
            <div class="border-t mt-6" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
              <h4 class="text-lg font-medium mt-4 mb-3" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">修改用户名</h4>
              
              <div class="mb-4">
                <label class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
                  新用户名
                </label>
                <input type="text" v-model="usernameForm.newUsername"
                  class="w-full px-3 py-2 border rounded-md"
                  :class="[isDarkMode ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-800']">
              </div>
              
              <button @click="updateUsername" class="w-full px-4 py-2 rounded-md font-medium transition-colors duration-300 mb-6"
                :class="[isDarkMode ? 'bg-blue-600 hover:bg-blue-700 text-white' : 'bg-blue-500 hover:bg-blue-600 text-white']"
                :disabled="isUpdatingUsername">
                <span v-if="isUpdatingUsername">处理中...</span>
                <span v-else>更新用户名</span>
              </button>
            </div>
            
            <div class="border-t" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
              <h4 class="text-lg font-medium mt-4 mb-3" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">修改密码</h4>
              
              <div class="mb-4">
                <label class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
                  当前密码
                </label>
                <div class="relative">
                  <input :type="showCurrentPassword ? 'text' : 'password'" v-model="passwordForm.currentPassword"
                    class="w-full px-3 py-2 border rounded-md" autocomplete="new-password"
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
                :disabled="isUpdatingPassword">
                <span v-if="isUpdatingPassword">处理中...</span>
                <span v-else>更新密码</span>
              </button>
            </div>
          </div>
          
          <div v-else class="flex justify-center items-center h-40">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2" :class="[isDarkMode ? 'border-blue-400' : 'border-blue-600']"></div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, inject, computed, onMounted, watch } from 'vue'
import { 
  FileIcon, 
  DownloadIcon,
  CalendarIcon,
  HardDriveIcon,
  ClipboardListIcon,
  XIcon,
  ShieldCheckIcon,
  LockIcon,
  LogOutIcon,
  UploadIcon,
  SearchIcon,
  UserIcon,
  EyeIcon,
  EyeOffIcon
} from 'lucide-vue-next'
import { useRouter, useRoute } from 'vue-router'
import { useAlertStore } from '@/stores/alertStore'
import axios from 'axios'
import api from '@/utils/api'

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

// 文件管理相关
const showDrawer = ref(false)
const userFiles = ref([])
const isLoadingFiles = ref(false)
const isLoggedIn = computed(() => localStorage.getItem('isLoggedIn') === 'true')

// 添加分页相关状态
const filesPagination = ref({
  currentPage: 1,
  pageSize: 6
})

// 添加搜索相关状态
const searchQuery = ref('')

// 添加过滤后的文件列表计算属性
const filteredFiles = computed(() => {
  if (!searchQuery.value) return userFiles.value
  
  const query = searchQuery.value.toLowerCase()
  return userFiles.value.filter(file => 
    file.fileName.toLowerCase().includes(query)
  )
})

// 计算分页后的文件列表
const paginatedFiles = computed(() => {
  const start = (filesPagination.value.currentPage - 1) * filesPagination.value.pageSize
  const end = start + filesPagination.value.pageSize
  return filteredFiles.value.slice(start, end)
})

// 计算总页数
const totalFilePages = computed(() => {
  return Math.ceil(filteredFiles.value.length / filesPagination.value.pageSize)
})

// 翻页函数
const changePage = (page) => {
  if (page < 1 || page > totalFilePages.value) return
  filesPagination.value.currentPage = page
}

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
    // 先获取文件元信息
    const response = await api.get(`/api/file/shared/${shareCode.value}/info`)

    if (response.success) {
      selectedFile.value = response.data.file
      alertStore.showAlert('文件找到，可以开始下载', 'success')
    } else {
      error.value = true
      alertStore.showAlert(response.message || '无效的分享码', 'error')
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

// 修复分享码下载文件函数
const downloadFileByShareCode = async () => {
  if (!shareCode.value) {
    alertStore.showAlert('请输入分享码', 'error')
    return
  }

  isDownloading.value = true

  try {
    // 使用原生axios下载，避免API拦截器处理二进制数据
    const baseURL = api.defaults.baseURL || 'https://localhost:5001'
    const response = await axios({
      url: `${baseURL}/api/file/shared/${shareCode.value}`,
      method: 'GET',
      responseType: 'blob',
      withCredentials: true
    })

    // 尝试从响应头获取文件名
    let fileName = shareCode.value
    const disposition = response.headers['content-disposition']
    if (disposition) {
      const match = disposition.match(/filename="?([^";]+)"?/)
      if (match && match[1]) {
        fileName = decodeURIComponent(match[1])
      }
    }

    // 创建Blob链接并触发下载
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)  // 释放Blob URL

    alertStore.showAlert('文件下载成功', 'success')
  } catch (err) {
    let errorMessage = '文件下载失败，请稍后重试'
    if (err.response) {
      if (err.response.status === 404) {
        errorMessage = '文件不存在或已过期'
      } else if (err.response.data?.message) {
        errorMessage = err.response.data.message
      }
    } else if (err.message && err.message.includes('Network Error')) {
      errorMessage = '网络错误，请检查后端API是否正在运行'
    }
    alertStore.showAlert(errorMessage, 'error')
  } finally {
    isDownloading.value = false
  }
}

// 修复下载文件函数
const downloadFile = async () => {
  if (!selectedFile.value) return
  
  isDownloading.value = true
  
  try {
    // 使用原生axios下载，避免API拦截器处理二进制数据
    const baseURL = api.defaults.baseURL || 'https://localhost:5001'
    const response = await axios({
      url: `${baseURL}/api/file/shared/${shareCode.value}`,
      method: 'GET',
      responseType: 'blob',
      withCredentials: true
    });
    
    // 创建Blob链接并触发下载
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', selectedFile.value.fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)  // 释放Blob URL
    
    alertStore.showAlert('文件下载成功', 'success')
  } catch (err) {
    let errorMessage = '文件下载失败，请稍后重试';
    
    if (err.response) {
      if (err.response.status === 401) {
        errorMessage = '下载需要登录';
      } else if (err.response.status === 404) {
        errorMessage = '文件不存在或已过期';
      } else if (err.response.data?.message) {
        errorMessage = err.response.data.message;
      }
    } else if (err.message && err.message.includes('Network Error')) {
      errorMessage = '网络错误，请检查后端API是否正在运行';
    }
    
    alertStore.showAlert(errorMessage, 'error');
  } finally {
    isDownloading.value = false;
  }
}

// 修复通过ID下载文件函数
const downloadFileById = async (fileId, fileName) => {
  if (!isLoggedIn.value) {
    alertStore.showAlert('请先登录', 'error')
    return
  }
  
  try {
    alertStore.showAlert('开始下载文件...', 'info')
    
    // 使用原生axios下载，避免API拦截器处理二进制数据
    const baseURL = api.defaults.baseURL || 'http://localhost:5000'
    const response = await axios({
      url: `${baseURL}/api/file/download/${fileId}`,
      method: 'GET',
      responseType: 'blob',
      withCredentials: true
    });
    
    // 创建Blob链接并触发下载
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)  // 释放Blob URL
    
    alertStore.showAlert('文件下载成功', 'success')
  } catch (err) {
    let errorMessage = '文件下载失败，请稍后重试';
    
    if (err.response) {
      if (err.response.status === 401) {
        errorMessage = '下载需要登录，请先登录';
        router.push('/login');
      } else if (err.response.status === 404) {
        errorMessage = '文件不存在';
      } else if (err.response.data?.message) {
        errorMessage = err.response.data.message;
      }
    } else if (err.message && err.message.includes('Network Error')) {
      errorMessage = '网络错误，请检查后端API是否正在运行';
    }
    
    alertStore.showAlert(errorMessage, 'error');
  }
}

// 重置视图
const resetView = () => {
  selectedFile.value = null
  shareCode.value = ''
  error.value = false
}

// 切换文件管理抽屉
const toggleDrawer = () => {
  showDrawer.value = !showDrawer.value
  if (showDrawer.value && isLoggedIn.value) {
    fetchUserFiles()
  }
}

// 获取用户文件列表
const fetchUserFiles = async () => {
  try {
    isLoadingFiles.value = true
    const response = await api.get('/api/file')
    
    console.log('获取文件列表响应:', response)
    if (response.success) {
      if (Array.isArray(response.data)) {
        userFiles.value = response.data
      } else if (response.data && Array.isArray(response.data.$values)) {
        userFiles.value = response.data.$values
      } else if (response.files) {
        userFiles.value = response.files
      } else {
        userFiles.value = []
        console.warn('文件列表数据格式不正确:', response)
      }
    } else {
      userFiles.value = []
      alertStore.showAlert('获取文件列表失败', 'error')
    }
  } catch (error) {
    console.error('获取文件列表出错:', error)
    userFiles.value = []
    if (error.response) {
      if (error.response.status === 401) {
        alertStore.showAlert('请先登录', 'error')
        router.push('/login')
      } else if (error.response.data?.message) {
        alertStore.showAlert(error.response.data.message, 'error')
      } else {
        alertStore.showAlert('获取文件列表失败', 'error')
      }
    } else {
      alertStore.showAlert('获取文件列表失败，请检查网络连接', 'error')
    }
  } finally {
    isLoadingFiles.value = false
  }
}

// 用户登出
const logout = async () => {
  console.log('Logout function triggered')
  alertStore.showAlert('正在退出登录...', 'info')
  
  try {
    // 调用登出API
    console.log('Calling logout API')
    await api.post('/api/user/logout')
    
    // 清除本地登录状态标志
    localStorage.removeItem('token')
    localStorage.removeItem('isLoggedIn')
    localStorage.removeItem('isUserLoggedIn')
    alertStore.showAlert('已成功退出登录', 'success')
    router.push('/login')
  } catch (error) {
    console.error('Logout error:', error)
    // 即使API调用失败，也清除本地登录状态
    localStorage.removeItem('token')
    localStorage.removeItem('isLoggedIn')
    localStorage.removeItem('isUserLoggedIn')
    alertStore.showAlert('登出过程中发生错误，但已清除本地登录状态', 'warning')
    router.push('/login')
  }
}

// 在组件挂载时执行初始化
onMounted(() => {
  initFromQuery()
})

// 添加搜索相关状态的监听器
watch(searchQuery, () => {
  filesPagination.value.currentPage = 1
})

// User profile related states
const showUserProfileModal = ref(false)
const userProfile = ref(null)
const isUpdatingUsername = ref(false)
const isUpdatingPassword = ref(false)

// Password visibility states
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

// Form data
const passwordForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const usernameForm = ref({
  newUsername: ''
})

// Open user profile modal
const openUserProfile = async () => {
  showUserProfileModal.value = true
  
  // Reset password form fields when opening the modal
  passwordForm.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  }
  
  await getUserProfile()
}

// Close user profile modal
const closeUserProfileModal = () => {
  showUserProfileModal.value = false
  passwordForm.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  }
  usernameForm.value = {
    newUsername: ''
  }
}

// Get user profile information
const getUserProfile = async () => {
  try {
    const response = await api.get('/api/user/profile')
    if (response.success) {
      userProfile.value = response.data
      
      // Reset username form to empty when opening profile
      usernameForm.value.newUsername = ''
    } else {
      alertStore.showAlert('获取用户资料失败', 'error')
    }
  } catch (error) {
    console.error('Error fetching user profile:', error)
    alertStore.showAlert('获取用户资料失败', 'error')
  }
}

// Update username
const updateUsername = async () => {
  if (!usernameForm.value.newUsername) {
    alertStore.showAlert('请输入新用户名', 'error')
    return
  }

  if (usernameForm.value.newUsername === userProfile.value.username) {
    alertStore.showAlert('新用户名不能与当前用户名相同', 'warning')
    return
  }

  isUpdatingUsername.value = true

  try {
    const response = await api.post('/api/user/update-username', {
      newUsername: usernameForm.value.newUsername
    })

    if (response.success) {
      alertStore.showAlert('用户名更新成功', 'success')
      // Update the profile data
      userProfile.value.username = usernameForm.value.newUsername
    } else {
      alertStore.showAlert(response.message || '用户名更新失败', 'error')
    }
  } catch (error) {
    console.error('Error updating username:', error)
    
    let errorMessage = '用户名更新失败，请稍后再试'
    if (error.response?.data?.message) {
      errorMessage = error.response.data.message
    }
    alertStore.showAlert(errorMessage, 'error')
  } finally {
    isUpdatingUsername.value = false
  }
}

// Change password
const changePassword = async () => {
  // Validate password
  if (!passwordForm.value.currentPassword || !passwordForm.value.newPassword || !passwordForm.value.confirmPassword) {
    alertStore.showAlert('请填写所有密码字段', 'error')
    return
  }
  
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    alertStore.showAlert('两次输入的新密码不一致', 'error')
    return
  }
  
  if (passwordForm.value.newPassword.length < 6) {
    alertStore.showAlert('新密码长度至少为6位', 'error')
    return
  }

  isUpdatingPassword.value = true
  
  try {
    const response = await api.post('/api/user/change-password', {
      currentPassword: passwordForm.value.currentPassword,
      newPassword: passwordForm.value.newPassword,
      confirmPassword: passwordForm.value.confirmPassword
    })
    
    if (response.success) {
      alertStore.showAlert('密码修改成功', 'success')
      // Reset the form
      passwordForm.value = {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      }
    } else {
      alertStore.showAlert(response.message || '密码修改失败', 'error')
    }
  } catch (error) {
    console.error('Error changing password:', error)
    
    let errorMessage = '密码修改失败，请稍后再试'
    if (error.response?.data?.message) {
      errorMessage = error.response.data.message
    }
    alertStore.showAlert(errorMessage, 'error')
  } finally {
    isUpdatingPassword.value = false
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

/* Drawer animation */
.drawer-enter-active,
.drawer-leave-active {
  transition: transform 0.3s ease;
}

.drawer-enter-from,
.drawer-leave-to {
  transform: translateX(100%);
}

.list-enter-active,
.list-leave-active {
  transition: all 0.5s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

@media (min-width: 640px) {
  .sm\:w-120 {
    width: 30rem;
    /* 480px */
  }
}
</style>


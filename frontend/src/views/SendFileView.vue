<template>
  <div class="min-h-screen flex items-center justify-center p-4 overflow-hidden transition-colors duration-300"
    @paste.prevent="handlePaste">
    <div class="rounded-3xl shadow-2xl overflow-hidden border w-full max-w-md transition-colors duration-300" :class="[
      isDarkMode
        ? 'bg-white bg-opacity-10 backdrop-filter backdrop-blur-xl border-gray-700'
        : 'bg-white border-gray-200'
    ]">
      <div class="p-8">
        <h2 class="text-3xl font-extrabold text-center mb-8 cursor-pointer transition-colors duration-300" :class="[
          isDarkMode
            ? 'text-transparent bg-clip-text bg-gradient-to-r from-indigo-300 via-purple-300 to-pink-300'
            : 'text-indigo-600'
        ]" @click="toRetrieve">
          {{ config.name }}
        </h2>
        <form @submit.prevent="handleSubmit" class="space-y-8">
          <!-- 文件上传区域 -->
          <div class="grid grid-cols-1 gap-8">
            <div
              class="rounded-xl p-8 flex flex-col items-center justify-center border-2 border-dashed transition-all duration-300 group cursor-pointer relative"
              :class="[
                isDarkMode
                  ? 'bg-gray-800 bg-opacity-50 border-gray-600 hover:border-indigo-500'
                  : 'bg-gray-100 border-gray-300 hover:border-indigo-500'
              ]" @click="triggerFileUpload" @dragover.prevent @drop.prevent="handleFileDrop">
              <input id="file-upload" type="file" class="hidden" @change="handleFileUpload" ref="fileInput" />
              <div class="absolute inset-0 w-full h-full" v-if="uploadProgress > 0">
                <BorderProgressBar :progress="uploadProgress" />
              </div>
              <UploadCloudIcon :class="[
                'w-16 h-16 transition-colors duration-300',
                isDarkMode
                  ? 'text-gray-400 group-hover:text-indigo-400'
                  : 'text-gray-600 group-hover:text-indigo-600'
              ]" />
              <p :class="[
                'mt-4 text-sm transition-colors duration-300 w-full text-center',
                isDarkMode
                  ? 'text-gray-400 group-hover:text-indigo-400'
                  : 'text-gray-600 group-hover:text-indigo-600'
              ]">
                <span class="block truncate">
                  {{ selectedUploadFile ? selectedUploadFile.name : '点击或拖放文件到此处上传' }}
                </span>
              </p>
              <p :class="['mt-2 text-xs', isDarkMode ? 'text-gray-500' : 'text-gray-400']">
                支持各种常见格式，最大{{ getStorageUnit(config.uploadSize) }}
              </p>
            </div>
          </div>
          
          <!-- 提交按钮 -->
          <button type="submit"
            class="w-full bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 text-white font-bold py-4 px-6 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-opacity-50 transition-all duration-300 transform hover:scale-105 hover:shadow-lg relative overflow-hidden group">
            <span
              class="absolute top-0 left-0 w-full h-full bg-white opacity-0 group-hover:opacity-20 transition-opacity duration-300"></span>
            <span class="relative z-10 flex items-center justify-center text-lg">
              <SendIcon class="w-6 h-6 mr-2" />
              <span>安全寄送</span>
            </span>
          </button>
        </form>
        <div class="mt-6 text-center">
          <router-link to="/retrieve" class="text-indigo-400 hover:text-indigo-300 transition duration-300">
            需要取件？点击这里
          </router-link>
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
        <div class="flex-grow overflow-y-auto p-6">
          <div v-if="isLoadingFiles" class="flex justify-center items-center h-full">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2" 
              :class="[isDarkMode ? 'border-indigo-400' : 'border-indigo-600']"></div>
          </div>
          <div v-else-if="userFiles.length === 0" class="flex flex-col items-center justify-center h-full">
            <FileIcon class="w-16 h-16 mb-4" :class="[isDarkMode ? 'text-gray-500' : 'text-gray-400']" />
            <p class="text-center" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
              你还没有上传任何文件
            </p>
          </div>
          <transition-group name="list" tag="div" class="space-y-4">
            <div v-for="file in userFiles" :key="file.id"
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
              <div class="flex-shrink-0 flex space-x-2">
                <button @click="shareFile(file)"
                  class="p-2 rounded-full hover:bg-opacity-20 transition duration-300" :class="[
                    isDarkMode
                      ? 'hover:bg-blue-400 text-blue-400'
                      : 'hover:bg-blue-100 text-blue-600'
                  ]" :title="'分享'">
                  <Share2Icon class="w-5 h-5" />
                </button>
                <button @click="startRenaming(file)"
                  class="p-2 rounded-full hover:bg-opacity-20 transition duration-300" :class="[
                    isDarkMode
                      ? 'hover:bg-green-400 text-green-400'
                      : 'hover:bg-green-100 text-green-600'
                  ]" :title="'重命名'">
                  <PencilIcon class="w-5 h-5" />
                </button>
                <button @click="confirmDeleteFile(file)"
                  class="p-2 rounded-full hover:bg-opacity-20 transition duration-300" :class="[
                    isDarkMode ? 'hover:bg-red-400 text-red-400' : 'hover:bg-red-100 text-red-600'
                  ]" :title="'删除'">
                  <TrashIcon class="w-5 h-5" />
                </button>
              </div>
            </div>
          </transition-group>
        </div>
      </div>
    </transition>

    <!-- 文件分享弹窗 -->
    <transition name="fade">
      <div v-if="showShareDialog"
        class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-3 sm:p-4 overflow-y-auto">
        <div
          class="w-full max-w-md rounded-2xl shadow-2xl transform transition-all duration-300 ease-out overflow-hidden"
          :class="[isDarkMode ? 'bg-gray-900 bg-opacity-70' : 'bg-white bg-opacity-95']">
          <!-- 顶部标题栏 -->
          <div class="px-4 sm:px-6 py-3 sm:py-4 border-b" :class="[isDarkMode ? 'border-gray-800' : 'border-gray-100']">
            <div class="flex items-center justify-between">
              <h3 class="text-lg sm:text-xl font-semibold" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
                创建文件分享
              </h3>
              <button @click="closeShareDialog"
                class="p-1.5 sm:p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
                <XIcon class="w-4 h-4 sm:w-5 sm:h-5" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']" />
              </button>
            </div>
          </div>

          <!-- 主要内容区域 -->
          <div class="p-4 sm:p-6 space-y-4">
            <!-- 文件信息 -->
            <div class="rounded-xl p-3 sm:p-4"
              :class="[isDarkMode ? 'bg-gray-800 bg-opacity-50' : 'bg-gray-50 bg-opacity-95']">
              <div class="flex items-center">
                <div class="p-2 sm:p-3 rounded-lg" :class="[isDarkMode ? 'bg-gray-700' : 'bg-white']">
                  <FileIcon class="w-5 h-5 sm:w-6 sm:h-6"
                    :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-600']" />
                </div>
                <div class="ml-3 sm:ml-4 min-w-0 flex-1">
                  <h4 class="font-medium text-sm sm:text-base truncate"
                    :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
                    {{ selectedFile ? selectedFile.fileName : '' }}
                  </h4>
                  <p class="text-xs sm:text-sm truncate" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                    {{ selectedFile ? formatFileSize(selectedFile.fileSize) : '' }} · 
                    {{ selectedFile ? formatDate(selectedFile.uploadedAt) : '' }}
                  </p>
                </div>
              </div>
            </div>

            <!-- 过期设置 - 改为日期选择器 -->
            <div class="flex flex-col space-y-3">
              <label :class="['text-sm font-medium', isDarkMode ? 'text-gray-300' : 'text-gray-700']">
                过期时间
              </label>
              <div class="relative flex-grow group">
                <div :class="[
                  'relative h-11 rounded-xl border transition-all duration-300',
                  isDarkMode
                    ? 'bg-gray-800/50 border-gray-700/50 group-hover:border-gray-600'
                    : 'bg-white border-gray-200 group-hover:border-gray-300'
                ]">
                  <input 
                    type="date" 
                    v-model="expirationDate" 
                    :min="minExpirationDate"
                    :max="maxExpirationDate"
                    :class="[
                      'w-full h-full px-4 rounded-xl transition-all duration-300',
                      'focus:outline-none focus:ring-2 focus:ring-offset-0',
                      'bg-transparent',
                      isDarkMode
                        ? 'text-gray-100 focus:ring-indigo-500/70 placeholder-gray-500'
                        : 'text-gray-900 focus:ring-indigo-500/50 placeholder-gray-400'
                    ]" />
                </div>
              </div>
            </div>

            <!-- 生成分享按钮 -->
            <button @click="createFileShare" 
              class="w-full mt-4 bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 text-white font-bold py-3 px-6 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-opacity-50 transition-all duration-300 transform hover:scale-105 hover:shadow-lg relative overflow-hidden group"
              :disabled="isCreatingShare">
              <span class="absolute top-0 left-0 w-full h-full bg-white opacity-0 group-hover:opacity-20 transition-opacity duration-300"></span>
              <span class="relative z-10 flex items-center justify-center text-lg">
                <span v-if="isCreatingShare">生成中...</span>
                <span v-else>生成分享链接</span>
              </span>
            </button>
          </div>

          <!-- 分享结果区域 -->
          <div v-if="shareResult" class="px-4 sm:px-6 py-4 sm:py-5 bg-opacity-50 space-y-4"
              :class="[isDarkMode ? 'bg-gray-800' : 'bg-gray-50']">
            <div class="bg-gradient-to-br from-indigo-500 to-purple-600 rounded-xl p-4 sm:p-5 text-white">
              <div class="flex items-center justify-between mb-3 sm:mb-4">
                <h4 class="font-medium text-sm sm:text-base">取件码</h4>
                <button @click="copyRetrieveCode(shareResult.shareCode)"
                  class="p-1.5 sm:p-2 rounded-full hover:bg-white/10 transition-colors">
                  <ClipboardCopyIcon class="w-4 h-4 sm:w-5 sm:h-5" />
                </button>
              </div>
              <p class="text-2xl sm:text-3xl font-bold tracking-wider text-center break-all">
                {{ shareResult.shareCode }}
              </p>
            </div>
            
            <div class="flex items-center space-x-2 text-sm" 
              :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">
              <ClockIcon class="w-4 h-4" />
              <span>有效期至: {{ formatDate(shareResult.expiresAt) }}</span>
            </div>
            
            <button @click="copyRetrieveLink(shareResult.shareCode)"
              class="w-full bg-indigo-600 hover:bg-indigo-700 text-white py-2 px-4 rounded-lg text-sm font-medium transition-colors flex items-center justify-center">
              <span>复制取件链接</span>
              <ClipboardCopyIcon class="w-4 h-4 ml-2" />
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- 删除确认弹窗 -->
    <transition name="fade">
      <div v-if="showDeleteConfirm"
        class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-3 sm:p-4">
        <div class="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-sm w-full">
          <h3 class="text-lg font-medium mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
            确认删除
          </h3>
          <p class="mb-6" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">
            确定要删除文件 "{{ fileToDelete?.fileName }}" 吗？此操作无法撤销。
          </p>
          <div class="flex justify-end space-x-3">
            <button @click="showDeleteConfirm = false"
              class="px-4 py-2 rounded-md border transition-colors"
              :class="[isDarkMode ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 
                'border-gray-300 text-gray-700 hover:bg-gray-100']">
              取消
            </button>
            <button @click="deleteFile"
              class="px-4 py-2 rounded-md bg-red-500 text-white hover:bg-red-600 transition-colors">
              删除
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- 文件重命名弹窗 -->
    <transition name="fade">
      <div v-if="showRenameDialog"
        class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-3 sm:p-4">
        <div class="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-sm w-full">
          <h3 class="text-lg font-medium mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
            重命名文件
          </h3>
          
          <div class="mb-4">
            <div class="flex items-center mb-3">
              <FileIcon class="w-6 h-6 mr-2" :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-600']" />
              <span :class="[isDarkMode ? 'text-gray-300' : 'text-gray-600']">
                {{ fileToRename?.fileName }}
              </span>
            </div>
            
            <label class="block text-sm mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
              新文件名
            </label>
            <input 
              type="text" 
              v-model="newFileName" 
              class="w-full px-3 py-2 rounded-lg border focus:outline-none focus:ring-2"
              :class="[isDarkMode ? 'bg-gray-700 border-gray-600 text-white focus:ring-indigo-500' : 
                'bg-white border-gray-300 text-gray-800 focus:ring-indigo-500']"
              placeholder="输入新文件名"
            />
          </div>
          
          <div class="flex justify-end space-x-3">
            <button @click="closeRenameDialog"
              class="px-4 py-2 rounded-md border transition-colors"
              :class="[isDarkMode ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 
                'border-gray-300 text-gray-700 hover:bg-gray-100']">
              取消
            </button>
            <button @click="confirmRename"
              class="px-4 py-2 rounded-md bg-indigo-500 text-white hover:bg-indigo-600 transition-colors">
              保存
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- 右下角退出登录按钮 -->
    <button @click="logout" 
      class="fixed bottom-4 right-4 z-30 flex items-center gap-2 px-4 py-2 rounded-full shadow-lg transition-all duration-300 hover:scale-105"
      :class="[isDarkMode ? 'bg-gray-800 text-red-400 hover:bg-gray-700' : 'bg-white text-red-600 hover:bg-gray-100']">
      <span>退出登录</span>
      <LogOutIcon class="w-5 h-5" />
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, computed } from 'vue'
import {
  UploadCloudIcon,
  SendIcon,
  ClipboardListIcon,
  XIcon,
  TrashIcon,
  FileIcon,
  ClockIcon,
  ShieldCheckIcon,
  ClipboardCopyIcon,
  LogOutIcon,
  Share2Icon,
  PencilIcon
} from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import BorderProgressBar from '@/components/common/BorderProgressBar.vue'
import { useFileDataStore } from '@/stores/fileData'
import { copyRetrieveLink, copyRetrieveCode } from '@/utils/clipboard'
import { getStorageUnit } from '@/utils/convert'
import axios from 'axios'

const config: any = JSON.parse(localStorage.getItem('config') || '{}')

const router = useRouter()
const isDarkMode = inject('isDarkMode')
const fileDataStore = useFileDataStore()

const selectedUploadFile = ref<File | null>(null)
const selectedFile = ref<FileItem | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)
const uploadProgress = ref(0)
const showDrawer = ref(false)
import { useAlertStore } from '@/stores/alertStore'

const alertStore = useAlertStore()
const sendRecords = computed(() => fileDataStore.shareData)

const fileHash = ref('')
const baseUrl = window.location.origin + '/#/'

// 文件管理相关
const isLoadingFiles = ref(false)
const userFiles = ref<FileItem[]>([])
const showShareDialog = ref(false)
const showDeleteConfirm = ref(false)
const fileToDelete = ref<FileItem | null>(null)
const shareResult = ref<ShareResult | null>(null)
const isCreatingShare = ref(false)

// 重命名弹窗相关
const showRenameDialog = ref(false)
const fileToRename = ref<FileItem | null>(null)
const newFileName = ref('')

// 添加日期选择器相关变量
const expirationDate = ref('')
const minExpirationDate = computed(() => {
  const today = new Date()
  return today.toISOString().split('T')[0]
})
const maxExpirationDate = computed(() => {
  const today = new Date()
  const maxDays = config.max_save_seconds ? Math.floor(config.max_save_seconds / 86400) : 1825
  const maxDate = new Date(today)
  maxDate.setDate(today.getDate() + maxDays)
  return maxDate.toISOString().split('T')[0]
})

const triggerFileUpload = () => {
  fileInput.value?.click()
}

const handleFileUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    const file = target.files[0]
    selectedUploadFile.value = file
    if (!checkUpload()) return
    fileHash.value = await calculateFileHash(file)
    console.log(fileHash.value)
  }
}

const handleFileDrop = async (event: DragEvent) => {
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    const file = event.dataTransfer.files[0]
    selectedUploadFile.value = file
    if (!checkUpload()) return
    fileHash.value = await calculateFileHash(file)
  }
}

/**
 * 处理粘贴事件，允许用户直接从剪贴板粘贴图片文件
 * @param event 剪贴板事件对象
 */
const handlePaste = async (event: ClipboardEvent) => {
  const items = event.clipboardData?.items
  if (!items) return

  for (const item of items) {
    if (item.kind === 'file') {
      const file = item.getAsFile()
      if (file) {
        // 检查文件是否为空
        if (file.size === 0) {
          alertStore.showAlert('无法读取空文件', 'error')
          return
        }

        // 检查文件类型
        if (file.type.startsWith('image/')) {
          selectedUploadFile.value = file
          if (!checkUpload()) return

          try {
            fileHash.value = await calculateFileHash(file)
            alertStore.showAlert('已从剪贴板添加图片：' + file.name, 'success')
          } catch (error) {
            alertStore.showAlert('文件处理失败', 'error')
            console.error('File hash calculation failed:', error)
          }
        } else {
          alertStore.showAlert('目前仅支持粘贴图片文件', 'warning')
        }
        break
      }
    }
  }
}

/**
 * 计算文件的SHA-256哈希值，如果不支持则使用备用哈希方法
 * @param file 要计算哈希的文件对象
 * @returns 返回文件的哈希值字符串
 */
const calculateFileHash = async (file: File): Promise<string> => {
  return new Promise((resolve) => {
    const chunkSize = 2097152 // 保持 2MB 的切片大小用于计算哈希
    const fileReader = new FileReader()
    let currentChunk = 0
    const chunks = Math.ceil(file.size / chunkSize)

    fileReader.onload = async (e) => {
      const chunk = new Uint8Array(e.target!.result as ArrayBuffer)

      try {
        // 尝试使用 crypto.subtle.digest
        if (window.isSecureContext) {
          const hashBuffer = await crypto.subtle.digest('SHA-256', chunk)
          const hashArray = Array.from(new Uint8Array(hashBuffer))
          const hashHex = hashArray.map((b) => b.toString(16).padStart(2, '0')).join('')

          currentChunk++
          if (currentChunk < chunks) {
            loadNext()
          } else {
            resolve(hashHex)
          }
        } else {
          // 如果不是安全上下文（HTTP），则返回一个基于文件信息的替代哈希
          const fallbackHash = generateFallbackHash(file)
          resolve(fallbackHash)
        }
      } catch (error) {
        // 如果 crypto.subtle.digest 失败，使用替代方案
        const fallbackHash = generateFallbackHash(file)
        resolve(fallbackHash)
      }
    }

    const loadNext = () => {
      const start = currentChunk * chunkSize
      const end = start + chunkSize >= file.size ? file.size : start + chunkSize
      fileReader.readAsArrayBuffer(file.slice(start, end))
    }

    loadNext()
  })
}

/**
 * 生成基于文件属性的备用哈希值，当无法使用标准加密API时使用
 * @param file 要生成哈希的文件对象
 * @returns 返回基于文件名、大小和修改时间的哈希字符串
 */
const generateFallbackHash = (file: File): string => {
  // 使用文件名、大小和最后修改时间生成一个简单的哈希
  const fileInfo = `${file.name}-${file.size}-${file.lastModified}`
  let hash = 0
  for (let i = 0; i < fileInfo.length; i++) {
    const char = fileInfo.charCodeAt(i)
    hash = ((hash << 5) - hash) + char
    hash = hash & hash // Convert to 32-bit integer
  }
  // 转换为16进制字符串并填充到64位
  return Math.abs(hash).toString(16).padStart(64, '0')
}

/**
 * 处理表单提交，上传选择的文件
 */
const handleSubmit = async () => {
  if (!selectedUploadFile.value) {
    alertStore.showAlert('请选择要上传的文件', 'error')
    return
  }

  try {
    await handleDirectFileUpload(selectedUploadFile.value!)
  } catch (error: any) {
    console.error('发送失败:', error)
    if (error.response?.data?.message) {
      alertStore.showAlert(error.response.data.message, 'error')
    } else if (error.message) {
      alertStore.showAlert(error.message, 'error')
    } else {
      alertStore.showAlert('发送失败,请稍后重试', 'error')
    }
  } finally {
    uploadProgress.value = 0
  }
}

// 定义响应类型以解决类型检查问题
interface UploadResponse {
  code: number;
  detail: {
    code: string;
    name: string;
    expiresAt?: string;
    message?: string;
  };
}

// 定义文件类型接口
interface FileItem {
  id: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
  hasActiveShare: boolean;
}

// 定义分享结果接口
interface ShareResult {
  shareCode: string;
  fileName: string;
  expiresAt: string;
}

/**
 * 执行文件上传操作
 * @param file 要上传的文件对象
 */
const handleDirectFileUpload = async (file: File): Promise<void> => {
  try {
    // 创建FormData对象
    const formData = new FormData();
    formData.append('file', file);
    
    // 确保axios默认设置包含凭据
    axios.defaults.withCredentials = true;
    
    // 发送文件上传请求
    const response = await axios.post('https://localhost:5001/api/file/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      withCredentials: true,
      onUploadProgress: (progressEvent: any) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        uploadProgress.value = percentCompleted;
      }
    });
    
    console.log('文件上传响应:', response.data);
    
    // 处理响应
    if (response.data && response.data.success) {
      // 文件上传成功，提示用户并重置表单
      alertStore.showAlert(`文件上传成功！您可以在文件管理中查看和分享该文件`, 'success');
      
      // 重置表单
      selectedUploadFile.value = null;
      uploadProgress.value = 0;
      
      // 打开文件管理抽屉并刷新文件列表
      showDrawer.value = true;
      await fetchUserFiles(); // 等待文件列表刷新完成
    } else {
      throw new Error(response.data?.message || '文件上传失败');
    }
  } catch (error: any) {
    // 简化错误处理
    let errorMessage = '上传失败，请稍后重试';
    
    if (error.response) {
      if (error.response.status === 401) {
        errorMessage = '登录已过期，请重新登录';
        router.push('/login');
      } else if (error.response.data && error.response.data.message) {
        errorMessage = error.response.data.message;
      } else {
        errorMessage = `上传失败 (${error.response.status})`;
      }
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    alertStore.showAlert(errorMessage, 'error');
    throw error;
  }
};

const toRetrieve = () => {
  router.push('/retrieve')
}

const toggleDrawer = () => {
  showDrawer.value = !showDrawer.value
  if (showDrawer.value) {
    fetchUserFiles()
  }
}

/**
 * 用户登出
 */
const logout = async () => {
  console.log('Logout function triggered')
  alertStore.showAlert('正在退出登录...', 'info')
  
  try {
    // 确保axios默认设置包含凭据
    axios.defaults.withCredentials = true;
    
    // 调用登出API
    console.log('Calling logout API at: https://localhost:5001/api/user/logout')
    await axios.post('https://localhost:5001/api/user/logout', {}, {
      withCredentials: true // 确保发送认证Cookie
    })
    
    // 清除本地登录状态标志
    localStorage.removeItem('token')
    localStorage.removeItem('isLoggedIn')
    alertStore.showAlert('已成功退出登录', 'success')
    router.push('/login')
  } catch (error) {
    console.error('Logout error:', error)
    // 即使API调用失败，也清除本地登录状态
    localStorage.removeItem('token')
    localStorage.removeItem('isLoggedIn')
    alertStore.showAlert('登出过程中发生错误，但已清除本地登录状态', 'warning')
    router.push('/login')
  }
}

// 使用 onMounted 钩子延迟加载一些非关键资源或初始化
onMounted(() => {
  // 这里可以放置一些非立即需要的初始化代码
  console.log('SendFileView mounted')
  if (localStorage.getItem('isLoggedIn') === 'true') {
    fetchUserFiles()
  }
})

/**
 * 检查上传条件是否满足
 * @returns 如果满足上传条件返回true，否则返回false
 */
const checkUpload = () => {
  if (!checkOpenUpload()) return false
  if (!checkFileSize(selectedUploadFile.value!)) return false
  return true
}

/**
 * 检查是否允许游客上传
 * @returns 如果允许上传返回true，否则返回false
 */
const checkOpenUpload = () => {
  // 检查是否允许游客上传，使用isLoggedIn标志判断是否登录
  const isLoggedIn = localStorage.getItem('isLoggedIn') === 'true';
  if (config.openUpload === 0 && !isLoggedIn) {
    alertStore.showAlert('游客上传功能已关闭，请先登录', 'error')
    return false
  }
  return true
}

/**
 * 检查文件大小是否超过限制
 * @param file 要检查的文件对象
 * @returns 如果文件大小合适返回true，否则返回false
 */
const checkFileSize = (file: File) => {
  if (file.size > config.uploadSize) {
    alertStore.showAlert(`文件大小超过限制 (${getStorageUnit(config.uploadSize)})`, 'error')
    selectedUploadFile.value = null
    return false
  }
  return true
}

/**
 * 获取用户的文件列表
 */
const fetchUserFiles = async () => {
  try {
    isLoadingFiles.value = true
    const response = await axios.get('https://localhost:5001/api/file', {
      withCredentials: true
    })
    
    console.log('获取文件列表响应:', response.data)
    
    if (response.data && response.data.success) {
      // 兼容 .NET 返回的 $values 数组
      if (Array.isArray(response.data.data)) {
        userFiles.value = response.data.data
      } else if (response.data.data && Array.isArray(response.data.data.$values)) {
        userFiles.value = response.data.data.$values
      } else if (response.data.files) {
        userFiles.value = response.data.files
      } else {
        userFiles.value = []
        console.warn('文件列表数据格式不正确:', response.data)
      }
    } else {
      userFiles.value = []
      alertStore.showAlert('获取文件列表失败', 'error')
    }
  } catch (error: any) {
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

/**
 * 格式化文件大小显示
 * @param bytes 文件大小（字节）
 * @returns 格式化后的文件大小字符串
 */
const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  else if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB'
  else if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(2) + ' MB'
  else return (bytes / (1024 * 1024 * 1024)).toFixed(2) + ' GB'
}

/**
 * 格式化日期时间显示
 * @param dateValue 日期值（字符串或数字）
 * @returns 格式化后的日期时间字符串
 */
const formatDate = (dateValue: string | number) => {
  if (!dateValue) return ''
  const date = typeof dateValue === 'string' ? new Date(dateValue) : new Date(dateValue)
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

/**
 * 打开文件分享对话框
 * @param file 要分享的文件对象
 */
const shareFile = (file: FileItem) => {
  console.log("准备分享文件:", { 
    id: file.id, 
    fileName: file.fileName, 
    fileSize: formatFileSize(file.fileSize),
    uploadedAt: formatDate(file.uploadedAt),
    hasActiveShare: file.hasActiveShare
  });
  selectedFile.value = file
  shareResult.value = null // 重置分享结果
  
  // 设置默认的过期日期（7天后）
  const defaultDate = new Date()
  defaultDate.setDate(defaultDate.getDate() + 7)
  expirationDate.value = defaultDate.toISOString().split('T')[0]
  
  showShareDialog.value = true
}

/**
 * 关闭文件分享对话框
 */
const closeShareDialog = () => {
  showShareDialog.value = false
  selectedUploadFile.value = null
}

/**
 * 创建文件分享链接
 */
const createFileShare = async () => {
  if (!selectedFile.value) return
  
  try {
    isCreatingShare.value = true
    
    // 从选择的日期计算天数
    let expirationDays = 7; // 默认7天
    
    if (expirationDate.value) {
      const today = new Date();
      const selectedDate = new Date(expirationDate.value);
      const diffTime = selectedDate.getTime() - today.getTime();
      expirationDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    }
    
    // 如果expirationDays小于1, 设为1
    expirationDays = Math.max(1, Math.round(expirationDays));
    
    // 创建分享
    const response = await axios.post('https://localhost:5001/api/file/share', {
      fileId: selectedFile.value.id,
      expirationDays: expirationDays
    }, {
      headers: {
        'Content-Type': 'application/json'
      },
      withCredentials: true
    });
    
    if (response.data && response.data.success) {
      // 兼容不同后端返回结构
      if (response.data.share) {
        shareResult.value = response.data.share
      } else if (response.data.data) {
        shareResult.value = response.data.data
      } else {
        shareResult.value = response.data
      }
      // 自动弹出分享弹窗
      showShareDialog.value = true
      // 可选：存储到本地
      // localStorage.setItem('lastShareCode', shareResult.value.shareCode)
      alertStore.showAlert('创建分享成功', 'success')
      fetchUserFiles()
    } else {
      throw new Error(response.data?.message || '创建分享失败');
    }
  } catch (error: any) {
    let errorMessage = '创建分享失败，请稍后重试';
    
    if (error.response) {
      if (error.response.status === 401) {
        errorMessage = '登录已过期，请重新登录';
        router.push('/login');
      } else if (error.response.data && error.response.data.message) {
        errorMessage = error.response.data.message;
      }
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    alertStore.showAlert(errorMessage, 'error');
  } finally {
    isCreatingShare.value = false
  }
}

/**
 * 开始文件重命名操作
 * @param file 要重命名的文件对象
 */
const startRenaming = (file: FileItem) => {
  console.log("准备重命名文件:", { id: file.id, fileName: file.fileName });
  fileToRename.value = file;
  newFileName.value = file.fileName;
  showRenameDialog.value = true;
}

/**
 * 关闭文件重命名对话框
 */
const closeRenameDialog = () => {
  showRenameDialog.value = false;
  fileToRename.value = null;
  newFileName.value = '';
}

/**
 * 确认重命名文件
 */
const confirmRename = async () => {
  if (!fileToRename.value) return;
  if (!newFileName.value || newFileName.value.trim() === '') {
    alertStore.showAlert('文件名不能为空', 'error');
    return;
  }
  
  try {
    const requestData = {
      fileId: fileToRename.value.id,
      newFileName: newFileName.value
    };
    
    const response = await axios.put('https://localhost:5001/api/file/rename', requestData, { 
      headers: {
        'Content-Type': 'application/json'
      },
      withCredentials: true 
    });
    
    if (response.data && response.data.success) {
      // 更新文件列表中的文件名
      const targetFile = userFiles.value.find(f => f.id === fileToRename.value!.id);
      if (targetFile) {
        targetFile.fileName = newFileName.value;
      }
      alertStore.showAlert('重命名成功', 'success');
      closeRenameDialog();
    } else {
      throw new Error(response.data?.message || '重命名失败')
    }
  } catch (error: any) {
    let errorMessage = '重命名失败，请稍后重试'
    
    if (error.response) {
      if (error.response.status === 401) {
        errorMessage = '未授权，请重新登录';
        router.push('/login');
      } else if (error.response.status === 404) {
        errorMessage = '文件不存在';
      } else if (error.response.status === 400) {
        errorMessage = '请求参数错误: ' + (error.response.data?.message || '参数格式不正确');
      } else if (error.response.data?.message) {
        errorMessage = error.response.data.message;
      }
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    alertStore.showAlert(errorMessage, 'error');
  }
}

/**
 * 确认删除文件，显示确认对话框
 * @param file 要删除的文件对象
 */
const confirmDeleteFile = (file: FileItem) => {
  console.log("准备删除文件:", { id: file.id, fileName: file.fileName });
  fileToDelete.value = file
  showDeleteConfirm.value = true
}

/**
 * 执行文件删除操作
 */
const deleteFile = async () => {
  if (!fileToDelete.value) return
  
  try {
    const response = await axios.delete(`https://localhost:5001/api/file/${fileToDelete.value.id}`, {
      withCredentials: true
    });
    
    if (response.data && response.data.success) {
      // 从列表中移除文件
      userFiles.value = userFiles.value.filter(f => f.id !== fileToDelete.value!.id)
      alertStore.showAlert('文件删除成功', 'success')
    } else {
      throw new Error(response.data?.message || '删除文件失败')
    }
  } catch (error: any) {
    let errorMessage = '删除失败，请稍后重试'
    
    if (error.response) {
      if (error.response.status === 401) {
        errorMessage = '未授权，请重新登录';
        router.push('/login');
      } else if (error.response.status === 404) {
        errorMessage = '文件不存在';
      } else if (error.response.data?.message) {
        errorMessage = error.response.data.message;
      }
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    alertStore.showAlert(errorMessage, 'error')
  } finally {
    showDeleteConfirm.value = false
    fileToDelete.value = null
  }
}

// 添加isLoggedIn计算属性
const isLoggedIn = computed(() => {
  return localStorage.getItem('isLoggedIn') === 'true'
})
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition:
    opacity 0.3s ease,
    transform 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

@media (min-width: 640px) {
  .sm\:w-120 {
    width: 30rem;
    /* 480px */
  }
}

.fade-enter-to,
.fade-leave-from {
  opacity: 1;
  transform: translateY(0);
}

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

select option {
  padding: 8px;
  margin: 4px;
  border-radius: 6px;
}

select option:checked {
  background: linear-gradient(to right, rgb(99 102 241 / 0.5), rgb(168 85 247 / 0.5)) !important;
  color: white !important;
}

.dark select option:checked {
  background: linear-gradient(to right, rgb(99 102 241 / 0.7), rgb(168 85 247 / 0.7)) !important;
}

select option:hover {
  background-color: rgb(99 102 241 / 0.1);
}

.dark select option:hover {
  background-color: rgb(99 102 241 / 0.2);
}
</style>

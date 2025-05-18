<template>  <div class="p-6 min-h-fit custom-scrollbar">
    <!-- 页面标题和统计信息 -->
    <div class="mb-8">
      <h2 class="text-2xl font-bold mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
        用户管理
      </h2>
    </div>

    <!-- 搜索和操作栏 -->
    <div
      class="mb-6 flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between bg-opacity-70 p-4 rounded-lg shadow-sm"
      :class="[isDarkMode ? 'bg-gray-800' : 'bg-white']">
      <div class="flex flex-1 gap-4 w-full sm:w-auto">
        <div class="relative flex-1">
          <input type="text" v-model="params.keyword" @keyup.enter="handleSearch" :class="[
            isDarkMode
              ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400'
              : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400',
            'w-full pl-10 pr-4 py-2.5 rounded-lg border focus:ring-2 focus:ring-indigo-500 focus:border-transparent'
          ]" placeholder="搜索用户名..." />
          <SearchIcon class="absolute left-3 top-3 w-5 h-5" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']" />
        </div>
        <button @click="handleSearch"
          class="px-4 py-2.5 rounded-lg inline-flex items-center transition-all duration-200 bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm">
          <SearchIcon class="w-5 h-5 mr-2" />
          搜索
        </button>
      </div>
    </div>

    <!-- 用户列表 -->
    <div class="rounded-lg shadow-sm overflow-hidden transition-all duration-300"
      :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
      <div class="px-6 py-4 border-b" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
        <h3 class="text-lg font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
          所有用户
        </h3>
      </div>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y" :class="[isDarkMode ? 'divide-gray-700' : 'divide-gray-200']">
          <thead :class="[isDarkMode ? 'bg-gray-900/50' : 'bg-gray-50']">
            <tr>
              <th v-for="header in userTableHeaders" :key="header"
                class="px-6 py-3.5 text-left text-xs font-medium uppercase tracking-wider"
                :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                {{ header }}
              </th>
            </tr>
          </thead>
          <tbody :class="[
            isDarkMode
              ? 'bg-gray-800/50 divide-y divide-gray-700'
              : 'bg-white divide-y divide-gray-200'
          ]">
            <tr v-for="user in tableData" :key="user.id" class="hover:bg-opacity-50 transition-colors duration-200"
              :class="[isDarkMode ? 'hover:bg-gray-700' : 'hover:bg-gray-50']">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <span class="font-medium cursor-pointer group relative" :class="[isDarkMode ? 'text-white' : 'text-gray-900']" @click="toggleIdDisplay($event)">
                    {{ truncateId(user.id) }}
                    <!-- 悬浮提示 -->
                    <div
                      class="absolute left-0 -top-2 -translate-y-full opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                      <div class="bg-gray-900 text-white text-sm rounded px-2 py-1 max-w-xs break-all">
                        {{ user.id }}
                      </div>
                    </div>
                  </span>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center">
                  <UserIcon class="w-5 h-5 mr-2 flex-shrink-0"
                    :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-500']" />
                  <span class="font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
                    {{ user.username }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4">
                <span class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                  {{ user.email }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                  :class="[
                    user.isActive 
                      ? (isDarkMode ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-800')
                      : (isDarkMode ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-800')
                  ]">
                  {{ user.isActive ? '活跃' : '禁用' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                  {{ formatTimestamp(user.createdAt) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <div class="flex items-center space-x-2">
                  <button @click="viewUserFiles(user.id)"
                    class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
                      isDarkMode
                        ? 'bg-blue-900/20 text-blue-400 hover:bg-blue-900/30'
                        : 'bg-blue-50 text-blue-600 hover:bg-blue-100'
                    ]">
                    <FileIcon class="w-4 h-4 mr-1.5" />
                    文件
                  </button>
                  <button @click="openResetPasswordModal(user.id)"
                    class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
                      isDarkMode
                        ? 'bg-yellow-900/20 text-yellow-400 hover:bg-yellow-900/30'
                        : 'bg-yellow-50 text-yellow-600 hover:bg-yellow-100'
                    ]">
                    <KeyIcon class="w-4 h-4 mr-1.5" />
                    重置密码
                  </button>
                  <button @click="deleteUser(user.id)"
                    class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
                      isDarkMode
                        ? 'bg-red-900/20 text-red-400 hover:bg-red-900/30'
                        : 'bg-red-50 text-red-600 hover:bg-red-100'
                    ]">
                    <TrashIcon class="w-4 h-4 mr-1.5" />
                    删除
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 分页控件 -->
      <div class="mt-4 flex items-center justify-between px-6 py-4 border-t"
        :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
        <div class="flex items-center text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
          显示第 {{ (params.page - 1) * params.size + 1 }} 到
          {{ Math.min(params.page * params.size, params.total) }} 条，共 {{ params.total }} 条
        </div>

        <div class="flex items-center space-x-2">
          <button @click="handlePageChange(params.page - 1)" :disabled="params.page === 1"
            class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
              isDarkMode
                ? params.page === 1
                  ? 'bg-gray-800 text-gray-600 cursor-not-allowed'
                  : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                : params.page === 1
                  ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            ]">
            <ChevronLeftIcon class="w-4 h-4" />
            上一页
          </button>

          <div class="flex items-center space-x-1">
            <template v-for="pageNum in displayedPages" :key="pageNum">
              <button v-if="pageNum !== '...'" @click="handlePageChange(pageNum)"
                class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
                  params.page === pageNum
                    ? 'bg-indigo-600 text-white'
                    : isDarkMode
                      ? 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                ]">
                {{ pageNum }}
              </button>
              <span v-else class="px-2" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                ...
              </span>
            </template>
          </div>

          <button @click="handlePageChange(params.page + 1)" :disabled="params.page >= totalPages"
            class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
              isDarkMode
                ? params.page >= totalPages
                  ? 'bg-gray-800 text-gray-600 cursor-not-allowed'
                  : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                : params.page >= totalPages
                  ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            ]">
            下一页
            <ChevronRightIcon class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <!-- 重置密码弹窗 -->
    <div v-if="showResetPasswordModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl p-6 w-full max-w-md animate-modal-scale">
        <h3 class="text-lg font-medium mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
          重置用户密码
        </h3>
        <div class="mb-4">
          <label for="new-password" class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
            新密码
          </label>
          <input
            type="password"
            id="new-password"
            v-model="newPassword"
            class="w-full px-3 py-2 border rounded-md"
            :class="[isDarkMode ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-900']"
            placeholder="输入新密码"
          />
        </div>
        <div class="mb-4">
          <label for="confirm-password" class="block text-sm font-medium mb-1" :class="[isDarkMode ? 'text-gray-300' : 'text-gray-700']">
            确认密码
          </label>
          <input
            type="password"
            id="confirm-password"
            v-model="confirmPassword"
            class="w-full px-3 py-2 border rounded-md"
            :class="[isDarkMode ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-900']"
            placeholder="再次输入新密码"
          />
        </div>
        <div class="flex justify-end space-x-3">
          <button
            @click="closeResetPasswordModal"
            class="px-4 py-2 rounded-md transition-colors duration-200"
            :class="[isDarkMode ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-200 text-gray-700 hover:bg-gray-300']"
          >
            取消
          </button>
          <button
            @click="resetUserPassword"
            class="px-4 py-2 rounded-md bg-indigo-600 text-white hover:bg-indigo-700 transition-colors duration-200"
          >
            确认
          </button>
        </div>
      </div>
    </div>

    <!-- 用户文件弹窗 -->
    <div v-if="showUserFilesModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl p-6 w-full max-w-4xl h-3/4 flex flex-col animate-modal-scale">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
            用户文件
          </h3>
          <button
            @click="closeUserFilesModal"
            class="rounded-full p-1 hover:bg-opacity-10 transition-colors duration-200"
            :class="[isDarkMode ? 'hover:bg-gray-500' : 'hover:bg-gray-200']"
          >
            <XIcon class="w-6 h-6" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']" />
          </button>
        </div>
        <div class="overflow-y-auto flex-1">
          <table class="min-w-full divide-y" :class="[isDarkMode ? 'divide-gray-700' : 'divide-gray-200']">
            <thead :class="[isDarkMode ? 'bg-gray-900/50' : 'bg-gray-50']">
              <tr>
                <th v-for="header in fileTableHeaders" :key="header"
                  class="px-6 py-3.5 text-left text-xs font-medium uppercase tracking-wider"
                  :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                  {{ header }}
                </th>
              </tr>
            </thead>
            <tbody :class="[
              isDarkMode
                ? 'bg-gray-800/50 divide-y divide-gray-700'
                : 'bg-white divide-y divide-gray-200'
            ]">
              <tr v-for="file in userFiles" :key="file.id" class="hover:bg-opacity-50 transition-colors duration-200"
                :class="[isDarkMode ? 'hover:bg-gray-700' : 'hover:bg-gray-50']">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <span class="font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
                      {{ truncateId(file.id) }}
                    </span>
                  </div>
                </td>
                <td class="px-6 py-4">
                  <div class="flex items-center">
                    <FileIcon class="w-5 h-5 mr-2 flex-shrink-0"
                      :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-500']" />
                    <span class="font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-900']">
                      {{ file.filename }}
                    </span>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    :class="[isDarkMode ? 'bg-gray-700 text-gray-300' : 'bg-gray-100 text-gray-800']">
                    {{ Math.round((file.fileSize / 1024) * 100) / 100 }}KB
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                    {{ formatTimestamp(file.uploadedAt) }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div class="flex items-center justify-end space-x-2">
                    <button @click="downloadFile(file)"
                      class="inline-flex items-center px-3 py-1.5 rounded-md transition-colors duration-200" :class="[
                        isDarkMode
                          ? 'bg-green-900/20 text-green-400 hover:bg-green-900/30'
                          : 'bg-green-50 text-green-600 hover:bg-green-100'
                      ]">
                      <DownloadIcon class="w-4 h-4 mr-1.5" />
                      下载
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="userFiles.length === 0">
                <td :colspan="fileTableHeaders.length" class="px-6 py-8 text-center" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']">
                  没有找到文件
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { inject, ref, computed } from 'vue'
import api from '@/utils/api'
import {
  UserIcon,
  FileIcon,
  SearchIcon,
  TrashIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  KeyIcon,
  XIcon,
  DownloadIcon
} from 'lucide-vue-next'
import { useAlertStore } from '@/stores/alertStore'
import type { AxiosResponse } from 'axios'

// Define interfaces for our data types
interface User {
  $id?: string;
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  [key: string]: any; // For any additional fields
}

interface UserFile {
  id: string;
  filename: string;
  fileSize: number;
  uploadedAt: string;
  [key: string]: any; // For any additional fields
}

const isDarkMode = inject<boolean>('isDarkMode')
const tableData = ref<User[]>([])
const allUsers = ref<User[]>([]) // 存储所有原始用户数据，用于搜索恢复
const alertStore = useAlertStore()

// 用户表头
const userTableHeaders = ['ID', '用户名', '邮箱', '状态', '创建时间', '操作']
const fileTableHeaders = ['ID', '文件名', '大小', '上传时间', '操作']

// 分页参数
const params = ref({
  page: 1,
  size: 5,
  total: 0,
  keyword: ''
})

// 用户密码重置相关
const showResetPasswordModal = ref(false)
const selectedUserId = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

// 用户文件相关
const showUserFilesModal = ref(false)
const userFiles = ref<UserFile[]>([])

// 格式化时间戳
function formatTimestamp(timestamp: string): string {
  if (!timestamp) return '-'
  const date = new Date(timestamp)
  const year = date.getFullYear()
  const month = (date.getMonth() + 1).toString().padStart(2, '0')
  const day = date.getDate().toString().padStart(2, '0')
  const hours = date.getHours().toString().padStart(2, '0')
  const minutes = date.getMinutes().toString().padStart(2, '0')
  const seconds = date.getSeconds().toString().padStart(2, '0')
  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`
}

// 加载用户列表
const loadUsers = async () => {
  try {
    // 创建查询参数对象
    const queryParams = {
      page: params.value.page,
      size: params.value.size
    };
    
    // 只用于记录
    if (params.value.keyword && params.value.keyword.trim() !== '') {
      console.log('搜索关键词:', params.value.keyword);
    }
    
    console.log('发送查询参数:', queryParams);
    
    const res: AxiosResponse = await api({
      url: '/api/admin/users',
      method: 'get',
      params: queryParams
    })
    console.log('Users API response:', res)
    
    // 基于实际API响应结构调整数据解析
    let responseData: any[] = [];
    if (res.data && res.data.data && res.data.data.$values) {
      // 数据结构: res.data.data.$values
      responseData = res.data.data.$values;
      params.value.total = res.data.data.$values.length;
    } else if (res.data && res.data.$values) {
      // 数据结构: res.data.$values
      responseData = res.data.$values;
      params.value.total = res.data.$values.length;
    } else if (res.data && Array.isArray(res.data.data)) {
      // 数据结构: res.data.data[]
      responseData = res.data.data;
      params.value.total = res.data.data.length;
    } else if (res.data && Array.isArray(res.data)) {
      // 数据结构: res.data[]
      responseData = res.data;
      params.value.total = res.data.length;
    } else {
      // 兜底
      responseData = [];
      params.value.total = 0;
      console.error('未知的API响应结构:', res);
    }
    
    // 处理用户数据
    const processedData: User[] = responseData.map((item: any) => {
      console.log('原始用户项数据:', item);
      
      return {
        $id: item.$id || '',
        id: item.id || '',
        username: item.username || item.userName || item.name || '',
        email: item.email || item.emailAddress || '',
        isActive: item.isActive !== false, // 默认为活跃状态
        createdAt: item.createdAt || item.created_at || new Date().toISOString()
      };
    });
    
    // 保存处理后的完整数据，用于搜索过滤
    allUsers.value = [...processedData];
    
    // 表格数据根据搜索关键词决定显示全部还是过滤后的
    if (params.value.keyword && params.value.keyword.trim() !== '') {
      // 如果有搜索关键词，立即应用搜索过滤
      applySearchFilter(params.value.keyword.trim());
    } else {
      // 否则显示所有数据
      tableData.value = processedData;
    }
    
  } catch (error: any) {
    console.error('加载用户列表失败:', error);
    const errorMessage = error.response?.data?.detail || error.message || '加载用户列表失败';
    alertStore.showAlert(errorMessage, 'error');
    // 失败时设置空数据
    tableData.value = [];
    params.value.total = 0;
  }
}

// 前端搜索过滤函数
const applySearchFilter = (keyword: string) => {
  if (!keyword) {
    // 如果没有关键词，显示所有用户
    tableData.value = [...allUsers.value];
    params.value.total = allUsers.value.length;
    return;
  }
  
  const lowerCaseKeyword = keyword.toLowerCase();
  // 在前端过滤数据
  const filteredData = allUsers.value.filter((user: User) => {
    // 在多个字段中搜索关键词
    const username = (user.username || '').toLowerCase();
    const email = (user.email || '').toLowerCase();
    
    return username.includes(lowerCaseKeyword) || 
           email.includes(lowerCaseKeyword);
  });
  
  console.log(`搜索结果: 从 ${allUsers.value.length} 项中过滤出 ${filteredData.length} 项`);
  
  // 更新表格数据和分页计数
  tableData.value = filteredData;
  params.value.total = filteredData.length;
}

// 页码改变处理函数
const handlePageChange = async (page: number | string) => {
  const numPage = typeof page === 'string' ? parseInt(page) : page;
  if (numPage < 1 || numPage > totalPages.value) return;
  params.value.page = numPage;
  
  // 如果是搜索状态，不需要重新加载数据
  if (params.value.keyword && params.value.keyword.trim() !== '') {
    console.log(`搜索状态切换到第 ${numPage} 页，前端分页`);
  } else {
    await loadUsers();
  }
}

// 搜索处理函数
const handleSearch = async () => {
  console.log('执行搜索，关键词:', params.value.keyword);
  params.value.page = 1; // 重置页码到第一页
  
  if (params.value.keyword && params.value.keyword.trim() !== '') {
    // 有搜索关键词时，在前端过滤
    applySearchFilter(params.value.keyword.trim());
  } else {
    // 无搜索关键词时，恢复显示所有用户
    tableData.value = [...allUsers.value];
    params.value.total = allUsers.value.length;
  }
}

// 计算总页数
const totalPages = computed(() => Math.ceil(params.value.total / params.value.size))

// 初始加载
loadUsers()

// 计算要显示的页码
const displayedPages = computed(() => {
  const current = params.value.page
  const total = totalPages.value
  const delta = 2 // 当前页码前后显示的页码数

  let pages: (number | string)[] = []

  // 始终显示第一页
  pages.push(1)

  // 计算显示范围
  let left = Math.max(2, current - delta)
  let right = Math.min(total - 1, current + delta)

  // 添加省略号和页码
  if (left > 2) {
    pages.push('...')
  }

  for (let i = left; i <= right; i++) {
    pages.push(i)
  }

  if (right < total - 1) {
    pages.push('...')
  }

  // 始终显示最后一页
  if (total > 1) {
    pages.push(total)
  }

  return pages
})

// ID截断函数
const truncateId = (id: string): string => {
  if (!id) return '';
  // 显示前8个字符
  return id.substring(0, 8) + '...';
}

// 点击ID显示完整内容的功能
const toggleIdDisplay = (event: MouseEvent) => {
  const target = event.target as HTMLElement;
  const fullId = target.parentElement?.querySelector('.group-hover\\:opacity-100 div')?.textContent?.trim();
  if (fullId) {
    // 复制到剪贴板
    navigator.clipboard.writeText(fullId)
      .then(() => {
        alertStore.showAlert('ID已复制到剪贴板', 'success');
      })
      .catch(() => {
        alertStore.showAlert('复制失败，请手动复制', 'error');
      });
  }
}

// 删除用户
const deleteUser = async (id: string) => {
  if (confirm('确定要删除此用户吗？此操作不可撤销。')) {
    try {
      await api({
        url: `/api/admin/users/${id}`,
        method: 'delete'
      })
      await loadUsers()
      alertStore.showAlert('用户删除成功', 'success')
    } catch (error: any) {
      console.error('删除失败:', error)
      alertStore.showAlert('删除失败', 'error')
    }
  }
}

// 打开重置密码弹窗
const openResetPasswordModal = (userId: string) => {
  selectedUserId.value = userId
  newPassword.value = ''
  confirmPassword.value = ''
  showResetPasswordModal.value = true
}

// 关闭重置密码弹窗
const closeResetPasswordModal = () => {
  showResetPasswordModal.value = false
  selectedUserId.value = ''
  newPassword.value = ''
  confirmPassword.value = ''
}

// 重置用户密码
const resetUserPassword = async () => {
  if (!newPassword.value) {
    alertStore.showAlert('请输入新密码', 'error')
    return
  }
  
  if (newPassword.value !== confirmPassword.value) {
    alertStore.showAlert('两次输入的密码不一致', 'error')
    return
  }
  
  try {
    await api({
      url: `/api/admin/users/${selectedUserId.value}/reset-password`,
      method: 'post',
      data: {
        newPassword: newPassword.value
      }
    })
    closeResetPasswordModal()
    alertStore.showAlert('密码重置成功', 'success')
  } catch (error: any) {
    console.error('密码重置失败:', error)
    alertStore.showAlert('密码重置失败', 'error')
  }
}

// 查看用户文件
const viewUserFiles = async (userId: string) => {
  try {
    const res: AxiosResponse = await api({
      url: `/api/admin/users/${userId}/files`,
      method: 'get'
    })
    
    // 解析文件数据，根据实际API响应调整
    let fileData: any[] = [];
    if (res.data && res.data.data && res.data.data.$values) {
      fileData = res.data.data.$values;
    } else if (res.data && res.data.$values) {
      fileData = res.data.$values;
    } else if (res.data && Array.isArray(res.data.data)) {
      fileData = res.data.data;
    } else if (res.data && Array.isArray(res.data)) {
      fileData = res.data;
    } else {
      fileData = [];
      console.error('未知的API响应结构:', res);
    }
    
    // 处理文件数据
    userFiles.value = fileData.map((item: any) => {
      return {
        id: item.id || '',
        filename: item.filename || item.name || item.fileName || '',
        fileSize: item.fileSize || item.size || 0,
        uploadedAt: item.uploadedAt || item.uploadAt || item.created_at || item.createdAt || new Date().toISOString()
      };
    });
    
    showUserFilesModal.value = true;
  } catch (error: any) {
    console.error('获取用户文件失败:', error);
    alertStore.showAlert('获取用户文件失败', 'error');
  }
}

// 关闭用户文件弹窗
const closeUserFilesModal = () => {
  showUserFilesModal.value = false;
  userFiles.value = [];
}

// 下载文件
const downloadFile = (file: UserFile) => {
  // 这里可以根据实际API实现文件下载
  // 示例：可以生成一个临时下载链接
  try {
    // 在实际项目中可能需要获取下载链接的API
    const downloadUrl = `/api/files/${file.id}/download`;
    
    // 创建一个临时链接来触发下载
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.download = file.filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    alertStore.showAlert('文件下载已开始', 'success');
  } catch (error: any) {
    console.error('文件下载失败:', error);
    alertStore.showAlert('文件下载失败', 'error');
  }
}
</script>

<style>
.animate-modal-scale {
  animation: modal-scale 0.3s ease-out;
}

@keyframes modal-scale {
  from {
    transform: scale(0.95);
    opacity: 0;
  }

  to {
    transform: scale(1);
    opacity: 1;
  }
}

/* 添加输入框聚焦时的微妙缩放效果 */
input:focus {
  transform: scale(1.002);
}
</style>
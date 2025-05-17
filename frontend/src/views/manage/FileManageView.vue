<template>  <div class="p-6 min-h-fit custom-scrollbar">
    <!-- 页面标题和统计信息 -->
    <div class="mb-8">
      <h2 class="text-2xl font-bold mb-4" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
        文件管理
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
          ]" placeholder="搜索文件名称..." />
          <SearchIcon class="absolute left-3 top-3 w-5 h-5" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-500']" />
        </div>
        <button @click="handleSearch"
          class="px-4 py-2.5 rounded-lg inline-flex items-center transition-all duration-200 bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm">
          <SearchIcon class="w-5 h-5 mr-2" />
          搜索
        </button>
      </div>
    </div>

    <!-- 文件列表 -->
    <div class="rounded-lg shadow-sm overflow-hidden transition-all duration-300"
      :class="[isDarkMode ? 'bg-gray-800 bg-opacity-70' : 'bg-white']">
      <div class="px-6 py-4 border-b" :class="[isDarkMode ? 'border-gray-700' : 'border-gray-200']">
        <h3 class="text-lg font-medium" :class="[isDarkMode ? 'text-white' : 'text-gray-800']">
          所有文件
        </h3>
      </div>
      <div class="overflow-x-auto">
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
            <tr v-for="file in tableData" :key="file.id" class="hover:bg-opacity-50 transition-colors duration-200"
              :class="[isDarkMode ? 'hover:bg-gray-700' : 'hover:bg-gray-50']">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <span class="font-medium cursor-pointer group relative" :class="[isDarkMode ? 'text-white' : 'text-gray-900']" @click="toggleIdDisplay($event)">
                    {{ truncateId(file.id) }}
                    <!-- 悬浮提示 -->
                    <div
                      class="absolute left-0 -top-2 -translate-y-full opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                      <div class="bg-gray-900 text-white text-sm rounded px-2 py-1 max-w-xs break-all">
                        {{ file.id }}
                      </div>
                    </div>
                  </span>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center group relative">
                  <FileIcon class="w-5 h-5 mr-2 flex-shrink-0"
                    :class="[isDarkMode ? 'text-indigo-400' : 'text-indigo-500']" />
                  <span class="font-medium truncate max-w-[200px]"
                    :class="[isDarkMode ? 'text-white' : 'text-gray-900']" :title="file.filename">
                    {{ file.filename }}
                  </span>
                  <!-- 悬浮提示 -->
                  <div
                    class="absolute left-0 -top-2 -translate-y-full opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                    <div class="bg-gray-900 text-white text-sm rounded px-2 py-1 max-w-xs break-all">
                      {{ file.filename }}
                    </div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                  :class="[isDarkMode ? 'bg-gray-700 text-gray-300' : 'bg-gray-100 text-gray-800']">
                  {{ Math.round((file.fileSize / 1024) * 100) / 100 }}KB
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="[
                  file.hasActiveShare
                    ? (isDarkMode ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-800')
                    : (isDarkMode ? 'bg-yellow-900/30 text-yellow-400' : 'bg-yellow-100 text-yellow-800')
                ]">
                  {{ file.hasActiveShare ? '已共享' : '未共享' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="text-sm" :class="[isDarkMode ? 'text-gray-400' : 'text-gray-600']">
                  {{ formatTimestamp(file.uploadedAt) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <div class="flex items-center space-x-2">
                  <button @click="deleteFile(file.id)"
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

  </div>
</template>

<script setup lang="ts">
import { inject, ref, computed } from 'vue'
import api from '@/utils/api'
import {
  FileIcon,
  SearchIcon,
  TrashIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  PencilIcon,
  CheckIcon
} from 'lucide-vue-next'
import { useAlertStore } from '@/stores/alertStore'

function formatTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  const year = date.getFullYear()
  const month = (date.getMonth() + 1).toString().padStart(2, '0')
  const day = date.getDate().toString().padStart(2, '0')
  const hours = date.getHours().toString().padStart(2, '0')
  const minutes = date.getMinutes().toString().padStart(2, '0')
  const seconds = date.getSeconds().toString().padStart(2, '0')
  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`
}

const isDarkMode = inject('isDarkMode')
const tableData: any = ref([])
const allFiles: any = ref([]) // 存储所有原始文件数据，用于搜索恢复
const alertStore = useAlertStore()
// 文件表头
const fileTableHeaders = ['ID', '文件名称', '大小', '共享状态', '上传时间', '操作']

// 分页参数
const params = ref({
  page: 1,
  size: 5,
  total: 0,
  keyword: ''
})

// 删除所有分享相关函数

// 删除下载相关函数

// 加载文件列表
const loadFiles = async () => {
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
    
    const res: any = await api({
      url: '/api/admin/files',
      method: 'get',
      params: queryParams
    })
    console.log('Files API response:', res)
    
    // 基于实际API响应结构调整数据解析
    let responseData = [];
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
    } else if (res.$values) {
      // 数据结构: res.$values
      responseData = res.$values;
      params.value.total = res.$values.length;
    } else {
      // 兜底
      responseData = [];
      params.value.total = 0;
      console.error('未知的API响应结构:', res);
    }
    
    // 数据转换 - 确保数据结构符合模板要求
    const processedData = responseData.map((item: any) => {
      console.log('原始文件项数据:', item);
      // 调试输出，查找文件名字段
      const keys = Object.keys(item);
      console.log('文件项字段列表:', keys);
      
      return {
        $id: item.$id || '',
        id: item.id || '',
        // 尝试多种可能的文件名字段名
        filename: item.filename || item.name || item.fileName || item.file_name || '',
        fileSize: item.fileSize || item.size || 0,
        contentType: item.contentType || item.type || item.mime_type || item.mimeType || '未知类型',
        hasActiveShare: item.hasActiveShare === true,
        uploadedAt: item.uploadedAt || item.uploadAt || item.created_at || item.createdAt || new Date().toISOString()
      };
    });
    
    // 保存处理后的完整数据，用于搜索过滤
    allFiles.value = [...processedData];
    
    // 表格数据根据搜索关键词决定显示全部还是过滤后的
    if (params.value.keyword && params.value.keyword.trim() !== '') {
      // 如果有搜索关键词，立即应用搜索过滤
      applySearchFilter(params.value.keyword.trim());
    } else {
      // 否则显示所有数据
      tableData.value = processedData;
    }
    
  } catch (error: any) {
    console.error('加载文件列表失败:', error);
    const errorMessage = error.response?.data?.detail || error.message || '加载文件列表失败';
    alertStore.showAlert(errorMessage, 'error');
    // 失败时设置空数据
    tableData.value = [];
    params.value.total = 0;
  }
}

// 前端搜索过滤函数
const applySearchFilter = (keyword: string) => {
  if (!keyword) {
    // 如果没有关键词，显示所有文件
    tableData.value = [...allFiles.value];
    params.value.total = allFiles.value.length;
    return;
  }
  
  const lowerCaseKeyword = keyword.toLowerCase();
  // 在前端过滤数据
  const filteredData = allFiles.value.filter((file: any) => {
    // 在多个字段中搜索关键词
    const filename = (file.filename || '').toLowerCase();
 /*   const id = (file.id || '').toLowerCase();
    const contentType = (file.contentType || '').toLowerCase();*/
    
    return filename.includes(lowerCaseKeyword);
          /* id.includes(lowerCaseKeyword) || 
           contentType.includes(lowerCaseKeyword);*/
  });
  
  console.log(`搜索结果: 从 ${allFiles.value.length} 项中过滤出 ${filteredData.length} 项`);
  
  // 更新表格数据和分页计数
  tableData.value = filteredData;
  params.value.total = filteredData.length;
}

// 页码改变处理函数
const handlePageChange = async (page: any) => {
  if (page < 1 || page > totalPages.value) return;
  params.value.page = page;
  
  // 如果是搜索状态，不需要重新加载数据
  if (params.value.keyword && params.value.keyword.trim() !== '') {
    console.log(`搜索状态切换到第 ${page} 页，前端分页`);
  } else {
    await loadFiles();
  }
}

// 添加搜索处理函数
const handleSearch = async () => {
  console.log('执行搜索，关键词:', params.value.keyword);
  params.value.page = 1; // 重置页码到第一页
  
  if (params.value.keyword && params.value.keyword.trim() !== '') {
    // 有搜索关键词时，在前端过滤
    applySearchFilter(params.value.keyword.trim());
  } else {
    // 无搜索关键词时，恢复显示所有文件
    tableData.value = [...allFiles.value];
    params.value.total = allFiles.value.length;
  }
}

// 计算总页数
const totalPages = computed(() => Math.ceil(params.value.total / params.value.size))

// 初始加载
loadFiles()

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

// 添加ID截断函数
const truncateId = (id: string) => {
  if (!id) return '';
  // 显示前8个字符
  return id.substring(0, 8) + '...';
}

// 添加点击ID显示完整内容的功能
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

// 保留删除文件处理函数
// 删除文件处理
const deleteFile = async (id: string) => {
  try {
    await api({
      url: `/api/admin/files/${id}`,
      method: 'delete'
    })
    await loadFiles()
    alertStore.showAlert('文件删除成功', 'success')
  } catch (error) {
    console.error('删除失败:', error)
    alertStore.showAlert('删除失败', 'error')
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


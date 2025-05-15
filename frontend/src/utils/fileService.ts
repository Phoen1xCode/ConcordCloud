import axios from 'axios';
import api from './api';

// 文件服务模块，封装与文件相关的API调用
const fileService = {
  // 获取用户文件列表
  async getFileList() {
    try {
      const response = await api.get('/api/File');
      return response;
    } catch (error) {
      console.error('获取文件列表失败:', error);
      throw error;
    }
  },

  // 上传文件
  async uploadFile(file: File, onProgressCallback?: (progress: number) => void) {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const config = {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (progressEvent: any) => {
          if (onProgressCallback && progressEvent.total) {
            const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            onProgressCallback(percentCompleted);
          }
        }
      };

      const response = await api.post('/api/File/upload', formData, config);
      
      // 确保返回的数据结构是SendFileView.vue期望的格式
      return {
        data: {
          id: response.data.id || response.data.fileId,
          fileName: response.data.fileName || file.name,
          success: true,
          file: response.data
        }
      };
    } catch (error) {
      console.error('文件上传失败:', error);
      throw error;
    }
  },

  // 下载文件
  async downloadFile(fileId: string, fileName: string) {
    try {
      // 使用原生 axios 直接获取二进制数据
      const response = await axios({
        url: `${api.defaults.baseURL}/api/File/download/${fileId}`,
        method: 'GET',
        responseType: 'blob',
      });

      // 创建 Blob URL 并模拟点击下载
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      return true;
    } catch (error) {
      console.error('文件下载失败:', error);
      throw error;
    }
  },

  // 删除文件
  async deleteFile(fileId: string) {
    try {
      const response = await api.delete(`/api/File/${fileId}`);
      return response;
    } catch (error) {
      console.error('文件删除失败:', error);
      throw error;
    }
  },

  // 重命名文件
  async renameFile(fileId: string, newFileName: string) {
    try {
      const response = await api.put('/api/File/rename', {
        fileId,
        newFileName
      });
      return response;
    } catch (error) {
      console.error('文件重命名失败:', error);
      throw error;
    }
  },

  // 创建文件分享
  async createFileShare(fileId: string, expirationDays: number = 7) {
    try {
      // Create a config object without withCredentials to maintain consistency
      const config = {
        headers: {
          'Content-Type': 'application/json'
        }
      };
      
      const response = await api.post('/api/File/share', {
        fileId,
        expirationDays
      }, config);
      
      // 确保返回的数据结构是SendFileView.vue期望的格式
      return {
        data: {
          shareCode: response.data.shareCode || response.data.code,
          success: true,
          share: response.data
        }
      };
    } catch (error) {
      console.error('创建分享失败:', error);
      throw error;
    }
  },

  // 通过分享码下载文件
  async downloadSharedFile(shareCode: string) {
    try {
      // 使用原生 axios 直接获取二进制数据
      const response = await axios({
        url: `${api.defaults.baseURL}/api/File/shared/${shareCode}`,
        method: 'GET',
        responseType: 'blob'
      });

      // 从响应头获取文件名
      const contentDisposition = response.headers['content-disposition'];
      let fileName = 'downloaded-file';
      
      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
        if (fileNameMatch && fileNameMatch[1]) {
          fileName = fileNameMatch[1].replace(/['"]/g, '');
          // 解码文件名
          try {
            fileName = decodeURIComponent(fileName);
          } catch (e) {
            console.warn('无法解码文件名', e);
          }
        }
      }

      // 创建 Blob URL 并模拟点击下载
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      return {
        success: true,
        fileName
      };
    } catch (error) {
      console.error('分享文件下载失败:', error);
      throw error;
    }
  }
};

export default fileService; 
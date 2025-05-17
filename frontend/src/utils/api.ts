import axios from 'axios';
import type { AxiosInstance, AxiosResponse } from 'axios';
import router from '@/router'; // 引入你的路由实例

// 定义 API 响应类型
interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  code?: number;
  files?: any[];
  share?: {
    shareCode: string;
    fileName: string;
    expiresAt: string;
  };
}

// 从环境变量中获取 API 基础 URL
const baseURL = 
  import.meta.env.MODE === 'production'
    ? import.meta.env.VITE_API_BASE_URL_PROD
    : import.meta.env.VITE_API_BASE_URL_DEV;

// 确保 baseURL 是一个有效的字符串（避免 undefined）
const sanitizedBaseURL = typeof baseURL === 'string' ? baseURL : '';

// 创建 axios 实例
const api: AxiosInstance = axios.create({
  baseURL: sanitizedBaseURL,
  timeout: 30000, // 请求超时时间30秒
  withCredentials: true, // 关键: 启用携带 Cookie
});

// 请求拦截器
api.interceptors.request.use(
  (config) => {
    // 配置请求头 - 不需要手动添加认证信息，浏览器会自动携带Cookie
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 响应拦截器
api.interceptors.response.use(
  (response: AxiosResponse): any => {
    // 直接返回响应体，并确保类型正确
    return response.data;
  },
  (error) => {
    // 处理重定向错误（后端返回302/307时）
    if (error.code === 'ERR_BAD_REDIRECT') {
      console.error('检测到重定向，可能未登录');
      router.push('/login'); // 手动跳转到登录页
      return Promise.reject(error);
    }

    // 处理网络错误（如 CORS）
    if (error.message && error.message.includes('Network Error')) {
      console.error('网络错误，可能是CORS或服务器问题');
    }

    // 处理 HTTP 状态码错误
    if (error.response) {
      switch (error.response.status) {
        case 401:
          console.error('未授权，请重新登录');
          router.push('/login'); // 使用路由跳转到登录页
          break;
        case 403:
          console.error('禁止访问');
          break;
        case 404:
          console.error('资源不存在');
          break;
        default:
          console.error('请求失败:', error.response.data);
      }
    } else if (error.request) {
      console.error('未收到响应:', error.request);
    } else {
      console.error('请求配置错误:', error.message);
    }

    return Promise.reject(error);
  }
);

// 扩展 axios 实例类型
declare module 'axios' {
  interface AxiosInstance {
    get<T = any>(url: string, config?: any): Promise<ApiResponse<T>>;
    post<T = any>(url: string, data?: any, config?: any): Promise<ApiResponse<T>>;
    put<T = any>(url: string, data?: any, config?: any): Promise<ApiResponse<T>>;
    delete<T = any>(url: string, config?: any): Promise<ApiResponse<T>>;
  }
}

export default api;

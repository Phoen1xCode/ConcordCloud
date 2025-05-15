import { createRouter, createWebHashHistory } from 'vue-router'

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login' // 将根路径重定向到登录页面
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/manage/LoginView.vue')
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('@/views/manage/RegisterView.vue')
    },
    {
      path: '/retrieve',
      name: 'Retrieve',
      component: () => import('@/views/RetrievewFileView.vue')
    },
    {
      path: '/send',
      name: 'Send',
      component: () => import('@/views/SendFileView.vue')
    },
    {
      path: '/admin',
      name: 'Manage',
      component: () => import('@/layout/AdminLayout/AdminLayout.vue'),
      redirect: '/admin/dashboard',
      children: [
        {
          path: '/admin/dashboard',
          name: 'Dashboard',
          component: () => import('@/views/manage/DashboardView.vue')
        },
        {
          path: '/admin/files',
          name: 'FileManage',
          component: () => import('@/views/manage/FileManageView.vue')
        },
        {
          path: '/admin/settings',
          name: 'Settings',
          component: () => import('@/views/manage/SystemSettingsView.vue')
        }
      ]
    },
    
  ]
})


export default router

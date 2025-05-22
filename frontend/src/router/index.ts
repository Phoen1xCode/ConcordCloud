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
      path: '/admin-login',
      name: 'AdminLogin',
      component: () => import('@/views/manage/AdminLoginView.vue')
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
      meta: { requiresAdminAuth: true },
      children: [
        {
          path: '/admin/dashboard',
          name: 'Dashboard',
          component: () => import('@/views/manage/DashboardView.vue'),
          meta: { requiresAdminAuth: true }
        },
        {
          path: '/admin/files',
          name: 'FileManage',
          component: () => import('@/views/manage/FileManageView.vue'),
          meta: { requiresAdminAuth: true }
        },
        {
          path: '/admin/users',
          name: 'UserManage',
          component: () => import('@/views/manage/UserManageView.vue'),
          meta: { requiresAdminAuth: true }
        },
      ]
    },
    
  ]
})

// 路由守卫 - 检查用户和管理员权限
router.beforeEach((to, from, next) => {
  console.log(`路由跳转: 从 ${from.path} 到 ${to.path}`);
  
  // 检查管理员权限路由
  if (to.matched.some(record => record.meta.requiresAdminAuth)) {
    const isAdminLoggedIn = localStorage.getItem('isAdminLoggedIn') === 'true';
    
    if (!isAdminLoggedIn) {
      console.log('访问管理员页面需要管理员权限，重定向到管理员登录');
      next({ path: '/admin-login' });
      return;
    }
  }
  
  // 允许导航继续
  next();
});

export default router

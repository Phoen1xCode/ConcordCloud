import { createRouter, createWebHashHistory } from 'vue-router'

const router = createRouter({
// 为了解决 '类型“ImportMeta”上不存在属性“env”' 的问题，这里使用一个默认值 '/' 替代 import.meta.env.BASE_URL
// 你也可以根据实际情况修改这个默认值
history: createWebHashHistory('/'),
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
      component: () => import('@/views/RetrievewFileView.vue'),
    },
    {
      path: '/send',
      name: 'Send',
      component: () => import('@/views/SendFileView.vue'),
      meta: { requiresAuth: true }
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

// 辅助函数：获取当前身份验证状态
const getAuthStatus = () => {
  const isAdminLoggedIn = localStorage.getItem('isAdminLoggedIn') === 'true';
  const isUserLoggedIn = localStorage.getItem('isUserLoggedIn') === 'true';
  const isLoggedIn = localStorage.getItem('isLoggedIn') === 'true';
  return { isAdminLoggedIn, isUserLoggedIn, isLoggedIn };
}

// 清除管理员登录状态
const clearAdminAuth = () => {
  localStorage.removeItem('isAdminLoggedIn');
  localStorage.removeItem('adminToken');
}

// 清除用户登录状态
const clearUserAuth = () => {
  localStorage.removeItem('isUserLoggedIn');
  localStorage.removeItem('isLoggedIn');
  localStorage.removeItem('token');
  localStorage.removeItem('userData');
}

// 路由守卫 - 检查用户和管理员权限
router.beforeEach((to, from, next) => {
  const { isAdminLoggedIn, isUserLoggedIn, isLoggedIn } = getAuthStatus();
  console.log(`路由跳转: 从 ${from.path} 到 ${to.path}`);
  console.log(`认证状态: 管理员=${isAdminLoggedIn}, 用户=${isUserLoggedIn}, 通用=${isLoggedIn}`);
  
  // 检查管理员权限路由
  if (to.matched.some(record => record.meta.requiresAdminAuth)) {
    // 如果是直接访问或刷新管理员页面，强制重新验证登录状态
    if (from.path === '/' || !from.name) {
      console.log('直接访问管理员页面，强制重新验证登录状态');
      clearAdminAuth();
      next({ path: '/admin-login' });
      return;
    }
    
    if (!isAdminLoggedIn) {
      console.log('访问管理员页面需要管理员权限，重定向到管理员登录');
      next({ path: '/admin-login' });
      return;
    }
  }
  
  // 检查普通用户权限路由
  if (to.matched.some(record => record.meta.requiresAuth)) {
    // 如果是直接访问或刷新用户页面，强制重新验证登录状态
    if (from.path === '/' || !from.name) {
      console.log('直接访问用户页面，强制重新验证登录状态');
      clearUserAuth();
      next({ path: '/login' });
      return;
    }
    
    if (!isUserLoggedIn) {
      console.log('访问用户页面需要登录权限，重定向到用户登录');
      next({ path: '/login' });
      return;
    }
  }
  
  // 允许导航继续
  next();
});

export default router

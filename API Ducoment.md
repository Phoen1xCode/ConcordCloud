# ConcordCloud 前端开发指南

## 概述

ConcordCloud是一个轻量级文件上传、下载和共享的Web解决方案。本文档为前端开发人员提供与后端API交互的详细指南。

后端基于ASP.NET Core开发，采用RESTful API风格，使用Cookie认证机制。API基础路径为`/api`。

## 服务器信息

启动服务器
```shell
cd backend
dotnet run --project API
```



- 开发环境API地址: `http://localhost:5000`
- Swagger文档: `http://localhost:5000/swagger`

## 认证机制

系统使用基于Cookie的认证机制，流程如下：

1. 用户通过`/api/User/login`接口登录
2. 服务器验证凭据后设置名为`ConcordCloud.Auth`的认证Cookie
3. 浏览器自动在后续请求中携带此Cookie
4. 用户可通过`/api/User/logout`接口登出，清除认证Cookie

**注意**：前端无需手动管理认证令牌，浏览器会自动处理Cookie。

## API接口文档

### 用户管理

#### 注册用户

- **URL**: `/api/User/register`
- **方法**: `POST`
- **认证要求**: 无
- **请求体**:
  ```json
  {
    "email": "user@example.com",
    "password": "YourPassword123!",
    "confirmPassword": "YourPassword123!"
  }
  ```
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "注册成功",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "role": "User",
      "createdAt": "2025-04-24T10:30:00Z"
    }
  }
  ```

#### 用户登录

- **URL**: `/api/User/login`
- **方法**: `POST`
- **认证要求**: 无
- **请求体**:
  ```json
  {
    "email": "user@example.com",
    "password": "YourPassword123!"
  }
  ```
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "登录成功",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "role": "User",
      "createdAt": "2025-04-24T10:30:00Z"
    }
  }
  ```
- **说明**: 登录成功后，服务器会自动设置认证Cookie

#### 用户登出

- **URL**: `/api/User/logout`
- **方法**: `POST`
- **认证要求**: 需要认证
- **请求体**: 无
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "已成功退出登录"
  }
  ```

#### 获取用户资料

- **URL**: `/api/User/profile`
- **方法**: `GET`
- **认证要求**: 需要认证
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "role": "User",
      "createdAt": "2025-04-24T10:30:00Z"
    }
  }
  ```

### 文件管理

#### 获取用户文件列表

- **URL**: `/api/File`
- **方法**: `GET`
- **认证要求**: 需要认证
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "files": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fileName": "document.pdf",
        "contentType": "application/pdf",
        "fileSize": 1048576,
        "uploadedAt": "2025-04-24T10:30:00Z",
        "hasActiveShare": false
      }
    ]
  }
  ```

#### 上传文件

- **URL**: `/api/File/upload`
- **方法**: `POST`
- **认证要求**: 需要认证
- **请求格式**: `multipart/form-data`
- **参数**:
  - `file`: 文件数据
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "文件上传成功",
    "file": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "document.pdf",
      "contentType": "application/pdf",
      "fileSize": 1048576,
      "uploadedAt": "2025-04-24T10:30:00Z",
      "hasActiveShare": false
    }
  }
  ```

#### 下载文件

- **URL**: `/api/File/download/{id}`
- **方法**: `GET`
- **认证要求**: 需要认证
- **参数**:
  - `id`: 文件ID (GUID)
- **成功响应**: 文件内容（二进制流）
- **说明**: 响应头包含`Content-Disposition`和`Content-Type`

#### 删除文件

- **URL**: `/api/File/{id}`
- **方法**: `DELETE`
- **认证要求**: 需要认证
- **参数**:
  - `id`: 文件ID (GUID)
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "文件删除成功"
  }
  ```

#### 重命名文件

- **URL**: `/api/File/rename`
- **方法**: `PUT`
- **认证要求**: 需要认证
- **请求体**:
  ```json
  {
    "fileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "newFileName": "新文件名.pdf"
  }
  ```
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "文件重命名成功",
    "file": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "新文件名.pdf",
      "contentType": "application/pdf",
      "fileSize": 1048576,
      "uploadedAt": "2025-04-24T10:30:00Z",
      "hasActiveShare": false
    }
  }
  ```

#### 创建文件分享

- **URL**: `/api/File/share`
- **方法**: `POST`
- **认证要求**: 需要认证
- **请求体**:
  ```json
  {
    "fileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "expirationDays": 7
  }
  ```
- **成功响应** (200 OK):
  ```json
  {
    "success": true,
    "message": "创建分享成功",
    "share": {
      "shareCode": "abc123def456",
      "expiresAt": "2025-05-01T10:30:00Z",
      "fileName": "document.pdf"
    }
  }
  ```

#### 通过分享码下载文件

- **URL**: `/api/File/shared/{shareCode}`
- **方法**: `GET`
- **认证要求**: 无
- **参数**:
  - `shareCode`: 分享码
- **成功响应**: 文件内容（二进制流）
- **说明**: 响应头包含`Content-Disposition`和`Content-Type`

## 数据模型

### 用户相关

```typescript
// 用户注册请求
interface UserRegisterDto {
  email: string;
  password: string;
  confirmPassword: string;
}

// 用户登录请求
interface UserLoginDto {
  email: string;
  password: string;
}

// 用户信息
interface UserDto {
  id: string; // GUID格式
  email: string;
  role: string; // "User" 或 "Admin"
  createdAt: string; // ISO 8601日期格式
}
```

### 文件相关

```typescript
// 文件信息
interface FileDto {
  id: string; // GUID格式
  fileName: string;
  contentType: string;
  fileSize: number; // 字节数
  uploadedAt: string; // ISO 8601日期格式
  hasActiveShare: boolean;
}

// 文件重命名请求
interface FileRenameDto {
  fileId: string; // GUID格式
  newFileName: string;
}

// 文件分享请求
interface FileShareDto {
  fileId: string; // GUID格式
  expirationDays: number; // 默认为7
}

// 文件分享结果
interface FileShareResultDto {
  shareCode: string;
  expiresAt: string; // ISO 8601日期格式
  fileName: string;
}
```

## 错误处理

所有API在发生错误时会返回适当的HTTP状态码和JSON格式的错误信息：

```json
{
  "success": false,
  "message": "错误描述信息"
}
```

常见错误状态码：
- `400 Bad Request`: 请求参数错误
- `401 Unauthorized`: 未认证或认证失败
- `403 Forbidden`: 权限不足
- `404 Not Found`: 资源不存在
- `500 Internal Server Error`: 服务器内部错误

## 示例代码

### 用户注册

```javascript
// 使用Fetch API
async function registerUser(email, password, confirmPassword) {
  try {
    const response = await fetch('http://localhost:5000/api/User/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email,
        password,
        confirmPassword
      })
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || '注册失败');
    }
    
    return data;
  } catch (error) {
    console.error('注册错误:', error);
    throw error;
  }
}
```

### 用户登录

```javascript
// 使用Fetch API
async function loginUser(email, password) {
  try {
    const response = await fetch('http://localhost:5000/api/User/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email,
        password
      }),
      credentials: 'include' // 重要：允许发送和接收Cookie
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || '登录失败');
    }
    
    return data;
  } catch (error) {
    console.error('登录错误:', error);
    throw error;
  }
}
```

### 文件上传

```javascript
// 使用FormData上传文件
async function uploadFile(file) {
  try {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch('http://localhost:5000/api/File/upload', {
      method: 'POST',
      body: formData,
      credentials: 'include' // 重要：包含认证Cookie
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || '上传失败');
    }
    
    return data;
  } catch (error) {
    console.error('上传错误:', error);
    throw error;
  }
}
```

### 获取文件列表

```javascript
// 使用Fetch API
async function getUserFiles() {
  try {
    const response = await fetch('http://localhost:5000/api/File', {
      method: 'GET',
      credentials: 'include' // 重要：包含认证Cookie
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || '获取文件列表失败');
    }
    
    return data.files;
  } catch (error) {
    console.error('获取文件列表错误:', error);
    throw error;
  }
}
```

## 注意事项

1. 所有需要认证的请求必须包含`credentials: 'include'`选项，以确保Cookie被正确发送
2. 文件上传大小限制为100MB
3. 确保在开发环境中启用CORS，以允许前端应用与API服务器通信
4. 使用Swagger UI (`http://localhost:5000/swagger`)可以交互式地测试所有API端点

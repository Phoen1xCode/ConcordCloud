# ConcordCloud Backend

## 后端架构
```
ConcordCloud.backend/
├── API/                           # Web API层 - 处理HTTP请求和响应
│   ├── Controllers/               # API控制器
│   └── Program.cs                 # 应用程序入口和配置
│
├── Core/                          # 业务核心层 - 包含业务逻辑和领域模型
│   ├── Entities/                  # 领域实体模型
│   ├── Interfaces/                # 服务接口定义
│   ├── DTOs/                      # 数据传输对象
│   └── Services/                  # 业务逻辑服务实现
│
└── Infrastructure/                # 基础设施层 - 提供技术实现
    ├── Database/                  # 数据访问
    │   └── AppDbContext.cs        # EF Core数据库上下文
    ├── Migrations/                # 数据库迁移
    └── FileStorage/               # 文件存储实现
        ├── LocalFileStorage.cs    # 本地文件存储

```

## 核心功能

### 功能介绍

1. 用户注册和登录
- 用户可通过填写邮箱、设置密码完成注册流程
- 用户可通过登录页面输入邮箱和密码进行登录
2. 文件上传和下载
- 用户可通过点击 “上传文件” 按钮，选择本地文件进行上传
- 上传文件支持多种常见格式，包括但不限于 `PDF`, `JPEG`, `PNG`, `MP4` 等
- 对于已上传到平台的文件，用户可在文件列表中找到对应文件，点击 “下载” 按钮即可将文件下载到本地设备
3. 文件分享
- 用户选择需要分享的文件，设置文件分享有效期，点击 “生成分享密码” 按钮，即可为该文件生成唯一的分享密码和分享连接
- 用户和访客可通过输入分享密码直接下载文件，无需额外进行注册登录
1. 文件管理
- 用户登录后，可在个人文件页面查看已上传的所有文件列表，列表展示文件名称、上传时间、文件大小等信息
- 支持对文件进行重命名操作，右键点击文件选择 “重命名”，输入新名称即可；对于不再需要的文件，可选择文件后点击 “删除” 按钮进行删除
- 文件可按类型（如将所有 PDF 文件归为一类、图片归为一类）或上传时间（按最新上传到最早上传排序）进行分类展示，方便用户查找和管理文件
1. 用户权限控制
- 平台设置不同用户角色：访客，用户和管理员
- 管理员拥有最高权限，可管理平台上所有文件，包括查看、编辑、删除任何用户上传的文件
- 用户仅能管理自己上传的文件，如查看、重命名、删除自己的文件，无法对其他用户文件进行操作
- 访客可以访问官网界面，通过输入分享密码直接下载文件

### API接口

#### 用户管理

- `POST /api/User/register` - 用户注册
- `POST /api/User/login` - 用户登录
- `POST /api/User/logout` - 用户登出
- `GET /api/User/profile` - 获取用户个人资料

#### 文件管理

- `GET /api/File` - 获取用户文件列表
- `POST /api/File/upload` - 上传文件
- `DELETE /api/File/{id}` - 删除文件
- `PUT /api/File/rename` - 重命名文件
- `GET /api/File/download/{id}` - 下载文件

#### 文件分享

- `POST /api/File/share` - 创建文件分享
- `GET /api/File/shared/{shareCode}` - 通过分享码下载文件


### 业务逻辑

#### 用户服务

- 用户注册：验证邮箱唯一性，密码哈希处理
- 用户登录：验证密码，更新最后登录时间
- 用户信息获取：根据ID获取用户信息
- 权限检查：验证用户是否为管理员

#### 文件服务

- 文件上传：保存文件到存储系统，创建文件记录
- 文件下载：验证权限，获取文件流
- 文件删除：验证权限，删除物理文件和数据库记录
- 文件重命名：验证权限，更新文件名
- 文件分享：生成唯一分享码，设置过期时间
- 分享下载：验证分享码有效性，获取文件流

#### 存储服务

- 本地文件存储：实现了文件的保存、获取和删除

## 开发计划

1. 增强文件安全机制
   - 实现文件内容类型验证和安全扫描
   - 增加文件访问审计日志
2. 优化文件上传下载性能
   - 实现大文件分片上传
   - 添加文件下载断点续传支持
3. 增加用户管理后台
   - 实现管理员用户管理界面
   - 添加用户统计和活动监控
4. 完善测试覆盖
   - 编写单元测试和集成测试
   - 设置CI/CD流程


## 开发细节

### 创建数据库表
```
dotnet tool install --global dotnet-ef

// 在 Infrastructure 项目目录下运行
dotnet ef migrations add InitialCreate --startup-project ../ConcordCloud.API/ConcordCloud.API.csproj

dotnet ef database update --startup-project ../ConcordCloud.API/ConcordCloud.API.csproj
```

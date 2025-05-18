using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using ConcordCloud.Infrastructure.Database;
using ConcordCloud.Infrastructure.FileStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BCrypt.Net;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.MaxDepth = 64;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConcordCloud API", Version = "v1" });
});

// 注册数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// 注册IAppDbContext接口
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// 注册服务
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorage>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "ConcordCloud.Auth";
        options.LoginPath = "/api/user/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });


// 配置 CORS
builder.Services.AddCors(options =>
{
    // 主策略：带凭证支持的跨域访问
    options.AddPolicy("AllowWithCredentials", policyBuilder =>
    {
        var origins = new List<string>();
        
        if (builder.Environment.IsDevelopment())
        {
            // 开发环境允许典型前端开发地址
            origins.AddRange(new[]
            {
                "http://localhost:5173",
                "https://localhost:5173",
                "http://127.0.0.1:5173"
            });
        }
        else
        {
            // 生产环境从配置读取
            var configOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (configOrigins == null || configOrigins.Length == 0)
            {
                throw new InvalidOperationException("CORS AllowedOrigins must be configured in production");
            }
            origins.AddRange(configOrigins);
        }

        policyBuilder.WithOrigins(origins.ToArray())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });

    // 管理员专用策略
    options.AddPolicy("AdminOnly", policyBuilder =>
    {
        var adminDomain = builder.Configuration.GetSection("Admin:Domain").Get<string>();
        if (string.IsNullOrEmpty(adminDomain))
        {
            adminDomain = "http://localhost:5173";
        }
        policyBuilder.WithOrigins(adminDomain)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


// 添加Web服务器配置
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // HTTP端口
    // serverOptions.ListenAnyIP(5001, listenOptions =>
    // {
    //     listenOptions.UseHttps(); // HTTPS端口
    // });
});

var app = builder.Build();

// 验证 CORS 配置 (生产环境)
if (!app.Environment.IsDevelopment())
{
    var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (allowedOrigins == null || allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
    {
         throw new InvalidOperationException("CORS AllowedOrigins are not configured correctly for production environment. Must specify allowed origins in appsettings or environment variables (Cors:AllowedOrigins). Do not use '*' in production.");
    }
    
    // 验证管理员域名配置
    var adminDomain = app.Configuration.GetSection("Admin:Domain").Get<string>();
    if (string.IsNullOrEmpty(adminDomain))
    {
        throw new InvalidOperationException("Admin domain is not configured correctly for production environment. Must specify Admin:Domain in appsettings or environment variables.");
    }
}

// 创建数据库和表（如果不存在）
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    
    // 检查是否需要初始化管理员账号
    var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
    var adminEmail = app.Configuration.GetSection("Admin:Email").Get<string>() ?? "admin@concordcloud.com";
    var adminPassword = app.Configuration.GetSection("Admin:DefaultPassword").Get<string>() ?? "Admin@123456";
    
    // 如果不存在管理员账号，则创建
    if (!dbContext.Admins.Any())
    {
        var result = adminService.InitializeDefaultAdminAsync(new ConcordCloud.Core.DTOs.AdminCreateDto
        {
            Email = adminEmail,
            Password = adminPassword,
            ConfirmPassword = adminPassword
        }).GetAwaiter().GetResult();
        
        if (!result.Success)
        {
            app.Logger.LogWarning("Failed to initialize default admin account: {Message}", result.Message);
        }
        else
        {
            app.Logger.LogInformation("Default admin account created successfully {Email} {Password}", adminEmail, adminPassword);
        }
    }
    else
    {
        var admins = dbContext.Admins.ToList();
        var passwordFixed = false;
        
        foreach (var admin in admins)
        {
            try
            {
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                passwordFixed = true;
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "无法修复管理员 {Email} 的密码哈希", admin.Email);
            }
        }
        
        if (passwordFixed)
        {
            dbContext.SaveChangesAsync().GetAwaiter().GetResult();
            app.Logger.LogInformation("已重置管理员密码为默认密码: {Password}", adminPassword);
        }
    }
}

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment() || true) // 确保Swagger始终可用
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConcordCloud API V1");
        c.RoutePrefix = "swagger";
    });
}

// app.UseHttpsRedirection();

// 应用全局 CORS 策略
app.UseCors("AllowWithCredentials");

// 管理员路由的域名限制中间件
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/admin"))
    {
        var adminDomain = app.Configuration.GetSection("Admin:Domain").Get<string>() ?? "admin.concordcloud.com";
        var host = context.Request.Host.Host;
        
        if (app.Environment.IsDevelopment() || host.Equals(adminDomain, StringComparison.OrdinalIgnoreCase))
        {
            await next();
        }
        else
        {
            context.Response.StatusCode = 404;
            return;
        }
    }
    else
    {
        await next();
    }
});

// 对管理员路由应用特定的CORS策略
app.UseCors(policyBuilder =>
{
    policyBuilder.SetIsOriginAllowed(origin =>
    {
        var adminDomain = app.Configuration.GetSection("Admin:Domain").Get<string>() ?? "admin.concordcloud.com";
        if (app.Environment.IsDevelopment())
        {
            return true; // 开发环境允许所有源
        }
        return origin.Equals($"https://{adminDomain}", StringComparison.OrdinalIgnoreCase);
    })
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Content-Disposition")
    .AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run(); 

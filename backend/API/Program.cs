using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using ConcordCloud.Infrastructure.Database;
using ConcordCloud.Infrastructure.FileStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConcordCloud API", Version = "v1" });
});

// 注册数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=concordcloud.db"));

// 注册IAppDbContext接口
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// 注册服务
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorage>();

// 添加简单的Cookie认证
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "ConcordCloud.Auth";
        options.LoginPath = "/api/User/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
         options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true; // 防止XSS攻击
    });

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (builder.Environment.IsDevelopment() || (allowedOrigins != null && allowedOrigins.Contains("*")))
        {
            // 开发环境或配置允许所有来源
            policyBuilder.SetIsOriginAllowed(_ => true) // 允许任何来源
                         .AllowCredentials(); // 允许凭证
        }
        else if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            // 生产环境，使用配置的来源
            policyBuilder.WithOrigins(allowedOrigins)
                         .AllowCredentials();
        }

        policyBuilder.AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

// 添加Web服务器配置
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // HTTP端口
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS端口
    });
});

var app = builder.Build();

// 不再需要验证JWT配置
// 验证 CORS 配置 (生产环境)
if (!app.Environment.IsDevelopment())
{
    var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (allowedOrigins == null || allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
    {
         // 在生产环境中，如果未配置具体的允许来源 (且不是'*')，则抛出异常或记录严重警告
         // 这里选择抛出异常强制要求配置
         throw new InvalidOperationException("CORS AllowedOrigins are not configured correctly for production environment. Must specify allowed origins in appsettings or environment variables (Cors:AllowedOrigins). Do not use '*' in production.");
    }
}

// 创建数据库和表（如果不存在）
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
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

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run(); 
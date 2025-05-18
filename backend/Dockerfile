FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY *.sln .
COPY Core/*.csproj ./Core/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY API/*.csproj ./API/


RUN dotnet restore


COPY . .

# 发布应用
WORKDIR /src/API
RUN dotnet publish -c Release -o /app/publish

# 构建运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# 创建文件存储目录
RUN mkdir -p /app/FileStorage && chmod 777 /app/FileStorage

# 暴露端口
EXPOSE 5000
EXPOSE 5001

ENV ASPNETCORE_URLS="http://+:5000;https://+:5001"
ENV ASPNETCORE_ENVIRONMENT=Production

# 启动应用
ENTRYPOINT ["dotnet", "ConcordCloud.API.dll"] 
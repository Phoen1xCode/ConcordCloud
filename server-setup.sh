#!/bin/bash

# 更新系统
apt-get update
apt-get upgrade -y

# 安装必要工具
apt-get install -y git curl nano

# 安装Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# 安装Docker Compose
curl -L "https://github.com/docker/compose/releases/download/v2.23.3/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# 安装Nginx
apt-get install -y nginx

# 安装Certbot（用于SSL证书）
apt-get install -y certbot python3-certbot-nginx

# 创建项目目录
mkdir -p /opt/concordcloud
cd /opt/concordcloud

# 克隆项目（请替换为实际的仓库URL）
git clone https://github.com/yourusername/ConcordCloud.git .

# 创建SSL目录
mkdir -p ssl

# 配置Nginx作为反向代理
cp nginx-proxy.conf /etc/nginx/sites-available/concordcloud
ln -s /etc/nginx/sites-available/concordcloud /etc/nginx/sites-enabled/
rm /etc/nginx/sites-enabled/default

# 获取SSL证书（请替换为实际域名）
certbot --nginx -d yourdomain.com -d www.yourdomain.com

# 启动应用
docker-compose up -d

# 设置防火墙
apt-get install -y ufw
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
echo "y" | ufw enable

echo "服务器初始化完成！" 
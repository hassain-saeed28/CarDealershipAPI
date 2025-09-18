# 🐳 Docker Deployment Guide

## 🚀 Quick Start

### Option 1: Automated Setup (Recommended)

```bash
# Linux/Mac
chmod +x docker-setup.sh
./docker-setup.sh

# Windows
docker-setup.bat
```

### Option 2: Manual Docker Compose

```bash
# Start services
docker-compose up --build -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Option 3: Docker Only

```bash
# Build image
docker build -t cardealership-api .

# Run container
docker run -d --name cardealership-api -p 8080:8080 cardealership-api

# Check logs
docker logs -f cardealership-api
```

## 📊 Access Points

| Service            | URL                                   | Description               |
| ------------------ | ------------------------------------- | ------------------------- |
| API                | http://localhost:8080                 | Main API endpoint         |
| Swagger            | http://localhost:8080                 | Interactive documentation |
| Health Check       | http://localhost:8080/api/test/health | Health status             |
| Nginx (production) | http://localhost:80                   | Reverse proxy             |

## 🔧 Environment Variables

Copy `.env.example` to `.env` and customize:

```bash
cp .env.example .env
# Edit .env file with your settings
```

## 🔍 Useful Commands

```bash
# Health check
curl http://localhost:8080/api/test/health

# View container logs
docker logs -f cardealership-api

# Enter container
docker exec -it cardealership-api /bin/bash

# Check container status
docker ps

# Clean up everything
docker-compose down -v
docker system prune -f
```

## 🏥 Troubleshooting

### Port Already in Use

```bash
# Linux/Mac
lsof -i :8080
sudo kill -9 <PID>

# Windows
netstat -ano | findstr :8080
taskkill /PID <PID> /F
```

### Database Issues

```bash
# Reset database
docker-compose down -v
docker-compose up --build
```

### Container Won't Start

```bash
# Check logs
docker logs cardealership-api

# Rebuild without cache
docker-compose build --no-cache
docker-compose up
```

## 🚀 Production Deployment

### With SSL (using Let's Encrypt)

```bash
# Enable production profile
docker-compose --profile production up -d

# Add SSL certificates to ./ssl/ folder
# Update nginx.conf to enable HTTPS
```

### Environment Variables for Production

```bash
export JWT_KEY="your-production-secret-key-32-chars"
export ASPNETCORE_ENVIRONMENT="Production"
docker-compose up -d
```

## 📈 Scaling

```bash
# Scale API instances
docker-compose up --scale cardealership-api=3 -d
```

For more detailed information, see the main README.md file.

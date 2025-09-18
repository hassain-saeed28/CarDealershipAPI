@echo off
echo 🐳 Car Dealership API - Docker Setup
echo =====================================

docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not installed. Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)

echo ✅ Docker is installed

echo 🔍 Checking required ports...
netstat -an | find "8080" | find "LISTENING" >nul
if %errorlevel% equ 0 (
    echo ❌ Port 8080 is already in use. Please stop the service using it.
    echo 💡 You can find what's using the port with: netstat -ano | findstr :8080
    pause
    exit /b 1
)

echo 📁 Creating required directories...
if not exist ssl mkdir ssl
if not exist data mkdir data
if not exist logs mkdir logs

if not exist .env (
    echo 🔑 Generating environment configuration...
    echo COMPOSE_PROJECT_NAME=cardealership > .env
    echo ASPNETCORE_ENVIRONMENT=Production >> .env
    echo JWT_KEY=your-secure-jwt-key-for-production-32chars >> .env
    echo JWT_ISSUER=CarDealershipAPI >> .env
    echo JWT_AUDIENCE=CarDealershipAPI >> .env
    echo API_PORT=8080 >> .env
    echo NGINX_PORT=80 >> .env
    echo ✅ Created .env file
) else (
    echo ✅ Using existing .env file
)

echo 🔨 Building and starting services...
docker-compose build
docker-compose up -d

echo ⏳ Waiting for services to start...
timeout /t 10 /nobreak >nul

echo 🏥 Checking service health...
for /L %%i in (1,1,15) do (
    curl -f -s http://localhost:8080/api/test/health >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✅ API is healthy!
        goto :health_ok
    )
    echo ⏳ Waiting for API to be ready... (%%i/15)
    timeout /t 2 /nobreak >nul
)

echo ❌ API failed to start properly. Check logs with: docker-compose logs
pause
exit /b 1

:health_ok
echo.
echo 🎉 Setup completed successfully!
echo.
echo 📋 Service Information:
echo   🌐 API URL:        http://localhost:8080
echo   📚 Swagger UI:     http://localhost:8080
echo   🏥 Health Check:   http://localhost:8080/api/test/health
echo.
echo 👥 Test Accounts:
echo   👨‍💼 Admin:    admin@cardealership.com / Admin123!
echo   👤 Customer: john.doe@email.com / Customer123!
echo.
echo 🔧 Useful Commands:
echo   📊 View logs:      docker-compose logs -f
echo   🔄 Restart:        docker-compose restart
echo   🛑 Stop:           docker-compose down
echo   🗑️  Clean up:       docker-compose down -v
echo.
echo 🚀 Ready to use! Open http://localhost:8080 in your browser.
pause
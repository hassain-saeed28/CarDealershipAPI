#!/bin/bash

echo "🐳 Car Dealership API - Docker Setup"
echo "====================================="

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker Desktop from https://www.docker.com/products/docker-desktop"
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null 2>&1; then
    echo "❌ Docker Compose is not available. Please install Docker Compose."
    exit 1
fi

echo "✅ Docker is installed"
echo "✅ Docker Compose is available"

# Function to check if port is available
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null; then
        echo "❌ Port $1 is already in use. Please stop the service using it or use a different port."
        echo "   To find what's using the port: lsof -i :$1"
        return 1
    fi
    return 0
}

# Check required ports
echo "🔍 Checking required ports..."
if ! check_port 8080; then
    echo "💡 You can kill the process using: lsof -ti:8080 | xargs kill -9"
    read -p "Do you want me to try to free port 8080? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        lsof -ti:8080 | xargs kill -9 2>/dev/null || true
        sleep 2
        if ! check_port 8080; then
            exit 1
        fi
    else
        exit 1
    fi
fi

# Create necessary directories
echo "📁 Creating required directories..."
mkdir -p ssl data logs

# Generate a secure JWT key for production
if [[ ! -f .env ]]; then
    echo "🔑 Generating environment configuration..."
    JWT_KEY=$(openssl rand -base64 32 | tr -d "=+/" | cut -c1-32)
    cat > .env << EOL
# Car Dealership API Environment Configuration
COMPOSE_PROJECT_NAME=cardealership
ASPNETCORE_ENVIRONMENT=Production
JWT_KEY=${JWT_KEY}
JWT_ISSUER=CarDealershipAPI
JWT_AUDIENCE=CarDealershipAPI
API_PORT=8080
NGINX_PORT=80
EOL
    echo "✅ Created .env file with secure configuration"
else
    echo "✅ Using existing .env file"
fi

# Build and start services
echo "🔨 Building and starting services..."
echo "This may take a few minutes on first run..."

if command -v docker-compose &> /dev/null; then
    COMPOSE_CMD="docker-compose"
else
    COMPOSE_CMD="docker compose"
fi

# Build the application
$COMPOSE_CMD build

# Start services
$COMPOSE_CMD up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to start..."
sleep 10

# Check health
echo "🏥 Checking service health..."
for i in {1..30}; do
    if curl -f -s http://localhost:8080/api/test/health > /dev/null; then
        echo "✅ API is healthy!"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ API failed to start properly. Check logs with: $COMPOSE_CMD logs"
        exit 1
    fi
    echo "⏳ Waiting for API to be ready... ($i/30)"
    sleep 2
done

# Display useful information
echo ""
echo "🎉 Setup completed successfully!"
echo ""
echo "📋 Service Information:"
echo "  🌐 API URL:        http://localhost:8080"
echo "  📚 Swagger UI:     http://localhost:8080"
echo "  🏥 Health Check:   http://localhost:8080/api/test/health"
echo ""
echo "👥 Test Accounts:"
echo "  👨‍💼 Admin:    admin@cardealership.com / Admin123!"
echo "  👤 Customer: john.doe@email.com / Customer123!"
echo ""
echo "🔧 Useful Commands:"
echo "  📊 View logs:      $COMPOSE_CMD logs -f"
echo "  🔄 Restart:        $COMPOSE_CMD restart"
echo "  🛑 Stop:           $COMPOSE_CMD down"
echo "  🗑️  Clean up:       $COMPOSE_CMD down -v"
echo ""
echo "🚀 Ready to use! Open http://localhost:8080 in your browser."
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}🚀 Starting Clean Architecture Application with Docker...${NC}"

# Check if Docker is running
echo -e "\n${YELLOW}📋 Checking Docker status...${NC}"
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker is running${NC}"

# Ask user which services to start
echo -e "\n${YELLOW}🔧 Select deployment mode:${NC}"
echo "1. Basic (API + PostgreSQL only)"
echo "2. With Observability (+ Seq, Loki, Grafana, OpenTelemetry)"
echo "3. With Kafka (+ Zookeeper, Kafka)"
echo "4. With Cache (+ Redis)"
echo "5. Full Stack (All services)"

read -p "Enter your choice (1-5): " choice

# Build the profiles parameter
profiles=""
case $choice in
    2) profiles="--profile observability" ;;
    3) profiles="--profile kafka" ;;
    4) profiles="--profile cache" ;;
    5) profiles="--profile observability --profile kafka --profile cache" ;;
esac

# Build and start containers
echo -e "\n${YELLOW}🏗️  Building Docker images...${NC}"
docker-compose build

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi

echo -e "\n${YELLOW}🚀 Starting containers...${NC}"
docker-compose $profiles up -d

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Failed to start containers!${NC}"
    exit 1
fi

# Wait for services to be healthy
echo -e "\n${YELLOW}⏳ Waiting for services to be healthy...${NC}"
sleep 5

# Check status
echo -e "\n${CYAN}📊 Container Status:${NC}"
docker-compose ps

# Display access information
echo -e "\n${GREEN}✅ Deployment complete!${NC}"
echo -e "\n${CYAN}🌐 Access your application at:${NC}"
echo "   API:              http://localhost:5000"
echo "   Health Check:     http://localhost:5000/health"
echo "   API Docs:         http://localhost:5000/scalar/v1"
echo "   PostgreSQL:       localhost:5432 (postgres/postgres)"

if [[ $profiles == *"observability"* ]]; then
    echo -e "\n${CYAN}📊 Observability Tools:${NC}"
    echo "   Seq:              http://localhost:5341"
    echo "   Grafana:          http://localhost:3000 (admin/admin)"
fi

if [[ $profiles == *"kafka"* ]]; then
    echo -e "\n${CYAN}📨 Messaging:${NC}"
    echo "   Kafka:            localhost:9092"
fi

if [[ $profiles == *"cache"* ]]; then
    echo -e "\n${CYAN}💾 Cache:${NC}"
    echo "   Redis:            localhost:6379"
fi

echo -e "\n${CYAN}📝 Useful commands:${NC}"
echo "   View logs:        docker-compose logs -f"
echo "   Stop services:    docker-compose down"
echo "   Restart API:      docker-compose restart webapi"

echo -e "\n${GREEN}✨ Happy coding!${NC}"

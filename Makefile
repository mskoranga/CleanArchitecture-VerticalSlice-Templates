.PHONY: help build up down restart logs ps clean test backup restore

# Default target
help:
    @echo "Clean Architecture Docker Commands"
    @echo ""
    @echo "Usage:"
    @echo "  make <target>"
    @echo ""
    @echo "Targets:"
    @echo "  build              Build Docker images"
    @echo "  up                 Start all services (basic)"
    @echo "  up-full            Start all services with observability, kafka, and cache"
    @echo "  up-obs             Start with observability stack"
    @echo "  up-kafka           Start with Kafka"
    @echo "  up-cache           Start with Redis cache"
    @echo "  down               Stop and remove containers"
    @echo "  down-volumes       Stop and remove containers and volumes (DELETE DATA)"
    @echo "  restart            Restart all services"
    @echo "  restart-api        Restart API only"
    @echo "  logs               View all logs"
    @echo "  logs-api           View API logs"
    @echo "  logs-db            View database logs"
    @echo "  ps                 List running containers"
    @echo "  clean              Remove containers, volumes, and images"
    @echo "  test               Run health check"
    @echo "  db-connect         Connect to PostgreSQL"
    @echo "  db-backup          Backup database"
    @echo "  db-restore         Restore database from backup.sql"

# Build images
build:
    docker-compose build

# Start services
up:
    docker-compose up -d

up-full:
    docker-compose --profile observability --profile kafka --profile cache up -d

up-obs:
    docker-compose --profile observability up -d

up-kafka:
    docker-compose --profile kafka up -d

up-cache:
    docker-compose --profile cache up -d

# Stop services
down:
    docker-compose down

down-volumes:
    docker-compose down -v

# Restart services
restart:
    docker-compose restart

restart-api:
    docker-compose restart webapi

# View logs
logs:
    docker-compose logs -f

logs-api:
    docker-compose logs -f webapi

logs-db:
    docker-compose logs -f postgres

# Status
ps:
    docker-compose ps

# Cleanup
clean:
    docker-compose down -v --rmi all --remove-orphans

# Test
test:
    @echo "Testing API health..."
    @curl -f http://localhost:5000/health || echo "API health check failed"

# Database operations
db-connect:
    docker exec -it cleanarch-postgres psql -U postgres -d Book

db-backup:
    @echo "Creating database backup..."
    docker exec cleanarch-postgres pg_dump -U postgres Book > backup.sql
    @echo "Backup saved to backup.sql"

db-restore:
    @echo "Restoring database from backup.sql..."
    docker exec -i cleanarch-postgres psql -U postgres Book < backup.sql
    @echo "Database restored"

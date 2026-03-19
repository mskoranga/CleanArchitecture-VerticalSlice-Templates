# Docker Deployment Guide

This guide explains how to deploy the Clean Architecture application with Docker and PostgreSQL.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose V2
- At least 4GB of available RAM

## Quick Start

### 1. Basic Deployment (API + PostgreSQL only)

```bash
# Build and start the application with PostgreSQL
docker-compose up -d

# View logs
docker-compose logs -f webapi
```

The API will be available at:
- HTTP: http://localhost:5000
- Health Check: http://localhost:5000/health
- API Documentation: http://localhost:5000/scalar/v1

PostgreSQL will be available at:
- Host: localhost
- Port: 5432
- Database: Book
- Username: postgres
- Password: postgres

### 2. With Observability Stack (Seq, Loki, Grafana, OpenTelemetry)

```bash
# Start with observability services
docker-compose --profile observability up -d

# Access observability tools
# Seq: http://localhost:5341
# Grafana: http://localhost:3000 (admin/admin)
# Loki: http://localhost:3100
```

### 3. With Kafka

```bash
# Start with Kafka
docker-compose --profile kafka up -d
```

### 4. With Redis Cache

```bash
# Start with Redis
docker-compose --profile cache up -d
```

### 5. Full Stack (All Services)

```bash
# Start all services
docker-compose --profile observability --profile kafka --profile cache up -d
```

## Docker Commands

### Build and Start
```bash
# Build the application image
docker-compose build

# Start services in detached mode
docker-compose up -d

# Start and rebuild
docker-compose up -d --build
```

### Monitoring
```bash
# View all running containers
docker-compose ps

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f webapi
docker-compose logs -f postgres

# Check health status
curl http://localhost:5000/health
```

### Database Operations
```bash
# Connect to PostgreSQL
docker exec -it cleanarch-postgres psql -U postgres -d Book

# Run database migrations (if using EF Core migrations)
docker-compose exec webapi dotnet ef database update

# Backup database
docker exec cleanarch-postgres pg_dump -U postgres Book > backup.sql

# Restore database
docker exec -i cleanarch-postgres psql -U postgres Book < backup.sql
```

### Cleanup
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: deletes data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Complete cleanup
docker-compose down -v --rmi all --remove-orphans
```

## Environment Variables

You can override settings using environment variables or a `.env` file:

```bash
# .env file example
POSTGRES_PASSWORD=your_secure_password
ASPNETCORE_ENVIRONMENT=Production
SEQ_API_KEY=your_seq_api_key
```

## Troubleshooting

### Container won't start
```bash
# Check logs
docker-compose logs webapi

# Check container status
docker-compose ps

# Restart specific service
docker-compose restart webapi
```

### Database connection issues
```bash
# Verify PostgreSQL is running
docker-compose ps postgres

# Check PostgreSQL logs
docker-compose logs postgres

# Test connection
docker exec cleanarch-postgres pg_isready -U postgres
```

### Port conflicts
If ports 5000, 5432, etc. are already in use, modify the port mappings in `docker-compose.yml`:

```yaml
ports:
  - "5050:8080"  # Change 5000 to 5050
```

## Network Architecture

All services run in a custom bridge network (`cleanarch-network`), allowing:
- Service discovery by container name
- Isolated communication
- Better security

## Volumes

Persistent data is stored in Docker volumes:
- `postgres_data` - PostgreSQL database
- `seq_data` - Seq logs
- `loki_data` - Loki logs
- `grafana_data` - Grafana dashboards
- `redis_data` - Redis cache

## Security Recommendations

For production deployments:

1. **Change default passwords** in `docker-compose.yml`
2. **Use secrets management** instead of environment variables
3. **Enable HTTPS** with proper certificates
4. **Restrict network access** using firewall rules
5. **Use specific image tags** instead of `latest`
6. **Run containers as non-root users** (already configured)
7. **Regularly update images** for security patches

## Performance Tuning

### PostgreSQL
```yaml
environment:
  - POSTGRES_SHARED_BUFFERS=256MB
  - POSTGRES_MAX_CONNECTIONS=200
```

### Application
```yaml
environment:
  - DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
  - DOTNET_gcServer=1
```

## Production Deployment

For production, consider:

1. **Use Docker Swarm or Kubernetes** for orchestration
2. **Set up load balancing** with nginx or Traefik
3. **Implement backup strategies** for databases
4. **Configure monitoring and alerting**
5. **Use managed database services** (Azure Database for PostgreSQL, AWS RDS)
6. **Implement CI/CD pipelines** for automated deployments

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)

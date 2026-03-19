# 🐳 Docker Deployment - Complete Setup Guide

## 📦 What Has Been Created

Your Clean Architecture application is now ready for Docker deployment with the following files:

### Core Docker Files
1. **Dockerfile** - Multi-stage build configuration for your .NET 10 application
2. **docker-compose.yml** - Complete orchestration with PostgreSQL and optional services
3. **.dockerignore** - Optimizes build by excluding unnecessary files
4. **appsettings.Docker.json** - Docker-specific configuration

### Configuration Files
5. **config/loki-config.yaml** - Grafana Loki log aggregation config
6. **config/otel-collector-config.yaml** - OpenTelemetry collector config
7. **scripts/init-db.sql** - PostgreSQL initialization script

### Helper Scripts
8. **scripts/start-docker.ps1** - Windows PowerShell script to start services
9. **scripts/stop-docker.ps1** - Windows PowerShell script to stop/cleanup
10. **scripts/start-docker.sh** - Linux/Mac bash script to start services

### Documentation
11. **DOCKER-README.md** - Comprehensive deployment guide
12. **DOCKER-QUICK-REFERENCE.md** - Quick command reference
13. **.env.template** - Environment variables template
14. **docker-compose.override.yml.example** - Local development overrides
15. **.gitignore.docker** - Git ignore entries for Docker files

## 🎯 Architecture Overview

### Services Included

#### Core Services (Always Running)
- **webapi** - Your .NET 10 Clean Architecture API
- **postgres** - PostgreSQL 17 database

#### Optional Services (Use Profiles)
- **seq** - Centralized logging (--profile observability)
- **loki** - Log aggregation (--profile observability)
- **grafana** - Visualization dashboard (--profile observability)
- **otel-collector** - OpenTelemetry metrics (--profile observability)
- **kafka** + **zookeeper** - Event streaming (--profile kafka)
- **redis** - Caching (--profile cache)

### Network Architecture
```
┌─────────────────────────────────────────────────┐
│         cleanarch-network (bridge)              │
│                                                 │
│  ┌─────────┐    ┌──────────┐    ┌──────────┐  │
│  │ webapi  │───▶│ postgres │    │   seq    │  │
│  │ :8080   │    │  :5432   │    │  :5341   │  │
│  └─────────┘    └──────────┘    └──────────┘  │
│       │                                         │
│       ├──────────▶ ┌──────────┐                │
│       │            │   loki   │                │
│       │            │  :3100   │                │
│       │            └──────────┘                │
│       │                                         │
│       └──────────▶ ┌──────────┐                │
│                    │  kafka   │                │
│                    │  :9092   │                │
│                    └──────────┘                │
└─────────────────────────────────────────────────┘
         │               │              │
    localhost:5000  localhost:5432  localhost:5341
```

## 🚀 Getting Started

### Prerequisites
1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac)
   OR
   Install Docker Engine (Linux)
2. Ensure Docker is running
3. At least 4GB available RAM

### Quick Start (3 Steps)

#### Option 1: Using Helper Scripts (Recommended)

**Windows:**
```powershell
# Navigate to project root
cd C:\Work\Learnings\Code\Clean-Architecture-with-Vertical-Slice-Template

# Run start script
.\scripts\start-docker.ps1
```

**Linux/Mac:**
```bash
# Navigate to project root
cd /path/to/Clean-Architecture-with-Vertical-Slice-Template

# Make script executable and run
chmod +x scripts/start-docker.sh
./scripts/start-docker.sh
```

#### Option 2: Manual Docker Compose

**Basic (API + PostgreSQL only):**
```bash
docker-compose up -d
```

**With Observability Tools:**
```bash
docker-compose --profile observability up -d
```

**Full Stack:**
```bash
docker-compose --profile observability --profile kafka --profile cache up -d
```

### Verify Deployment

1. **Check containers are running:**
   ```bash
   docker-compose ps
   ```

2. **Test API health:**
   ```bash
   curl http://localhost:5000/health
   ```

3. **Access API documentation:**
   Open http://localhost:5000/scalar/v1 in your browser

4. **Check logs:**
   ```bash
   docker-compose logs -f webapi
   ```

## 🔧 Configuration

### Environment Variables

The application uses these key settings (configured in docker-compose.yml):

```yaml
ConnectionStrings__connection: "Server=postgres;Port=5432;Database=Book;Username=postgres;Password=postgres"
ASPNETCORE_ENVIRONMENT: "Docker"
Serilog__Seq__ServerUrl: "http://seq:5341"
OpenTelemetry__OtlpEndpoint: "http://otel-collector:4317"
Kafka__BootstrapServers: "kafka:9092"
```

### Custom Configuration

Create a `.env` file from the template:
```bash
cp .env.template .env
# Edit .env with your values
```

## 📊 Accessing Services

| Service | URL | Default Credentials |
|---------|-----|---------------------|
| **API** | http://localhost:5000 | - |
| **API Docs (Scalar)** | http://localhost:5000/scalar/v1 | - |
| **Health Check** | http://localhost:5000/health | - |
| **PostgreSQL** | localhost:5432 | postgres / postgres |
| **Seq (Logs)** | http://localhost:5341 | - |
| **Grafana** | http://localhost:3000 | admin / admin |
| **Kafka** | localhost:9092 | - |
| **Redis** | localhost:6379 | - |

## 🗄️ Database Management

### Connect to Database
```bash
# Using psql in container
docker exec -it cleanarch-postgres psql -U postgres -d Book

# Using external tool (DBeaver, pgAdmin, etc.)
# Host: localhost
# Port: 5432
# Database: Book
# User: postgres
# Password: postgres
```

### Run Migrations
```bash
# If using Entity Framework Core migrations
docker-compose exec webapi dotnet ef database update
```

### Backup Database
```bash
# Create backup
docker exec cleanarch-postgres pg_dump -U postgres Book > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore from backup
docker exec -i cleanarch-postgres psql -U postgres Book < backup.sql
```

## 📈 Monitoring & Observability

### View Logs

**All services:**
```bash
docker-compose logs -f
```

**Specific service:**
```bash
docker-compose logs -f webapi
docker-compose logs -f postgres
```

**Last 100 lines:**
```bash
docker-compose logs --tail=100 webapi
```

### Seq (Structured Logs)
1. Start with observability profile
2. Open http://localhost:5341
3. View structured logs with filtering and search

### Grafana (Metrics & Dashboards)
1. Start with observability profile
2. Open http://localhost:3000 (admin/admin)
3. Add Loki data source: http://loki:3100
4. Create dashboards

## 🛑 Stopping & Cleanup

### Using Helper Script (Windows)
```powershell
.\scripts\stop-docker.ps1
# Follow prompts to choose cleanup level
```

### Manual Commands

**Stop containers (keep data):**
```bash
docker-compose stop
```

**Stop and remove containers (keep volumes):**
```bash
docker-compose down
```

**Remove everything including data (⚠️ DESTRUCTIVE):**
```bash
docker-compose down -v
```

**Complete cleanup including images:**
```bash
docker-compose down -v --rmi all --remove-orphans
```

## 🐛 Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs <service-name>

# Check status
docker-compose ps

# Restart service
docker-compose restart <service-name>

# Rebuild image
docker-compose build --no-cache <service-name>
```

### Database Connection Issues

```bash
# Verify PostgreSQL is running
docker-compose ps postgres

# Check PostgreSQL health
docker exec cleanarch-postgres pg_isready -U postgres

# View PostgreSQL logs
docker-compose logs postgres

# Test connection from API container
docker-compose exec webapi ping postgres
```

### Port Already in Use

Edit `docker-compose.yml` and change port mappings:
```yaml
services:
  webapi:
    ports:
      - "5050:8080"  # Changed from 5000 to 5050
```

### Out of Memory

Increase Docker Desktop memory:
1. Docker Desktop → Settings → Resources
2. Increase Memory to at least 4GB
3. Apply & Restart

## 🔐 Security Recommendations

### For Production Deployment:

1. **Change Default Passwords**
   ```yaml
   environment:
     POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}  # Use environment variable
   ```

2. **Use Secrets Management**
   - Docker Secrets (Swarm)
   - Kubernetes Secrets
   - Azure Key Vault / AWS Secrets Manager

3. **Enable HTTPS**
   - Add SSL certificates
   - Configure reverse proxy (nginx, Traefik)

4. **Use Specific Image Versions**
   ```yaml
   image: postgres:17.2-alpine  # Instead of :latest
   ```

5. **Implement Network Segmentation**
   - Separate networks for DB, cache, messaging
   - Use internal networks where possible

6. **Regular Updates**
   ```bash
   docker-compose pull  # Update images
   docker-compose up -d  # Recreate containers
   ```

## 📦 Volumes & Data Persistence

Data is persisted in Docker volumes:

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect clean-architecture-with-vertical-slice-template_postgres_data

# Backup volume
docker run --rm -v postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres-backup.tar.gz /data

# Restore volume
docker run --rm -v postgres_data:/data -v $(pwd):/backup alpine tar xzf /backup/postgres-backup.tar.gz -C /
```

## 🚀 Production Deployment Options

### Option 1: Docker Swarm
```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml cleanarch
```

### Option 2: Kubernetes
Convert compose file to K8s manifests:
```bash
kompose convert -f docker-compose.yml
```

### Option 3: Azure Container Instances
```bash
az container create \
  --resource-group myResourceGroup \
  --name cleanarch-api \
  --image myregistry.azurecr.io/cleanarch-api:latest
```

### Option 4: AWS ECS
Use AWS Copilot CLI:
```bash
copilot init
copilot deploy
```

## 🎓 Next Steps

1. **Customize Configuration**
   - Update `appsettings.Docker.json` with your settings
   - Modify database credentials
   - Configure external services (ForgeRock, Kafka, etc.)

2. **Set Up CI/CD**
   - Build Docker images in pipeline
   - Push to container registry
   - Deploy to production

3. **Add Database Migrations**
   - Create EF Core migrations
   - Run on container startup

4. **Configure Monitoring**
   - Set up Application Insights / Prometheus
   - Configure alerting
   - Create dashboards

5. **Implement Backup Strategy**
   - Automated database backups
   - Backup retention policy
   - Disaster recovery plan

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Official Image](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Best Practices for .NET in Docker](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/docker-application-development-process/docker-app-development-workflow)

## ✅ Checklist

- [ ] Docker Desktop installed and running
- [ ] All files created successfully
- [ ] Configuration reviewed and customized
- [ ] Containers built and started
- [ ] API health check passes
- [ ] Database connection verified
- [ ] Logs reviewed in Seq/Grafana (if using observability profile)
- [ ] Security settings reviewed
- [ ] Backup strategy planned

## 🆘 Getting Help

If you encounter issues:

1. Check the troubleshooting section above
2. Review logs: `docker-compose logs`
3. Verify configuration files
4. Check Docker Desktop resources
5. Ensure ports are not in use

---

**Happy Dockerizing! 🐳**

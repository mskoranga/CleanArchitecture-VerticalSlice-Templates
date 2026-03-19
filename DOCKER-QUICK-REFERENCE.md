# Docker Quick Reference

## 🚀 Quick Start

```bash
# Windows (PowerShell)
.\scripts\start-docker.ps1

# Linux/Mac
chmod +x scripts/start-docker.sh
./scripts/start-docker.sh

# Or manually
docker-compose up -d
```

## 📋 Common Commands

### Start/Stop
```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Restart a service
docker-compose restart webapi
```

### Logs
```bash
# All logs
docker-compose logs -f

# Specific service
docker-compose logs -f webapi
docker-compose logs -f postgres
```

### Status
```bash
# List containers
docker-compose ps

# Check health
curl http://localhost:5000/health
```

### Database
```bash
# Connect to PostgreSQL
docker exec -it cleanarch-postgres psql -U postgres -d Book

# Run SQL file
docker exec -i cleanarch-postgres psql -U postgres -d Book < script.sql

# Backup
docker exec cleanarch-postgres pg_dump -U postgres Book > backup.sql

# Restore
docker exec -i cleanarch-postgres psql -U postgres Book < backup.sql
```

### Cleanup
```bash
# Stop and remove containers (keep data)
docker-compose down

# Remove containers and volumes (DELETE DATA)
docker-compose down -v

# Complete cleanup
docker-compose down -v --rmi all
```

## 🔧 Profiles

```bash
# With observability
docker-compose --profile observability up -d

# With Kafka
docker-compose --profile kafka up -d

# With Redis
docker-compose --profile cache up -d

# All profiles
docker-compose --profile observability --profile kafka --profile cache up -d
```

## 🌐 Service URLs

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:5000 | - |
| Health Check | http://localhost:5000/health | - |
| API Docs | http://localhost:5000/scalar/v1 | - |
| PostgreSQL | localhost:5432 | postgres/postgres |
| Seq | http://localhost:5341 | - |
| Grafana | http://localhost:3000 | admin/admin |
| Kafka | localhost:9092 | - |
| Redis | localhost:6379 | - |

## 🐛 Troubleshooting

### Container won't start
```bash
docker-compose logs <service-name>
docker-compose ps
docker-compose restart <service-name>
```

### Port conflicts
Edit `docker-compose.yml` and change port mappings:
```yaml
ports:
  - "5050:8080"  # Changed from 5000
```

### Database connection issues
```bash
# Check PostgreSQL
docker exec cleanarch-postgres pg_isready -U postgres

# Verify network
docker network inspect clean-architecture-with-vertical-slice-template_cleanarch-network
```

### Reset everything
```bash
# Nuclear option - deletes everything
docker-compose down -v --rmi all --remove-orphans
docker system prune -a --volumes
```

## 🔐 Security Checklist

- [ ] Change default passwords in docker-compose.yml
- [ ] Use .env file for sensitive data
- [ ] Enable HTTPS in production
- [ ] Use specific image tags (not `latest`)
- [ ] Restrict network access
- [ ] Regular security updates
- [ ] Implement secrets management

## 📊 Monitoring

```bash
# Container stats
docker stats

# Disk usage
docker system df

# Network inspection
docker network ls
docker network inspect cleanarch-network
```

## 🎯 Production Tips

1. Use orchestration (Docker Swarm/Kubernetes)
2. Implement load balancing
3. Set up automated backups
4. Configure monitoring and alerting
5. Use managed database services
6. Implement CI/CD pipelines
7. Use health checks
8. Configure resource limits

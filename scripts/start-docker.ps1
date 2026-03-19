# PowerShell script to build and start Docker containers

Write-Host "🚀 Starting Clean Architecture Application with Docker..." -ForegroundColor Cyan

# Check if Docker is running
Write-Host "`n📋 Checking Docker status..." -ForegroundColor Yellow
$dockerStatus = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker is running" -ForegroundColor Green

# Ask user which services to start
Write-Host "`n🔧 Select deployment mode:" -ForegroundColor Yellow
Write-Host "1. Basic (API + PostgreSQL only)"
Write-Host "2. With Observability (+ Seq, Loki, Grafana, OpenTelemetry)"
Write-Host "3. With Kafka (+ Zookeeper, Kafka)"
Write-Host "4. With Cache (+ Redis)"
Write-Host "5. Full Stack (All services)"

$choice = Read-Host "`nEnter your choice (1-5)"

# Build the profiles parameter
$profiles = @()
switch ($choice) {
    "2" { $profiles += "--profile observability" }
    "3" { $profiles += "--profile kafka" }
    "4" { $profiles += "--profile cache" }
    "5" { 
        $profiles += "--profile observability"
        $profiles += "--profile kafka"
        $profiles += "--profile cache"
    }
}

# Build and start containers
Write-Host "`n🏗️  Building Docker images..." -ForegroundColor Yellow
$buildCmd = "docker-compose build"
Invoke-Expression $buildCmd

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n🚀 Starting containers..." -ForegroundColor Yellow
$startCmd = "docker-compose $($profiles -join ' ') up -d"
Invoke-Expression $startCmd

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start containers!" -ForegroundColor Red
    exit 1
}

# Wait for services to be healthy
Write-Host "`n⏳ Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check status
Write-Host "`n📊 Container Status:" -ForegroundColor Cyan
docker-compose ps

# Display access information
Write-Host "`n✅ Deployment complete!" -ForegroundColor Green
Write-Host "`n🌐 Access your application at:" -ForegroundColor Cyan
Write-Host "   API:              http://localhost:5000" -ForegroundColor White
Write-Host "   Health Check:     http://localhost:5000/health" -ForegroundColor White
Write-Host "   API Docs:         http://localhost:5000/scalar/v1" -ForegroundColor White
Write-Host "   PostgreSQL:       localhost:5432 (postgres/postgres)" -ForegroundColor White

if ($profiles -contains "--profile observability") {
    Write-Host "`n📊 Observability Tools:" -ForegroundColor Cyan
    Write-Host "   Seq:              http://localhost:5341" -ForegroundColor White
    Write-Host "   Grafana:          http://localhost:3000 (admin/admin)" -ForegroundColor White
}

if ($profiles -contains "--profile kafka") {
    Write-Host "`n📨 Messaging:" -ForegroundColor Cyan
    Write-Host "   Kafka:            localhost:9092" -ForegroundColor White
}

if ($profiles -contains "--profile cache") {
    Write-Host "`n💾 Cache:" -ForegroundColor Cyan
    Write-Host "   Redis:            localhost:6379" -ForegroundColor White
}

Write-Host "`n📝 Useful commands:" -ForegroundColor Cyan
Write-Host "   View logs:        docker-compose logs -f" -ForegroundColor White
Write-Host "   Stop services:    docker-compose down" -ForegroundColor White
Write-Host "   Restart API:      docker-compose restart webapi" -ForegroundColor White

Write-Host "`n✨ Happy coding!" -ForegroundColor Green

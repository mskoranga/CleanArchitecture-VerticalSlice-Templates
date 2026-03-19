# PowerShell script to stop and clean up Docker containers

Write-Host "🛑 Stopping Clean Architecture Docker containers..." -ForegroundColor Cyan

# Ask user about cleanup level
Write-Host "`n🔧 Select cleanup option:" -ForegroundColor Yellow
Write-Host "1. Stop containers (keep data)"
Write-Host "2. Stop and remove containers (keep data)"
Write-Host "3. Stop, remove containers and volumes (DELETE ALL DATA)"
Write-Host "4. Complete cleanup (delete everything including images)"

$choice = Read-Host "`nEnter your choice (1-4)"

switch ($choice) {
    "1" {
        Write-Host "`n⏸️  Stopping containers..." -ForegroundColor Yellow
        docker-compose stop
        Write-Host "✅ Containers stopped. Data preserved." -ForegroundColor Green
    }
    "2" {
        Write-Host "`n🗑️  Stopping and removing containers..." -ForegroundColor Yellow
        docker-compose down
        Write-Host "✅ Containers removed. Volumes preserved." -ForegroundColor Green
    }
    "3" {
        Write-Host "`n⚠️  WARNING: This will delete all database data!" -ForegroundColor Red
        $confirm = Read-Host "Are you sure? (yes/no)"
        if ($confirm -eq "yes") {
            Write-Host "`n🗑️  Stopping and removing containers and volumes..." -ForegroundColor Yellow
            docker-compose down -v
            Write-Host "✅ Containers and volumes removed." -ForegroundColor Green
        } else {
            Write-Host "❌ Cancelled." -ForegroundColor Yellow
        }
    }
    "4" {
        Write-Host "`n⚠️  WARNING: This will delete everything (containers, volumes, images)!" -ForegroundColor Red
        $confirm = Read-Host "Are you sure? (yes/no)"
        if ($confirm -eq "yes") {
            Write-Host "`n🗑️  Complete cleanup..." -ForegroundColor Yellow
            docker-compose down -v --rmi all --remove-orphans
            Write-Host "✅ Complete cleanup done." -ForegroundColor Green
        } else {
            Write-Host "❌ Cancelled." -ForegroundColor Yellow
        }
    }
    default {
        Write-Host "❌ Invalid choice." -ForegroundColor Red
    }
}

Write-Host "`n✨ Done!" -ForegroundColor Green

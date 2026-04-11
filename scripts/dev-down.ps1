# Stop and remove development environment
param()

docker-compose down -v
Write-Host "Containers removed. Volumes removed."

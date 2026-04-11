# Start development environment using Docker Compose
param()

Write-Host "Building and starting containers..."
docker-compose up --build -d

Write-Host "Showing service status:"
docker-compose ps

Write-Host "To view logs: docker-compose logs -f"

# Docker Deployment Guide

This guide explains how to build and run the Reactivities API using Docker.

## Prerequisites

- [Docker](https://www.docker.com/get-started) installed and running
- [Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)

## Quick Start with Docker Compose

The easiest way to run the application is with Docker Compose:

```bash
# Build and start the container
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop the container
docker-compose down

# Stop and remove volumes (reset database)
docker-compose down -v
```

The API will be available at `http://localhost:5000`

## Manual Docker Commands

### Build the Docker Image

```bash
# From the project root directory
docker build -t reactivities-api:latest -f API/Dockerfile .
```

### Run the Container

```bash
docker run -d \
  --name reactivities-api \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v reactivities-data:/app/data \
  reactivities-api:latest
```

### Useful Docker Commands

```bash
# View running containers
docker ps

# View logs
docker logs -f reactivities-api

# Stop the container
docker stop reactivities-api

# Start the container
docker start reactivities-api

# Remove the container
docker rm -f reactivities-api

# Remove the image
docker rmi reactivities-api:latest
```

## Configuration

### Environment Variables

You can configure the application using environment variables:

```bash
docker run -d \
  --name reactivities-api \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/reactivities.db" \
  -v reactivities-data:/app/data \
  reactivities-api:latest
```

### Persistent Data

The SQLite database is stored in `/app/data` inside the container. Use Docker volumes to persist data:

```bash
# Named volume (recommended)
-v reactivities-data:/app/data

# Bind mount (for development)
-v $(pwd)/data:/app/data
```

## Health Check

The API includes a health check endpoint at `/health`. You can check the container health:

```bash
# Check health status
docker inspect --format='{{.State.Health.Status}}' reactivities-api

# Manual health check
curl http://localhost:5000/health
```

Expected response: `Healthy` with HTTP 200

## Multi-Stage Build

The Dockerfile uses a multi-stage build for optimization:

1. **Build Stage**: Restores dependencies and builds the application
2. **Publish Stage**: Publishes the release build
3. **Runtime Stage**: Creates a minimal runtime image with only the published output

Benefits:
- Smaller final image size (runtime image only)
- Faster deployment
- Improved security (no SDK tools in production)

## Security Features

The Docker image includes several security best practices:

- ✅ Non-root user (`appuser`)
- ✅ Minimal runtime image (aspnet, not SDK)
- ✅ Health checks for container orchestration
- ✅ No HTTPS redirection (handled by reverse proxy/load balancer)
- ✅ Production environment configuration

## Troubleshooting

### Container won't start

```bash
# Check logs
docker logs reactivities-api

# Check if port is already in use
lsof -i :5000

# Use a different port
docker run -d -p 5001:8080 reactivities-api:latest
```

### Database issues

```bash
# Reset the database by removing the volume
docker-compose down -v
docker-compose up -d

# Or manually remove the volume
docker volume rm reactivities-data
```

### Build failures

```bash
# Clean Docker build cache
docker builder prune

# Rebuild without cache
docker build --no-cache -t reactivities-api:latest -f API/Dockerfile .
```

## Production Deployment

For production deployments, consider:

1. **Use a reverse proxy** (Nginx, Traefik) for HTTPS termination
2. **External database** instead of SQLite (PostgreSQL, SQL Server)
3. **Container orchestration** (Kubernetes, Docker Swarm)
4. **Secrets management** for sensitive configuration
5. **Monitoring and logging** (Prometheus, Grafana, ELK stack)

### Example with Traefik

```yaml
version: '3.8'

services:
  api:
    image: reactivities-api:latest
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.api.rule=Host(`api.yourdomain.com`)"
      - "traefik.http.routers.api.tls=true"
      - "traefik.http.routers.api.tls.certresolver=letsencrypt"
    networks:
      - traefik-public
    volumes:
      - api-data:/app/data

networks:
  traefik-public:
    external: true

volumes:
  api-data:
```

## GitHub Actions CI/CD

The `.github/workflows/dotnet.yml` workflow includes automated Docker image building:

- Builds Docker image on push to `main` branch
- Uses GitHub Actions cache for faster builds
- Validates Dockerfile during CI

## Further Reading

- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [ASP.NET Core in Docker](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)

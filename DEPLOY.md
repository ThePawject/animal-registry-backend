# Deployment Guide

This guide explains how to deploy the Animal Registry Backend application using Docker and GitHub Container Registry (GHCR). The application is containerized for easy CI/CD and local/remote deployment.

## Overview

The application is built as a .NET 9 ASP.NET Core web API, packaged in a Docker image, and automatically pushed to GHCR on every push to the `main` branch. The image includes automatic database migrations on startup.

## Prerequisites

- Docker and Docker Compose installed on your system.
- Access to pull the image from GHCR (if the repository is private, you need appropriate permissions).
- A production-ready `appsettings.json` file with your configuration.

## Preparing appsettings.json

The application uses `appsettings.json` for configuration. You need to prepare a version for production deployment that includes:

- Database connection strings (e.g., PostgreSQL, SQL Server).
- Any API keys, secrets, or external service URLs.
- Logging levels, allowed hosts, and other environment-specific settings.

Copy your development `appsettings.json` and modify it for production:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-server;Database=animalregistry;User Id=your-user;Password=your-password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
  // Add other production settings here
}
```

**Security Note:** Never commit sensitive data like passwords to the repository. Use environment variables or external secret management for sensitive values.

## Docker Compose Setup

Create a `docker-compose.yml` file in your deployment directory with the following content (adjust the image name if your repository differs):

```yaml
version: "3.9"
services:
  animal-registry:
    image: ghcr.io/your-org/animal-registry-backend:main
    container_name: animal-registry
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_URLS=http://0.0.0.0:8080
      # Add any additional environment variables here if needed
    volumes:
      - ./appsettings.json:/app/appsettings.json:ro
    restart: unless-stopped
```

- Replace `your-org` with your GitHub username or organization.
- Ensure `appsettings.json` is in the same directory as `docker-compose.yml` or adjust the volume path.

## Running the Application

1. Place your `appsettings.json` in the same directory as `docker-compose.yml`.
2. Run the following command to start the application:

   ```bash
   docker-compose up -d
   ```

   This will:
   - Pull the latest image from GHCR.
   - Mount your `appsettings.json` into the container.
   - Start the application on port 8080.
   - Run database migrations automatically on startup.

3. Check the logs to ensure everything started correctly:

   ```bash
   docker-compose logs -f animal-registry
   ```

4. Test the application by accessing `http://localhost:8080` (or your server's IP/port).

## Updating the Application

To update to the latest version:

1. Pull the latest image:

   ```bash
   docker-compose pull
   ```

2. Restart the services:

   ```bash
   docker-compose up -d
   ```

The application will run migrations if the database schema has changed.

## Troubleshooting

- **Port conflicts:** Ensure port 8080 is available or change the port mapping in `docker-compose.yml`.
- **Database issues:** Verify your connection string in `appsettings.json` and ensure the database is accessible.
- **Permission issues:** If mounting volumes fails, check file permissions and Docker user settings.
- **Image pull failures:** Confirm you have access to the GHCR repository and the image exists.

## Additional Notes

- The container runs as a non-root user for security.
- Environment variables can override settings in `appsettings.json` if needed.
- For production, consider using Docker secrets or external configuration management for sensitive data.
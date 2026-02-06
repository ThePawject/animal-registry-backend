# Deployment Guide

**Note:** This is a development setup using Docker Compose with SQL Server and Azurite. Not suitable for production.

## Docker Compose Setup

Create a `docker-compose.yml` file with the following content (adjust passwords and image names as needed):

```yaml
version: "3.9"
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=YourStrong!Passw0rd  # Change this to your desired password
      - ACCEPT_EULA=Y
    ports:
      - "1433:1433"
    volumes:
      - sql_data:/var/opt/mssql

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - "10000:10000"  # Blob service
      - "10001:10001"  # Queue service
      - "10002:10002"  # Table service
    volumes:
      - azurite_data:/data

  animal-registry:
    image: ghcr.io/thepawject/animal-registry-backend:main
    depends_on:
      - sqlserver
      - azurite
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_URLS=http://0.0.0.0:8080
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=AnimalRegistry;User Id=sa;Password=YourStrong!Passw0rd;
      - AzureStorage__ConnectionString=UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite;
    restart: unless-stopped

volumes:
  sql_data:
  azurite_data:
```
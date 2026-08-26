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

- Replace `thepawject` with your GitHub username or organization.
- Change `YourStrong!Passw0rd` to your desired SQL Server SA password.

## Contact form (`POST /contact`)

The public contact form endpoint needs a mailbox to notify. Configuration lives under `Email` and
`Contact:RateLimit`; on Azure App Service supply the secrets as application settings
(`Email__UserName`, `Email__Password`, `Email__FromAddress`, `Email__ContactRecipient`).

| Setting | Default | Notes |
| --- | --- | --- |
| `Email:Enabled` | `true` | `false` skips sending entirely; submissions are still stored. |
| `Email:Host` | `smtp.gmail.com` | |
| `Email:Port` | `587` | **Port 25 is blocked outbound on App Service** and is rejected on start-up. Use 587 (STARTTLS) or 465 (`Email:UseImplicitTls: true`). |
| `Email:UserName` / `Email:Password` | - | For Gmail this is the mailbox plus an **app password** (2FA has to be on). |
| `Email:FromAddress` | - | Must be the authenticated mailbox or one of its aliases. |
| `Email:ContactRecipient` | - | Team mailbox that receives the submissions. |
| `Contact:RateLimit:PermitLimit` | `5` | Fixed window per client IP. |
| `Contact:RateLimit:WindowMinutes` | `15` | |

The settings are validated when the host starts, so a misconfiguration fails the deployment instead of the
first submission.

### Running it locally

`appsettings.Development.json` ships with `Email:Enabled: false`, so the app runs without SMTP credentials and
only logs what it would have sent. To send for real, put the credentials in user secrets and flip the flag:

```bash
cd AnimalRegistry
dotnet user-secrets set "Email:Enabled" "true"
dotnet user-secrets set "Email:UserName" "you@gmail.com"
dotnet user-secrets set "Email:Password" "<gmail app password>"
dotnet user-secrets set "Email:FromAddress" "you@gmail.com"
dotnet user-secrets set "Email:ContactRecipient" "you@gmail.com"
```

Every submission is stored in `ContactRequests` with the consent timestamp and a delivery status, so leads
survive a mail outage:

```sql
SELECT Id, ShelterName, Email, ConsentGivenOn, DeliveryStatus, DeliveryError
FROM ContactRequests
WHERE DeliveryStatus = 'Failed';
```

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ./AnimalRegistry/AnimalRegistry.csproj ./AnimalRegistry/
RUN dotnet restore ./AnimalRegistry/AnimalRegistry.csproj

COPY . .
WORKDIR /src/AnimalRegistry

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup --home /app appuser
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

USER appuser
ENTRYPOINT ["dotnet", "AnimalRegistry.dll"]
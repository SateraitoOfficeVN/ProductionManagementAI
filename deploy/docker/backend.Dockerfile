# Build context is the repo root (see deploy/compose.yaml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/backend/ProductionManagementAI.slnx .
COPY src/backend/ProductionManagementAI.Domain/ProductionManagementAI.Domain.csproj ProductionManagementAI.Domain/
COPY src/backend/ProductionManagementAI.Application/ProductionManagementAI.Application.csproj ProductionManagementAI.Application/
COPY src/backend/ProductionManagementAI.Infrastructure/ProductionManagementAI.Infrastructure.csproj ProductionManagementAI.Infrastructure/
COPY src/backend/ProductionManagementAI.Api/ProductionManagementAI.Api.csproj ProductionManagementAI.Api/
RUN dotnet restore ProductionManagementAI.Api/ProductionManagementAI.Api.csproj

COPY src/backend/ .
RUN dotnet publish ProductionManagementAI.Api/ProductionManagementAI.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductionManagementAI.Api.dll"]

# One-shot EF Core migration runner, so migrations need no .NET SDK on the host (scripts/start-app.ps1).
# Build context is the repo root (see deploy/compose.yaml). Runs as the database owner, never as pmai_app (DEC-016).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN dotnet tool install --global dotnet-ef --version 10.0.12
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY src/backend/ProductionManagementAI.slnx .
COPY src/backend/ProductionManagementAI.Domain/ProductionManagementAI.Domain.csproj ProductionManagementAI.Domain/
COPY src/backend/ProductionManagementAI.Application/ProductionManagementAI.Application.csproj ProductionManagementAI.Application/
COPY src/backend/ProductionManagementAI.Infrastructure/ProductionManagementAI.Infrastructure.csproj ProductionManagementAI.Infrastructure/
COPY src/backend/ProductionManagementAI.Api/ProductionManagementAI.Api.csproj ProductionManagementAI.Api/
RUN dotnet restore ProductionManagementAI.Api/ProductionManagementAI.Api.csproj

COPY src/backend/ .
RUN dotnet ef migrations bundle \
        --project ProductionManagementAI.Infrastructure \
        --startup-project ProductionManagementAI.Api \
        --output /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/efbundle .
# The connection comes from ConnectionStrings__DefaultConnection, set by the compose `migrate` service.
ENTRYPOINT ["./efbundle"]

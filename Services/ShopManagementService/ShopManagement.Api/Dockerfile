FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files for restore (better caching)
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/ShopManagementService/ShopManagement.Api/ShopManagement.Api.csproj", "Services/ShopManagementService/ShopManagement.Api/"]
COPY ["Services/ShopManagementService/ShopManagement.Application/ShopManagement.Application.csproj", "Services/ShopManagementService/ShopManagement.Application/"]
COPY ["Services/ShopManagementService/ShopManagement.Domain/ShopManagement.Domain.csproj", "Services/ShopManagementService/ShopManagement.Domain/"]
COPY ["Services/ShopManagementService/ShopManagement.Infrastructure/ShopManagement.Infrastructure.csproj", "Services/ShopManagementService/ShopManagement.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "BuildingBlocks/BuildingBlocks.Api/"]
COPY ["BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "BuildingBlocks/BuildingBlocks.Application/"]
COPY ["BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/BuildingBlocks.MediatR/BuildingBlocks.MediatR.csproj", "BuildingBlocks/BuildingBlocks.MediatR/"]
COPY ["BuildingBlocks/BuildingBlocks.EfCore/BuildingBlocks.EfCore.csproj", "BuildingBlocks/BuildingBlocks.EfCore/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/BuildingBlocks.Messaging.RabbitMQ.csproj", "BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/"]

# Restore dependencies
RUN dotnet restore Services/ShopManagementService/ShopManagement.Api/ShopManagement.Api.csproj

# Copy source code
COPY ["Services/ShopManagementService/", "Services/ShopManagementService/"]
COPY ["Services/Shared/", "Services/Shared/"]
COPY ["BuildingBlocks/", "BuildingBlocks/"]

# Build and publish in one step
RUN dotnet publish Services/ShopManagementService/ShopManagement.Api/ShopManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Simplified user setup
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app

USER appuser

COPY --from=build ["/app/publish", "."]

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ShopManagement.Api.dll"]
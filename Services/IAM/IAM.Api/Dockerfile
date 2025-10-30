FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files for restore (better caching)
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/IAM/IAM.Api/IAM.Api.csproj", "Services/IAM/IAM.Api/"]
COPY ["Services/IAM/IAM.Application/IAM.Application.csproj", "Services/IAM/IAM.Application/"]
COPY ["Services/IAM/IAM.Domain/IAM.Domain.csproj", "Services/IAM/IAM.Domain/"]
COPY ["Services/IAM/IAM.Infrastructure/IAM.Infrastructure.csproj", "Services/IAM/IAM.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "BuildingBlocks/BuildingBlocks.Api/"]
COPY ["BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "BuildingBlocks/BuildingBlocks.Application/"]
COPY ["BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/BuildingBlocks.MediatR/BuildingBlocks.MediatR.csproj", "BuildingBlocks/BuildingBlocks.MediatR/"]
COPY ["BuildingBlocks/BuildingBlocks.EfCore/BuildingBlocks.EfCore.csproj", "BuildingBlocks/BuildingBlocks.EfCore/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/BuildingBlocks.Messaging.RabbitMQ.csproj", "BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/"]

# Restore dependencies
RUN dotnet restore Services/IAM/IAM.Api/IAM.Api.csproj

# Copy source code
COPY ["Services/IAM/", "Services/IAM/"]
COPY ["Services/Shared/", "Services/Shared/"]
COPY ["BuildingBlocks/", "BuildingBlocks/"]

# Copy JWT keys
COPY ["secrets/", "secrets/"]

# Build and publish in one step
RUN dotnet publish Services/IAM/IAM.Api/IAM.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Simplified user setup
RUN adduser --disabled-password --gecos '' appuser && \
    mkdir -p /app/secrets && \
    chown -R appuser:appuser /app

USER appuser

COPY --from=build ["/app/publish", "."]
COPY --from=build ["/app/secrets", "secrets/"]

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "IAM.Api.dll"]
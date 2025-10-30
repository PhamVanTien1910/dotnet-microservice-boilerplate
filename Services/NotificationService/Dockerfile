FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files for restore (better caching)
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/NotificationService/NotificationService.csproj", "Services/NotificationService/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/BuildingBlocks.Messaging.RabbitMQ.csproj", "BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/"]
COPY ["BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "BuildingBlocks/BuildingBlocks.Application/"]

# Restore dependencies
RUN dotnet restore Services/NotificationService/NotificationService.csproj

# Copy source code
COPY ["Services/NotificationService/", "Services/NotificationService/"]
COPY ["Services/Shared/", "Services/Shared/"]
COPY ["BuildingBlocks/", "BuildingBlocks/"]

# Build and publish in one step
RUN dotnet publish Services/NotificationService/NotificationService.csproj -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "NotificationService.dll"]
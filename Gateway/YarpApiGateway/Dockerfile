FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files for restore (better caching)
COPY ["Gateway/YarpApiGateway/YarpApiGateway.csproj", "Gateway/YarpApiGateway/"]
COPY ["BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "BuildingBlocks/BuildingBlocks.Api/"]
COPY ["BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "BuildingBlocks/BuildingBlocks.Application/"]
COPY ["BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/BuildingBlocks.MediatR/BuildingBlocks.MediatR.csproj", "BuildingBlocks/BuildingBlocks.MediatR/"]

# Restore dependencies
RUN dotnet restore Gateway/YarpApiGateway/YarpApiGateway.csproj

# Copy source code
COPY ["Gateway/", "Gateway/"]
COPY ["BuildingBlocks/", "BuildingBlocks/"]

# Build and publish in one step
RUN dotnet publish Gateway/YarpApiGateway/YarpApiGateway.csproj -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "YarpApiGateway.dll"]
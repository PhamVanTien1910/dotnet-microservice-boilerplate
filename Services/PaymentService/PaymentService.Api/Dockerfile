FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files for restore (better caching)
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/PaymentService/PaymentService.Api/PaymentService.Api.csproj", "Services/PaymentService/PaymentService.Api/"]
COPY ["Services/PaymentService/PaymentService.Application/PaymentService.Application.csproj", "Services/PaymentService/PaymentService.Application/"]
COPY ["Services/PaymentService/PaymentService.Domain/PaymentService.Domain.csproj", "Services/PaymentService/PaymentService.Domain/"]
COPY ["Services/PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "Services/PaymentService/PaymentService.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "BuildingBlocks/BuildingBlocks.Api/"]
COPY ["BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "BuildingBlocks/BuildingBlocks.Application/"]
COPY ["BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/BuildingBlocks.MediatR/BuildingBlocks.MediatR.csproj", "BuildingBlocks/BuildingBlocks.MediatR/"]
COPY ["BuildingBlocks/BuildingBlocks.EfCore/BuildingBlocks.EfCore.csproj", "BuildingBlocks/BuildingBlocks.EfCore/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/BuildingBlocks.Messaging.RabbitMQ.csproj", "BuildingBlocks/BuildingBlocks.Messaging.RabbitMQ/"]

# Restore dependencies
RUN dotnet restore Services/PaymentService/PaymentService.Api/PaymentService.Api.csproj

# Copy source code
COPY ["Services/PaymentService/", "Services/PaymentService/"]
COPY ["Services/Shared/", "Services/Shared/"]
COPY ["BuildingBlocks/", "BuildingBlocks/"]

# Build and publish in one step
RUN dotnet publish Services/PaymentService/PaymentService.Api/PaymentService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]

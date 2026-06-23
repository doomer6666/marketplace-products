FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Marketplace.Products.Api/Marketplace.Products.Api.csproj", "Marketplace.Products.Api/"]
COPY ["Marketplace.Products.Application/Marketplace.Products.Application.csproj", "Marketplace.Products.Application/"]
COPY ["Marketplace.Products.Domain/Marketplace.Products.Domain.csproj", "Marketplace.Products.Domain/"]
COPY ["Marketplace.Products.Infrastructure/Marketplace.Products.Infrastructure.csproj", "Marketplace.Products.Infrastructure/"]
COPY ["Marketplace.Products.Migrations/Marketplace.Products.Migrations.csproj", "Marketplace.Products.Migrations/"]

RUN dotnet restore "Marketplace.Products.Api/Marketplace.Products.Api.csproj"
COPY . .
WORKDIR "/src/Marketplace.Products.Api"
RUN dotnet build "Marketplace.Products.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Marketplace.Products.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Marketplace.Products.Api.dll"]
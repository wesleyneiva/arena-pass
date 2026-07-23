FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ArenaPass.Domain/ArenaPass.Domain.csproj src/ArenaPass.Domain/
COPY src/ArenaPass.Application/ArenaPass.Application.csproj src/ArenaPass.Application/
COPY src/ArenaPass.Infrastructure/ArenaPass.Infrastructure.csproj src/ArenaPass.Infrastructure/
COPY src/ArenaPass.Api/ArenaPass.Api.csproj src/ArenaPass.Api/
RUN dotnet restore src/ArenaPass.Api/ArenaPass.Api.csproj

COPY src/ src/
RUN dotnet publish src/ArenaPass.Api/ArenaPass.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libkrb5-3 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
CMD ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ArenaPass.Api.dll"]

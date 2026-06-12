FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer-cached restore
COPY GolfManager.sln ./
COPY src/GolfManager.Api/GolfManager.Api.csproj src/GolfManager.Api/
COPY src/GolfManager.Core/GolfManager.Core.csproj src/GolfManager.Core/
COPY src/GolfManager.Data/GolfManager.Data.csproj src/GolfManager.Data/
COPY src/GolfManager.Services/GolfManager.Services.csproj src/GolfManager.Services/
COPY src/GolfManager.Shared/GolfManager.Shared.csproj src/GolfManager.Shared/
COPY src/GolfManager.Web/GolfManager.Web.csproj src/GolfManager.Web/
COPY nuget.config ./
RUN dotnet restore src/GolfManager.Api/GolfManager.Api.csproj

# Copy all source and publish — the Web project is built automatically via ProjectReference
COPY . .
RUN dotnet publish src/GolfManager.Api/GolfManager.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GolfManager.Api.dll"]

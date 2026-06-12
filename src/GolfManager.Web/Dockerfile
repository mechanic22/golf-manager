FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer-cached restore
COPY GolfManager.sln ./
COPY src/GolfManager.Web/GolfManager.Web.csproj src/GolfManager.Web/
COPY src/GolfManager.Shared/GolfManager.Shared.csproj src/GolfManager.Shared/
COPY nuget.config ./
RUN dotnet restore src/GolfManager.Web/GolfManager.Web.csproj

# Copy all source and publish
COPY . .
RUN dotnet publish src/GolfManager.Web/GolfManager.Web.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/GolfManager.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

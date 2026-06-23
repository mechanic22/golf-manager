FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY publish/ .
ENTRYPOINT ["dotnet", "GolfManager.Api.dll"]

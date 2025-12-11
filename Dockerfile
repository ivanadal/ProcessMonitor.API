# Multi-stage build for .NET 10 ASP.NET app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy project files and restore (adjust if you have a solution file)
COPY . .
WORKDIR /src/ProcessMonitor.API
RUN dotnet restore

# publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app

# Create DB folder
RUN mkdir -p /app/Data

# copy published output
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProcessMonitor.API.dll"]

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project definitions for layer caching
COPY SecureNoteApp.sln ./
COPY Directory.Build.props ./
COPY src/Domain/SecureNoteApp.Domain.csproj src/Domain/
COPY src/Application/SecureNoteApp.Application.csproj src/Application/
COPY src/Infrastructure/SecureNoteApp.Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/SecureNoteApp.WebApi.csproj src/WebApi/

# Restore dependencies
RUN dotnet restore

# Copy full source code
COPY src/ src/

# Build and publish release
WORKDIR /app/src/WebApi
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published application from build stage
COPY --from=build /out ./

# Environment defaults
ENV ASPNETCORE_URLS=http://+:5039
ENV ASPNETCORE_ENVIRONMENT=Production
ENV EnableSwagger=true

# Expose backend port
EXPOSE 5039

# Run WebApi
ENTRYPOINT ["dotnet", "SecureNoteApp.WebApi.dll"]

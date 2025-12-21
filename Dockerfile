# ==============================================================================
# HR Management .NET API - Dockerfile
# Multi-stage build for optimized production image
# ==============================================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["hr-management-dotnet.csproj", "./"]
RUN dotnet restore "hr-management-dotnet.csproj"

# Copy all source files and build
COPY . .
RUN dotnet build "hr-management-dotnet.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "hr-management-dotnet.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5177
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port
EXPOSE 5177

# Copy published files
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "hr-management-dotnet.dll"]

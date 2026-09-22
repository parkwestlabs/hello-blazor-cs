# Stage 1: Build the application using the heavy SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /workspace

# Copy csproj and restore dependencies (optimizes Docker layer caching)
COPY MyApp.slnx ./
COPY global.json ./
COPY .editorconfig ./
COPY dotnet-tools.json ./
COPY coverage.settings.xml ./
COPY Directory.Build.props ./

COPY src/MyApp.Core/MyApp.Core.csproj src/MyApp.Core/
COPY src/MyApp.Data/MyApp.Data.csproj src/MyApp.Data/
COPY src/MyApp.Web/MyApp.Web.csproj src/MyApp.Web/
COPY tests/MyApp.Tests/MyApp.Tests.csproj tests/MyApp.Tests/

RUN dotnet restore

COPY src/ src/
COPY tests/ tests/

WORKDIR /workspace/src/MyApp.Web
RUN dotnet publish "MyApp.Web.csproj" -c Release -o /app/out /p:UseAppHost=false

# Stage 2: Create the final lean runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the compiled binaries from the build stage
COPY --from=build /app/out ./

# Run the container as a secure, non-root user (built into .NET 8+)
USER $APP_UID

# Note: the default value for ASPNETCORE_HTTP_PORTS is 8080 since .NET 8
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyApp.Web.dll"]

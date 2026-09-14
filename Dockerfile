# Stage 1: Build the application using the heavy SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (optimizes Docker layer caching)
COPY MyApp.slnx ./
COPY global.json ./
COPY .editorconfig ./
COPY dotnet-tools.json ./
COPY coverage.settings.xml ./
COPY Directory.Build.props ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore

# Copy the rest of the code and publish the release binaries
COPY . ./
WORKDIR /src/src/MyApp.Web
RUN dotnet publish "MyApp.Web.csproj" -c Release -o /app/out /p:UseAppHost=false

# Stage 2: Create the final lean runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the compiled binaries from the build stage
COPY --from=build /app/out ./

# Run the container as a secure, non-root user (built into .NET 8+)
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "MyApp.Web.dll"]

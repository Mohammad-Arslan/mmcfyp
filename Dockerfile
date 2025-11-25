# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files first
COPY . ./

# Restore dependencies - use solution file if exists, otherwise use the first csproj file (searches recursively)
RUN CSPROJ=$(find . -name "*.csproj" -type f | head -n 1) && \
    if [ -n "$CSPROJ" ]; then \
        if find . -name "*.sln" -type f | head -n 1 | grep -q .; then \
            dotnet restore; \
        else \
            dotnet restore "$CSPROJ"; \
        fi; \
    else \
        echo "Error: No .csproj file found!" && exit 1; \
    fi

# Build using solution file if exists, otherwise use the first csproj file (searches recursively)
RUN CSPROJ=$(find . -name "*.csproj" -type f | head -n 1) && \
    if [ -n "$CSPROJ" ]; then \
        if find . -name "*.sln" -type f | head -n 1 | grep -q .; then \
            dotnet build -c Release -o /app/build; \
        else \
            dotnet build "$CSPROJ" -c Release -o /app/build; \
        fi; \
    else \
        echo "Error: No .csproj file found!" && exit 1; \
    fi

# Publish the application
FROM build AS publish
# Publish using solution file if exists, otherwise use the first csproj file (searches recursively)
RUN CSPROJ=$(find . -name "*.csproj" -type f | head -n 1) && \
    if [ -n "$CSPROJ" ]; then \
        if find . -name "*.sln" -type f | head -n 1 | grep -q .; then \
            dotnet publish -c Release -o /app/publish /p:UseAppHost=false; \
        else \
            dotnet publish "$CSPROJ" -c Release -o /app/publish /p:UseAppHost=false; \
        fi; \
    else \
        echo "Error: No .csproj file found!" && exit 1; \
    fi

# Use the ASP.NET Core runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
# Find the main application DLL by looking for one with a runtimeconfig.json file
RUN echo '#!/bin/sh' > /app/entrypoint.sh && \
    echo 'set -e' >> /app/entrypoint.sh && \
    echo '# Find DLLs that have a corresponding runtimeconfig.json (these are executable apps)' >> /app/entrypoint.sh && \
    echo 'for dll in /app/*.dll; do' >> /app/entrypoint.sh && \
    echo '  if [ -f "$dll" ]; then' >> /app/entrypoint.sh && \
    echo '    basename_dll=$(basename "$dll" .dll)' >> /app/entrypoint.sh && \
    echo '    runtimeconfig="/app/${basename_dll}.runtimeconfig.json"' >> /app/entrypoint.sh && \
    echo '    if [ -f "$runtimeconfig" ]; then' >> /app/entrypoint.sh && \
    echo '      # Skip system DLLs and view DLLs' >> /app/entrypoint.sh && \
    echo '      if echo "$basename_dll" | grep -qE "^System\.|^Microsoft\.|Views$|PrecompiledViews$"; then' >> /app/entrypoint.sh && \
    echo '        continue' >> /app/entrypoint.sh && \
    echo '      fi' >> /app/entrypoint.sh && \
    echo '      echo "Starting application: $basename_dll.dll"' >> /app/entrypoint.sh && \
    echo '      exec dotnet "$dll"' >> /app/entrypoint.sh && \
    echo '    fi' >> /app/entrypoint.sh && \
    echo '  fi' >> /app/entrypoint.sh && \
    echo 'done' >> /app/entrypoint.sh && \
    echo 'echo "Error: No application DLL with runtimeconfig.json found!"' >> /app/entrypoint.sh && \
    echo 'echo "Available DLLs:"' >> /app/entrypoint.sh && \
    echo 'ls -la /app/*.dll 2>/dev/null | head -10 || true' >> /app/entrypoint.sh && \
    echo 'exit 1' >> /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh && \
    chown appuser:appuser /app/entrypoint.sh

# Switch to non-root user
USER appuser

ENTRYPOINT ["/app/entrypoint.sh"]

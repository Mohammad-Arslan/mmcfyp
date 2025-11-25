# Hot Reload Setup - Docker Development

This project is configured for **hot reload** in Docker, so code changes are automatically reflected without restarting containers.

## How It Works

1. **Development Dockerfile** (`Dockerfile.dev`):
   - Uses .NET SDK image (includes build tools)
   - Installs `dotnet watch` tool
   - Runs `dotnet watch run` for automatic rebuilds

2. **Volume Mounting**:
   - Source code is mounted as a volume
   - Changes on host are immediately visible in container
   - `dotnet watch` detects changes and rebuilds automatically

3. **Environment Variables**:
   - `DOTNET_USE_POLLING_FILE_WATCHER=true` - Ensures file changes are detected
   - `DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true` - Restarts on major changes

## Usage

### Start with Hot Reload (Default)

```bash
# Uses Dockerfile.dev by default
docker-compose up --build

# Or explicitly
DOCKERFILE=Dockerfile.dev docker-compose up --build
```

### Production Build (No Hot Reload)

```bash
# Use production Dockerfile
DOCKERFILE=Dockerfile docker-compose up --build
```

## What Gets Hot Reloaded

✅ **Automatically reloads:**
- `.razor` files (Blazor components)
- `.cs` files (C# code)
- `.css` files (styles)
- Configuration files

⚠️ **Requires container restart:**
- `Program.cs` changes (startup code)
- `appsettings.json` changes
- Database migrations
- NuGet package changes

## Monitoring Hot Reload

Watch the container logs to see hot reload in action:

```bash
docker-compose logs -f blazor-app
```

You'll see messages like:
```
watch : File changed: /app/MedicalManagementSystem/Components/Pages/Admin/Appointments.razor
watch : Building...
watch : Build succeeded
```

## Troubleshooting

### Changes Not Reflecting

1. **Check file permissions**: Ensure files are writable
2. **Check volume mount**: Verify source code is mounted correctly
   ```bash
   docker exec blazor-app ls -la /app/MedicalManagementSystem
   ```
3. **Check logs**: Look for file watcher errors
   ```bash
   docker-compose logs blazor-app | grep -i watch
   ```

### Performance Issues

- Build artifacts are stored in anonymous volumes (faster)
- First build may take longer
- Subsequent builds are incremental (faster)

### File Watcher Not Working

If file changes aren't detected:
1. Ensure `DOTNET_USE_POLLING_FILE_WATCHER=true` is set
2. Check if you're on WSL2 or Docker Desktop (may need polling)
3. Restart the container if needed

## Development Workflow

1. **Start containers**:
   ```bash
   docker-compose up --build
   ```

2. **Make code changes** in your IDE/editor

3. **Save the file** - changes are automatically detected

4. **Wait for rebuild** - check logs for "Build succeeded"

5. **Refresh browser** - see your changes!

## Production Deployment

For production, use the standard Dockerfile:

```bash
DOCKERFILE=Dockerfile docker-compose -f docker-compose.prod.yml up --build
```

The production Dockerfile:
- Uses optimized runtime image
- Pre-builds the application
- No hot reload (not needed in production)

---

**Note**: Hot reload works best for code changes. For major structural changes or new dependencies, you may need to restart the container.


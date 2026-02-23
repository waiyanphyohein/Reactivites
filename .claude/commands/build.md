# Build: $ARGUMENTS

Build and verify the Reactivities application.

## Backend

```bash
# Restore and build the full solution
dotnet restore && dotnet build

# Build only (assumes restore done)
dotnet build

# Build in Release mode
dotnet build -c Release

# Run the API locally (HTTPS on port 5001)
dotnet run --project API/API.csproj
```

## Frontend

```bash
# Install dependencies
cd client && npm install

# Development server (http://localhost:3000)
cd client && npm run dev

# Production build
cd client && npm run build

# Lint check
cd client && npm run lint

# Preview production build
cd client && npm run preview
```

## Checks to Run Before Committing

1. `dotnet build` — no warnings treated as errors
2. `dotnet test` — all tests pass (65 total, 2 expected skips)
3. `cd client && npm run lint` — no ESLint errors
4. `cd client && npm run build` — production build succeeds

## Common Build Issues

- **EF Core tools missing**: `dotnet tool install --global dotnet-ef`
- **Port already in use**: Check `API/appsettings.json` Kestrel config (HTTP:5000, HTTPS:5001)
- **HTTPS dev cert**: `dotnet dev-certs https --trust`
- **npm version mismatch**: Use the node version specified in `client/package.json` engines field

# Secure API Project

This project is a secure ASP.NET Core Web API template with best practices for security, database initialization, and modern architecture.

## Features

- **HTTPS enforced**: All traffic is redirected to HTTPS.
- **HSTS (HTTP Strict Transport Security)**: Configured for 1 year, with preload and subdomain support.
- **CORS**: Only allows requests from specified frontend origins with credentials and wildcard subdomain support.
- **Custom Security Headers**: Middleware adds security headers to all responses.
- **Rate Limiting**: Basic rate limiting middleware (can be extended).
- **Swagger/OpenAPI**: Enabled in development for API documentation.
- **Repository Pattern**: Data access is abstracted for maintainability.
- **Service Layer**: Business logic is separated from controllers.
- **Database Initialization & Migration**: Automatic migration and seeding on startup.
- **Snake_case Database Columns**: Consistent naming convention for database columns.

## Getting Started

### Prerequisites

- [.NET 7+ SDK](https://dotnet.microsoft.com/download)
- SQL Server or compatible database (update connection string as needed)
- (Optional) [Docker](https://www.docker.com/) for containerization

### Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/your-username/your-repo.git
   cd your-repo/API
   ```

2. **Configure Environment**

   - Update `appsettings.json` with your database connection string and other settings.

3. **Run Database Migrations & Seed**

   On first run, the API will automatically apply migrations and seed the database.

4. **Run the API**

   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001`.

5. **Access Swagger UI (Development Only)**

   Visit `https://localhost:5001/swagger` for interactive API documentation.

## Security Highlights

- **HSTS**: Enforced in production for all subdomains.
- **CORS**: Only allows requests from trusted frontend URLs.
- **Security Headers**: Custom middleware adds headers like `X-Content-Type-Options`, `X-Frame-Options`, etc.
- **Rate Limiting**: Basic IP-based rate limiting middleware (can be replaced with a full-featured library).

## Project Structure

- `API/Program.cs`: Main entry point, configures services and middleware.
- `DbInitializer`: Seeds the database on startup.
- `Repositories/`: Implements the repository pattern for data access.
- `Services/`: Contains business logic (service layer).
- `Controllers/`: API endpoints.

## Customization

- **Add more CORS origins** in `Program.cs` as needed.
- **Enhance rate limiting** by integrating a library like [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit).
- **Add more security headers** in the custom middleware.

## Contributing

Pull requests are welcome! Please follow the repository and service layer patterns, and use `snake_case` for all database columns.

## License

[MIT](LICENSE)

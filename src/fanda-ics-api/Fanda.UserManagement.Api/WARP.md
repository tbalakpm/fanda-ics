# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

The Fanda User Management API is an ASP.NET Core 9.0 authentication and authorization service built using Clean Architecture principles. It provides JWT-based authentication, role-based authorization, and complete user management functionality with PostgreSQL as the data store.

## Essential Commands

### Development Commands
```bash
# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run the application (Development environment)
dotnet run

# Run with hot reload during development
dotnet watch run

# Run from solution level
dotnet run --project Fanda.UserManagement.Api
```

### Database Commands
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database with latest migrations
dotnet ef database update

# Remove last migration (if not applied to database)
dotnet ef migrations remove

# View migration history
dotnet ef migrations list
```

**Important:** Set the `USERDB_CONNECTION` environment variable with your PostgreSQL connection string before running database commands.

### Testing and API Exploration
```bash
# Test endpoints using provided HTTP file
# Use VS Code REST Client extension with Fanda.UserManagement.Api.http

# Access API documentation (Development/Staging only)
# Navigate to: https://localhost:7298/scalar/v1 (via Scalar OpenAPI)
```

## Architecture Overview

This project implements Clean Architecture with the following layers:

### Domain Layer (`Domain/`)
- **Entities**: Core business entities (User, Role, RefreshToken)
- **Interfaces**: Repository and service contracts
- Contains no dependencies on external frameworks

### Application Layer (`Application/`)
- **DTOs**: Data transfer objects for API communication
- **Interfaces**: Use case contracts (IAuthUseCase, IUserUseCase)
- **UseCases**: Business logic implementation
- Orchestrates domain entities and infrastructure services

### Infrastructure Layer (`Infrastructure/`)
- **Data**: Entity Framework Core setup, DbContext, and repositories
- **ExternalServices**: JWT token generation, email service, password hashing
- **Configuration**: Settings classes for JWT and email
- Implements domain interfaces using external frameworks

### Presentation Layer (`Presentation/`)
- **Endpoints**: Minimal API endpoint definitions
- Maps HTTP requests to application use cases
- Handles authentication and authorization concerns

### Key Components Flow
1. HTTP request hits **AuthEndpoints** or **UserEndpoints**
2. Endpoint calls appropriate **UseCase** (AuthUseCase, UserUseCase)
3. UseCase coordinates with **Repositories** and **External Services**
4. Data persisted via **Entity Framework Core** with **PostgreSQL**
5. JWT tokens generated and returned via **JwtTokenGenerator**

## Development Workflows

### Adding New Features
1. **Create DTOs** in `Application/DTOs/` for request/response models
2. **Add business logic** in `Application/UseCases/` or create new use case
3. **Create endpoints** in `Presentation/Endpoints/` to expose functionality
4. **Update Program.cs** to register new dependencies if needed

### Database Schema Changes
1. **Modify entities** in `Domain/Entities/` or `Infrastructure/Data/ApplicationUser.cs`
2. **Update DbContext** in `Infrastructure/Data/UserDbContext.cs` if needed
3. **Create migration**: `dotnet ef migrations add DescriptiveName`
4. **Apply migration**: `dotnet ef database update`

### Adding New User Roles
1. **Update ApplicationRole constants** in `Infrastructure/Data/ApplicationUser.cs`
2. **Add role to AllRoles array** for automatic seeding
3. **Create migration** if database schema changes needed
4. **Update authorization policies** in endpoints as required

## Critical Configuration

### Required Environment Variables
- `USERDB_CONNECTION`: PostgreSQL connection string

### appsettings.json Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "Fanda.UserManagement.Api",
    "Audience": "Fanda.UserManagement.Api",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  },
  "AdminUser": {
    "Email": "admin@fanda.com",
    "Password": "Admin@123"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@fanda.com",
    "FromName": "Fanda User Management"
  }
}
```

### Database Conventions
- **Snake case naming**: Automatically applied via `UseSnakeCaseNamingConvention()`
- **NodaTime support**: Enhanced PostgreSQL date/time handling
- **Auto-migration**: Database is automatically migrated on startup
- **Role seeding**: All roles from `ApplicationRole.AllRoles` are created on startup
- **Admin seeding**: Default admin user created if not exists

## API Testing

### Using the HTTP File
The `Fanda.UserManagement.Api.http` file contains pre-configured requests for all endpoints:
1. Open in VS Code with REST Client extension
2. Update tokens from login response for protected endpoints
3. Base URL: `http://localhost:5174` (HTTP) or `https://localhost:7298` (HTTPS)

### Key Endpoints
- **POST** `/api/auth/register` - Register new user
- **POST** `/api/auth/login` - Login and receive JWT tokens
- **POST** `/api/auth/refresh-token` - Refresh expired access token
- **POST** `/api/auth/change-password` - Change password (requires auth)
- **POST** `/api/auth/logout` - Logout and revoke refresh token
- **GET** `/api/users` - List users (requires auth)

### Authentication Flow
1. **Register** or **Login** to receive access token and refresh token
2. **Include Bearer token** in Authorization header for protected endpoints
3. **Refresh token** when access token expires (60 minutes default)
4. **Logout** to revoke refresh token when session ends

## Logs and Debugging

### Serilog Configuration
- **Console output**: Enabled in all environments
- **File logging**: `logs/usermgmt-{date}.log` with daily rolling
- **Structured logging**: JSON format with machine name, process ID, thread ID
- **Request logging**: HTTP request/response logging via middleware

### Development Settings
- **Sensitive data logging**: Enabled in Development environment only
- **Detailed errors**: Full exception details in Development
- **Hot reload**: Use `dotnet watch run` for automatic restart on file changes

## Security Notes

- **JWT Secret**: Must be at least 32 characters for HMAC-SHA256
- **Password hashing**: Uses ASP.NET Core Identity's default hasher
- **Token validation**: Zero clock skew, validates issuer, audience, and expiry
- **HTTPS**: Required in production (configured in launchSettings.json)
- **CORS**: Configure as needed for your frontend applications

## Solution Structure

This project is part of the larger `Fanda.ICS.Api.sln` solution which includes:
- `Fanda.ICS.Api` - Main ICS API project
- `Fanda.UserManagement.Api` - This authentication service

When working at the solution level, reference projects by name in commands:
```bash
dotnet build Fanda.UserManagement.Api
dotnet run --project Fanda.UserManagement.Api
```

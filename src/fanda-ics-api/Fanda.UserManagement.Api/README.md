# Fanda User Management API

A complete authentication and authorization system built with ASP.NET Core 9.0, Entity Framework Core, and JWT tokens.

## Features

### Authentication

- **User Registration**: Create new user accounts with email validation
- **User Login**: Authenticate users with email and password
- **JWT Token Generation**: Secure access tokens with configurable expiration
- **Refresh Token Support**: Long-lived refresh tokens for seamless user experience
- **Password Reset**: Forgot password and reset password functionality
- **Change Password**: Authenticated users can change their passwords
- **Logout**: Secure logout with token revocation

### Authorization

- **Role-based Access Control**: Support for multiple user roles (Admin, Manager, Supervisor, Staff, Member, User, Guest)
- **JWT Bearer Authentication**: Secure API endpoints with JWT token validation
- **Protected Endpoints**: Authentication required for sensitive operations

### Security Features

- **Password Hashing**: Secure password storage using ASP.NET Core Identity
- **Token Revocation**: Ability to revoke refresh tokens
- **Email Verification**: Password reset via email
- **Input Validation**: Comprehensive request validation
- **Error Handling**: Secure error responses without information leakage

## API Endpoints

### Authentication Endpoints

#### Register User

```
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

#### Login

```
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "rememberMe": false
}
```

#### Refresh Token

```
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "your_refresh_token_here"
}
```

#### Forgot Password

```
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Reset Password

```
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

#### Change Password (Protected)

```
POST /api/auth/change-password
Authorization: Bearer your_access_token_here
Content-Type: application/json

{
  "currentPassword": "CurrentPassword123!",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

#### Logout (Protected)

```
POST /api/auth/logout
Authorization: Bearer your_access_token_here
Content-Type: application/json

{
  "refreshToken": "your_refresh_token_here"
}
```

#### Revoke Token (Protected)

```
POST /api/auth/revoke-token
Authorization: Bearer your_access_token_here
Content-Type: application/json

{
  "refreshToken": "your_refresh_token_here"
}
```

## Configuration

### JWT Settings

Configure JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "Fanda.UserManagement.Api",
    "Audience": "Fanda.UserManagement.Api",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  }
}
```

### Email Settings

Configure email settings for password reset functionality:

```json
{
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

### Database Connection

Set the `USERDB_CONNECTION` environment variable with your PostgreSQL connection string.

## User Roles

The system supports the following roles:

- **Admin**: Full system access
- **Manager**: Management-level access
- **Supervisor**: Supervisory access
- **Staff**: Staff-level access
- **Member**: Member access
- **User**: Basic user access
- **Guest**: Limited guest access

## Security Considerations

1. **JWT Secret Key**: Use a strong, randomly generated secret key (at least 32 characters)
2. **HTTPS**: Always use HTTPS in production
3. **Token Expiration**: Configure appropriate token expiration times
4. **Email Security**: Use secure SMTP settings and app passwords
5. **Password Policy**: Implement strong password requirements
6. **Rate Limiting**: Consider implementing rate limiting for authentication endpoints

## Getting Started

1. **Install Dependencies**:

   ```bash
   dotnet restore
   ```

2. **Configure Database**:

   - Set up PostgreSQL database
   - Set `USERDB_CONNECTION` environment variable
   - Run migrations: `dotnet ef database update`

3. **Configure Settings**:

   - Update `appsettings.json` with your JWT and email settings
   - Generate a secure JWT secret key

4. **Run the Application**:

   ```bash
   dotnet run
   ```

5. **Test the API**:
   - Use the provided `Fanda.UserManagement.Api.http` file
   - Or use tools like Postman or curl

## Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    /* response data */
  },
  "errors": []
}
```

## Error Handling

The API provides detailed error responses for:

- Validation errors
- Authentication failures
- Authorization errors
- Server errors

Error responses include appropriate HTTP status codes and descriptive messages.

## Development

### Adding New Features

1. Create DTOs in the `Dto` folder
2. Implement services in the `Services` folder
3. Create endpoints in the `Endpoints` folder
4. Update the database context if needed
5. Create and run migrations

### Testing

Use the provided HTTP file or create unit tests for comprehensive testing.

## License

This project is part of the Fanda organization.

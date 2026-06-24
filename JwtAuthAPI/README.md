# JWT Authentication API (.NET 9)

M?t h? th?ng xác th?c JWT hoàn ch?nh s? d?ng ASP.NET Core 9, SQL Server, và Razor Pages.

## ?? Architecture

```
???????????????????????????????????????????????????????????????
?                    Frontend (Razor Pages)                    ?
?  - Login/Register Pages                                      ?
?  - Dashboard (Protected)                                     ?
?  - Cookie Authentication + Session Management               ?
???????????????????????????????????????????????????????????????
                 ? HTTP/HTTPS
                 ?
???????????????????????????????????????????????????????????????
?              Backend API (JwtAuthAPI)                        ?
?  ????????????????????????????????????????????????????????   ?
?  ? AuthorizeController (Public)                         ?   ?
?  ? - POST /api/authorize/register                       ?   ?
?  ? - POST /api/authorize/login                          ?   ?
?  ? - POST /api/authorize/refresh                        ?   ?
?  ? - POST /api/authorize/revoke                         ?   ?
?  ????????????????????????????????????????????????????????   ?
?  ????????????????????????????????????????????????????????   ?
?  ? ProductController (Protected - [Authorize])          ?   ?
?  ? - GET /api/product/list                              ?   ?
?  ? - GET /api/product/{id}                              ?   ?
?  ? - GET /api/product/admin-only ([Authorize(Admin)])   ?   ?
?  ????????????????????????????????????????????????????????   ?
?  ????????????????????????????????????????????????????????   ?
?  ? Services                                             ?   ?
?  ? - TokenService (JWT generation & validation)         ?   ?
?  ? - ApplicationDbContext (EF Core)                      ?   ?
?  ????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????
                 ?
                 ?
         ????????????????
         ?  SQL Server  ?
         ?  (LocalDB)   ?
         ????????????????
```

## ?? Security Features

### 1. **Password Hashing**
- ? HMACSHA512 with salt
- ? Unique salt per user
- ? Constant-time comparison to prevent timing attacks

### 2. **JWT Token Management**
- ? Access Token: 15 minutes (short-lived)
- ? Refresh Token: 7 days (long-lived)
- ? Token rotation on refresh (revoke old, generate new)
- ? Claims include: userId (sub), username, role
- ? Signature validation (HMAC256)

### 3. **HttpOnly Cookies**
- ? RefreshToken stored in HttpOnly, Secure, SameSite cookie
- ? Cannot be accessed from JavaScript (prevents XSS)
- ? Auto-sent by browser with credentials
- ? Deleted on logout

### 4. **CORS**
- ? Allows frontend from `http://localhost:5273` and `https://localhost:7273`
- ? Credentials enabled for cookie transmission

## ??? Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARBINARY(MAX) NOT NULL,
    PasswordSalt VARBINARY(MAX) NOT NULL,
    Role NVARCHAR(50)
);
```

### RefreshTokens Table
```sql
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY,
    Token NVARCHAR(MAX) NOT NULL,
    TokenId UNIQUEIDENTIFIER NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT NOT NULL,
    UserId INT FOREIGN KEY REFERENCES Users(Id)
);
```

## ?? Getting Started

### 1. Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### 2. Database Setup
```powershell
cd JwtAuthAPI
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run Backend API
```powershell
cd JwtAuthAPI
dotnet run
# Runs at: http://localhost:5000 or https://localhost:5001
# Swagger UI: http://localhost:5000 (Development)
```

### 4. Configuration
Update `appsettings.json` if needed:
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JwtAuthDb;Trusted_Connection=True;MultipleActiveResultSets=true"
    },
    "Jwt": {
        "SecretKey": "MoChuoiBiMatSieuDaiVaAnToanNenCoTren32KyTu123!@#$%^&*()",
        "Issuer": "JwtAuthAPI",
        "Audience": "JwtAuthUsers",
        "AccessTokenExpiryMinutes": 15,
        "RefreshTokenExpiryDays": 7
    }
}
```

> ?? **Important**: Change `SecretKey` on production! Use environment variables or Azure Key Vault.

## ?? API Endpoints

### Authorization Endpoints (Public)

#### 1. Register User
```http
POST /api/authorize/register
Content-Type: application/json

{
    "username": "admin",
    "password": "P@ssw0rd123",
    "role": "Admin"
}

Response: 200 OK
{
    "message": "User registered successfully"
}
```

#### 2. Login
```http
POST /api/authorize/login
Content-Type: application/json

{
    "username": "admin",
    "password": "P@ssw0rd123"
}

Response: 200 OK (with HttpOnly cookie)
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshTokenId": "550e8400-e29b-41d4-a716-446655440000"
}

Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict; Path=/
```

#### 3. Refresh Token
```http
POST /api/authorize/refresh
Content-Type: application/json

{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "..." (from body or cookie)
}

Response: 200 OK
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshTokenId": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### 4. Logout (Revoke)
```http
POST /api/authorize/revoke
Authorization: Bearer <accessToken>

Response: 200 OK
{
    "message": "Logged out successfully"
}
```

### Protected Endpoints (Require Authorization)

#### 1. Get Products
```http
GET /api/product/list
Authorization: Bearer <accessToken>

Response: 200 OK
{
    "message": "Hello admin! Here are your products.",
    "products": [
        { "id": 1, "name": "Product 1", "price": 100 },
        { "id": 2, "name": "Product 2", "price": 200 },
        { "id": 3, "name": "Product 3", "price": 300 }
    ]
}
```

#### 2. Get Product by ID
```http
GET /api/product/1
Authorization: Bearer <accessToken>

Response: 200 OK
{
    "message": "Product retrieved for user 1",
    "product": { "id": 1, "name": "Product 1", "price": 100 }
}
```

#### 3. Admin Only
```http
GET /api/product/admin-only
Authorization: Bearer <accessToken>

Response: 200 OK (if user has Admin role)
{
    "message": "This is admin-only content"
}

Response: 403 Forbidden (if user doesn't have Admin role)
```

## ?? Testing with Postman

### 1. Register
- **Method**: POST
- **URL**: `http://localhost:5000/api/authorize/register`
- **Body** (JSON):
  ```json
  {
      "username": "testuser",
      "password": "Test@123456",
      "role": "User"
  }
  ```

### 2. Login
- **Method**: POST
- **URL**: `http://localhost:5000/api/authorize/login`
- **Body** (JSON):
  ```json
  {
      "username": "testuser",
      "password": "Test@123456"
  }
  ```
- Copy the `accessToken` value

### 3. Call Protected API
- **Method**: GET
- **URL**: `http://localhost:5000/api/product/list`
- **Headers**:
  - `Authorization`: `Bearer <paste-accessToken-here>`

## ?? JWT Token Flow

```
1. User Login
   ?? Send username + password
   ?? Verify credentials (HMACSHA512 hash comparison)
   ?? Generate accessToken (15 min)
   ?? Generate refreshToken (7 days)
   ?? Return accessToken + set HttpOnly cookie

2. Access Protected Resource
   ?? Send GET /api/product/list
   ?? Header: Authorization: Bearer <accessToken>
   ?? Middleware validates JWT signature + expiry
   ?? Extract claims (userId, username, role)
   ?? Return protected resource

3. Access Token Expires (after 15 min)
   ?? Client detects 401 Unauthorized
   ?? Send POST /api/authorize/refresh
   ?? Validate refreshToken from cookie
   ?? Revoke old refreshToken (set IsRevoked = true)
   ?? Generate new accessToken + new refreshToken
   ?? Continue with new accessToken

4. Logout (Optional)
   ?? Send POST /api/authorize/revoke
   ?? Revoke refreshToken (set IsRevoked = true)
   ?? Delete refreshToken cookie
   ?? Redirect to login page
```

## ?? Code Examples

### Validate Claims
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var username = User.FindFirst(ClaimTypes.Name)?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
```

### Require Authorization
```csharp
[Authorize]
public IActionResult ProtectedEndpoint()
{
    return Ok("Only authenticated users can access this");
}
```

### Require Specific Role
```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    return Ok("Only admins can access this");
}
```

## ?? Configuration Files

### appsettings.json
- Database connection string
- JWT secret key, issuer, audience
- Token expiry times

### Program.cs
- DbContext registration
- JWT authentication setup
- CORS configuration
- Swagger/OpenAPI setup

## ?? Troubleshooting

### "Access to XMLHttpRequest denied by CORS policy"
- **Solution**: Ensure frontend URL is in CORS AllowedOrigins

### "Invalid token" / "Invalid signature"
- **Solution**: Verify same SecretKey is used in frontend validation

### "Refresh token missing"
- **Solution**: Ensure cookies are being sent (check CORS AllowCredentials = true)

### "User not found after login"
- **Solution**: Run `dotnet ef database update` to create tables

## ?? References

- [JWT Official Site](https://jwt.io/)
- [OWASP - Authentication](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Microsoft - ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [ASP.NET Core JWT Guide](https://code-maze.com/aspnetcore-jwt-authentication/)

## ?? License

This project is licensed under the MIT License.

## ? Checklist for Production

- [ ] Change `Jwt:SecretKey` to a strong random value (minimum 64 characters)
- [ ] Store secrets in Azure Key Vault or environment variables
- [ ] Enable HTTPS only (`app.UseHttpsRedirection()`)
- [ ] Set `Secure = true` for cookies
- [ ] Implement rate limiting for login attempts
- [ ] Add logging and monitoring
- [ ] Regular security audits and updates
- [ ] Database backups enabled
- [ ] GDPR compliance (data retention policy)
- [ ] Implement 2FA (Two-Factor Authentication)

---

**Version**: 1.0  
**Last Updated**: 2026-01-XX  
**Author**: Your Team

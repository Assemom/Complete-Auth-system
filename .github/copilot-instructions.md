# ASP.NET Core Web API Starter Template — Copilot Instructions

## How to Use This File

- This file is saved at `.github/copilot-instructions.md` — Copilot reads it automatically as project context.
- Implement **one phase at a time**. Do NOT move to the next phase until the current one is complete.
- Use Copilot Chat (Agent mode) with prompts like:
  > "Implement Phase 1 only. Follow all rules in `.github/copilot-instructions.md` exactly."
- After each phase, verify with:
  > "Review Phase N output. Does it match the blueprint? List any deviations."

---

# Project Goal

Build a production-ready, reusable ASP.NET Core Web API starter template.
Scalable. Maintainable. Reusable for future API projects.

---

# Technology Stack

| Technology | Version |
|---|---|
| .NET | 10 (latest stable) |
| ASP.NET Core Identity | Included in .NET 10 |
| Entity Framework Core | 10.x |
| SQL Server | Latest |
| Serilog | 4.x |
| FluentValidation | 11.x |
| AutoMapper | 13.x |
| Swashbuckle (Swagger) | 6.x |
| Asp.Versioning.Mvc | 8.x |
| Google.Apis.Auth | Latest stable |
| xunit (Testing) | Latest stable |

---

# Development Rules

## General Rules

- Use clean architecture principles inside 3-tier architecture.
- Use SOLID principles throughout all layers.
- Use `async`/`await` everywhere — no synchronous database or I/O calls.
- Use dependency injection — no `new` keyword for services.
- Controllers must stay thin — no business logic.
- Business logic belongs exclusively in services.
- Data access logic belongs exclusively in repositories.
- Use interfaces for all abstractions.
- Use clean, consistent naming conventions.
- Production-ready code only — no `TODO`, no placeholder, no dummy logic.
- No hardcoded strings — use constants or configuration.
- No magic numbers — use named constants.
- Return `Task` or `Task<T>` from all async methods.

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Classes | PascalCase | `AuthService` |
| Interfaces | `I` + PascalCase | `IAuthService` |
| Methods | PascalCase | `GetUserByIdAsync` |
| Properties | PascalCase | `CreatedAt` |
| Private fields | `_` + camelCase | `_unitOfWork` |
| DTOs | PascalCase + `Dto` | `RegisterDto` |
| Controllers | PascalCase + `Controller` | `AuthController` |
| Validators | PascalCase + `Validator` | `RegisterDtoValidator` |
| Profiles | PascalCase + `Profile` | `AuthProfile` |

---

# Solution Structure

```
ProjectName.sln

src/
├── API/
│   ├── Controllers/
│   ├── Middlewares/
│   └── Extensions/
├── Business/
│   ├── Services/
│   ├── Interfaces/
│   ├── Validators/
│   └── Mappings/
├── DataAccess/
│   ├── Context/
│   ├── Repositories/
│   ├── UnitOfWork/
│   ├── Configurations/
│   └── Migrations/
├── Domain/
│   ├── Entities/
│   ├── DTOs/
│   └── Enums/
├── Infrastructure/
│   ├── Email/
│   └── ExternalProviders/
└── Shared/
    ├── Responses/
    ├── Exceptions/
    ├── Helpers/
    └── Constants/
```

---

# Layer Responsibilities

## API Layer

- Controllers (thin — delegate to services only)
- Middlewares (exception handling, logging)
- Extension methods for DI registration
- Versioning configuration
- Swagger configuration

## Business Layer

- Services (all business logic lives here)
- Service interfaces
- FluentValidation validators
- AutoMapper profiles
- Authentication and authorization logic

## DataAccess Layer

- `ApplicationDbContext`
- Generic and specific repositories
- Unit of Work
- Fluent API entity configurations
- EF Core migrations

## Domain Layer

- Entities (POCOs — no logic)
- DTOs (data transfer objects — no logic)
- Enums

## Infrastructure Layer

- Email service (SMTP)
- Google OAuth provider

## Shared Layer

- `ApiResponse<T>` wrapper
- Custom exception classes
- Helper utilities
- Global constants

---

# appsettings.json Structure

Use this exact structure. Bind to strongly typed settings classes.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ProjectDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Secret": "",
    "Issuer": "",
    "Audience": "",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "EmailSettings": {
    "Host": "",
    "Port": 587,
    "Email": "",
    "Password": "",
    "DisplayName": ""
  },
  "GoogleAuthSettings": {
    "ClientId": "",
    "ClientSecret": ""
  },
  "SeedSettings": {
    "AdminEmail": "",
    "AdminPassword": ""
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

---

# Exception Hierarchy

All exceptions live in `Shared/Exceptions/`.

```
AppException (base — abstract)
├── ValidationException        → HTTP 400
├── UnauthorizedException      → HTTP 401
├── ForbiddenException         → HTTP 403
├── NotFoundException          → HTTP 404
├── ConflictException          → HTTP 409
└── ServerException            → HTTP 500
```

All exceptions carry:
- `Message` (string)
- `StatusCode` (int)

The global exception middleware maps these to `ErrorResponse`.

---

# Implementation Phases

Follow this exact order. Do not skip or reorder phases.

---

## Phase 1 — Domain Layer

### BaseEntity

```
Id            : Guid
CreatedAt     : DateTime
UpdatedAt     : DateTime?
CreatedBy     : string?
UpdatedBy     : string?
IsDeleted     : bool
DeletedAt     : DateTime?
```

### Entities

**ApplicationUser : IdentityUser**
- No extra properties needed unless required by auth flows.

**RefreshToken**

```
Id         : Guid
Token      : string
ExpiresAt  : DateTime
IsRevoked  : bool
UserId     : string
User       : ApplicationUser  (navigation)
```

**EmailOtp**

```
Id         : Guid
Code       : string
ExpiresAt  : DateTime
IsUsed     : bool
UserId     : string
User       : ApplicationUser  (navigation)
```

### DTOs

**Auth DTOs:**

```
RegisterDto          → Email, Password, ConfirmPassword, FirstName, LastName
LoginDto             → Email, Password
AuthResponseDto      → AccessToken, RefreshToken, ExpiresAt, Email, Roles[]
RefreshTokenDto      → RefreshToken
ForgotPasswordDto    → Email
ResetPasswordDto     → Email, Token, NewPassword, ConfirmNewPassword
ConfirmEmailDto      → UserId, Code
ResendConfirmationDto → Email
ChangePasswordDto    → CurrentPassword, NewPassword, ConfirmNewPassword
GoogleLoginDto       → IdToken
```

### Enums

```csharp
public enum Roles
{
    Admin,
    User
}
```

---

## Phase 2 — DataAccess Layer

### ApplicationDbContext

- Inherit from `IdentityDbContext<ApplicationUser>`
- DbSets: `RefreshTokens`, `EmailOtps`
- Apply all Fluent API configurations via `modelBuilder.ApplyConfigurationsFromAssembly`
- Apply global soft delete query filter: `.HasQueryFilter(e => !e.IsDeleted)` on all `BaseEntity` types
- Override `SaveChangesAsync()`:
  - Auto-set `CreatedAt` on new entities
  - Auto-set `UpdatedAt` on modified entities
  - Handle soft delete: intercept `Deleted` state → set `IsDeleted = true`, `DeletedAt = UtcNow`, change state to `Modified`

### Generic Repository

**Interface: `IGenericRepository<T>`**

```
GetAllAsync(Expression<Func<T, bool>>? filter, int page, int pageSize)  → Task<IEnumerable<T>>
GetByIdAsync(Guid id)                                                    → Task<T?>
AddAsync(T entity)                                                       → Task
UpdateAsync(T entity)                                                    → Task
DeleteAsync(T entity)                                                    → Task  (soft delete)
FindAsync(Expression<Func<T, bool>> predicate)                          → Task<T?>
ExistsAsync(Expression<Func<T, bool>> predicate)                        → Task<bool>
CountAsync(Expression<Func<T, bool>>? filter)                           → Task<int>
```

### Specific Repositories

**IAuthRepository / AuthRepository**
- `GetRefreshTokenAsync(string token)` → `Task<RefreshToken?>`
- `RevokeAllUserTokensAsync(string userId)` → `Task`
- `AddRefreshTokenAsync(RefreshToken token)` → `Task`

**IUserRepository / UserRepository**
- `GetByEmailAsync(string email)` → `Task<ApplicationUser?>`
- `GetOtpAsync(string userId, string code)` → `Task<EmailOtp?>`
- `AddOtpAsync(EmailOtp otp)` → `Task`
- `InvalidateUserOtpsAsync(string userId)` → `Task`

### Unit of Work

**Interface: `IUnitOfWork`**

```
IAuthRepository Auth  { get; }
IUserRepository Users { get; }
SaveChangesAsync()    → Task<int>
```

**Implementation: `UnitOfWork`**
- Inject `ApplicationDbContext`
- Expose repositories as lazy-initialized properties
- Implement `SaveChangesAsync`

### Fluent Configurations

Create separate configuration class per entity in `DataAccess/Configurations/`:

- `RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>`
- `EmailOtpConfiguration : IEntityTypeConfiguration<EmailOtp>`
- `ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>`

Configure:
- Primary keys
- Required fields
- Max lengths
- Indexes (e.g., unique index on `Token` for `RefreshToken`)
- Foreign key relationships

### Soft Delete Rules

- All entities extending `BaseEntity` have the global query filter applied automatically.
- `DeleteAsync` NEVER issues a SQL DELETE. It sets `IsDeleted = true` and `DeletedAt = UtcNow`.
- Hard delete is NEVER used anywhere in the codebase.
- Soft-deleted records are invisible to all queries automatically.

---

## Phase 3 — Authentication System

### ASP.NET Core Identity Setup

Use:
- `UserManager<ApplicationUser>`
- `RoleManager<IdentityRole>`
- `SignInManager<ApplicationUser>`

### Features to Implement

| Feature | Notes |
|---|---|
| Register | Hash password, create user, assign User role, trigger OTP email |
| Login | Validate credentials, check email confirmed, check lockout, return JWT + refresh token |
| Refresh Token | Validate token, rotate (issue new, revoke old), return new pair |
| Logout | Revoke all user refresh tokens |
| Change Password | Validate current password, update, revoke all tokens |
| Forgot Password | Generate Identity reset token, build reset link, send email |
| Reset Password | Validate Identity token, reset password |
| Email Confirmation | Validate OTP code, mark email confirmed |
| Resend Confirmation | Invalidate old OTPs, generate new OTP, send email |
| Google Login | Validate Google ID token, create user if not exists, link if email exists, return JWT |

### JWT Rules

- Sign with HMAC-SHA256 using secret from `JwtSettings.Secret`.
- Claims: `sub` (userId), `email`, `jti` (unique token id), `roles`.
- Access token expiry: from `JwtSettings.ExpiryMinutes`.
- Refresh token: cryptographically random string (use `RandomNumberGenerator`).
- Store refresh token in database.
- Refresh token expiry: from `JwtSettings.RefreshTokenExpiryDays`.

### Refresh Token Rules

- One active token per user is allowed (revoke old on rotation).
- On every refresh: issue new access + refresh token, revoke old refresh token immediately.
- On logout: revoke ALL user refresh tokens.
- On password change: revoke ALL user refresh tokens.
- Expired or revoked tokens return `UnauthorizedException`.

---

## Phase 4 — Authorization

### Roles

- `Admin`
- `User`

Roles are defined in `Roles` enum. Seeded on startup.

### Policies

```csharp
options.AddPolicy("RequireAdmin", policy => policy.RequireRole(nameof(Roles.Admin)));
options.AddPolicy("RequireUser", policy => policy.RequireRole(nameof(Roles.User)));
```

Apply via `[Authorize(Policy = "RequireAdmin")]` on controllers/endpoints.

---

## Phase 5 — Email Confirmation (OTP)

**This system is for email confirmation ONLY. Do NOT use OTP for password reset.**

### Flow

```
Register → Generate 6-digit OTP → Store in EmailOtps table → Send email → User submits OTP → Mark confirmed
```

### OTP Rules

- 6-digit numeric code.
- Expiry: 10 minutes.
- One-time use: mark `IsUsed = true` on successful confirmation.
- On resend: invalidate all existing OTPs for user, generate new one.
- If OTP expired or already used → return `ValidationException`.

---

## Phase 6 — Forgot Password

**Use ASP.NET Core Identity built-in token provider. Do NOT use OTP here.**

### Flow

```
POST /forgot-password (email)
→ Generate token via UserManager.GeneratePasswordResetTokenAsync
→ URL-encode token
→ Build reset link: {FrontendBaseUrl}/reset-password?email={email}&token={token}
→ Send email with reset link

POST /reset-password (email, token, newPassword)
→ Validate token via UserManager.ResetPasswordAsync
→ If invalid/expired → ValidationException
→ On success → revoke all refresh tokens
```

### Notes

- Frontend base URL comes from configuration — not hardcoded.
- Token expiry is controlled by Identity's `DataProtectionTokenProviderOptions`.

---

## Phase 7 — Google Authentication

### Flow

```
POST /google-login (idToken)
→ Validate Google ID token using Google.Apis.Auth
→ Extract email, name, googleId
→ If user exists with email → link Google login, issue JWT
→ If user does not exist → create account (auto-confirm email), assign User role, issue JWT
→ Return AuthResponseDto
```

### Rules

- Use `GoogleJsonWebSignature.ValidateAsync` for token validation.
- `ClientId` from `GoogleAuthSettings.ClientId`.
- Auto-confirmed email — no OTP needed for Google signups.

---

## Phase 8 — Infrastructure: Email Service

### Interface: `IEmailService`

```csharp
Task SendEmailAsync(string to, string subject, string htmlBody);
Task SendEmailConfirmationAsync(string to, string otp);
Task SendPasswordResetAsync(string to, string resetLink);
```

### Implementation: `EmailService`

- SMTP-based using `System.Net.Mail.SmtpClient`.
- Settings bound from `EmailSettings`.
- HTML templates (inline, no external files needed):
  - Email Confirmation: shows OTP code, expiry time, branded layout.
  - Password Reset: shows clickable reset link, expiry notice, branded layout.

### Rules

- Never throw raw SMTP exceptions — wrap in `ServerException`.
- Log all email send attempts (success and failure).

---

## Phase 9 — Validation

Use FluentValidation 11.x.

### Validators to Create

| Validator | Rules |
|---|---|
| `RegisterDtoValidator` | Email format, password min 8 chars + uppercase + number + special, passwords match, names not empty |
| `LoginDtoValidator` | Email format, password not empty |
| `ResetPasswordDtoValidator` | Email format, new password policy, passwords match, token not empty |
| `ConfirmEmailDtoValidator` | UserId not empty, code is 6 digits |
| `ChangePasswordDtoValidator` | Current password not empty, new password policy, passwords match |
| `ForgotPasswordDtoValidator` | Email format |

### Registration

Register all validators via `AddValidatorsFromAssembly` in `AddBusiness()` extension.
Integrate with pipeline via `AddFluentValidationAutoValidation()`.

### Password Policy (used in validators AND Identity config)

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character

---

## Phase 10 — AutoMapper

Use AutoMapper 13.x.

### Profiles

**`AuthProfile`**

```
RegisterDto → ApplicationUser  (map FirstName, LastName, Email, set UserName = Email)
ApplicationUser → UserDto       (if needed)
```

### Rules

- All profiles registered via `AddAutoMapper(typeof(AuthProfile).Assembly)` in `AddBusiness()`.
- Never map passwords in profiles — handle manually in services.

---

## Phase 11 — API Response Wrapper

### `ApiResponse<T>`

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") { ... }
    public static ApiResponse<T> Fail(string message) { ... }
}
```

### `ErrorResponse`

```csharp
public class ErrorResponse
{
    public bool Success => false;
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
```

### Rules

- ALL controller endpoints return `ApiResponse<T>`.
- ALL error responses use `ErrorResponse` (returned by exception middleware).
- Never return raw objects from controllers.

---

## Phase 12 — Global Exception Middleware

Create `ExceptionMiddleware` in `API/Middlewares/`.

### Mapping

| Exception Type | HTTP Status | Log Level |
|---|---|---|
| `ValidationException` | 400 | Warning |
| `UnauthorizedException` | 401 | Warning |
| `ForbiddenException` | 403 | Warning |
| `NotFoundException` | 404 | Warning |
| `ConflictException` | 409 | Warning |
| `ServerException` | 500 | Error |
| Unhandled `Exception` | 500 | Error |

### Rules

- Return `ErrorResponse` JSON for all exceptions.
- Set `Content-Type: application/json`.
- Log all exceptions with structured context (request path, status code, message).
- Never expose stack traces in production (`IHostEnvironment.IsProduction()`).

---

## Phase 13 — Logging

Use Serilog 4.x.

### Configuration

- Read from `appsettings.json` `Serilog` section (see configuration above).
- Log to Console (Development) and rolling File (all environments).
- File path: `logs/log-{Date}.txt`.

### What to Log

| Event | Level |
|---|---|
| Application start/stop | Information |
| Register attempt | Information |
| Login success | Information |
| Login failure | Warning |
| Token refresh | Information |
| Password reset | Information |
| Email sent | Information |
| Email send failure | Error |
| Unhandled exception | Error |
| Validation failure | Warning |

### Rules

- Use structured logging: `Log.Information("User {Email} logged in", email)`.
- Never log passwords, tokens, or sensitive data.

---

## Phase 14 — API Versioning

Use `Asp.Versioning.Mvc` 8.x.

### Format

```
/api/v1/auth/login
/api/v2/auth/login  (future)
```

### Configuration

```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### Rules

- All controllers decorated with `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/[controller]")]`.
- Swagger must show each version separately.

---

## Phase 15 — Swagger

Configure Swagger to support:

- JWT Bearer authentication (Authorization header).
- API versioning (separate docs per version).

```csharp
// Swagger security definition
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Enter your JWT token."
});
```

- Swagger UI available only in Development (`app.UseSwagger()` inside `if (app.Environment.IsDevelopment())`).

---

## Phase 16 — Strongly Typed Configuration

Create in `Shared/Settings/` or bind in each layer as appropriate.

### Classes

```csharp
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class GoogleAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class SeedSettings
{
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
```

### Binding

```csharp
services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
services.Configure<GoogleAuthSettings>(configuration.GetSection(nameof(GoogleAuthSettings)));
services.Configure<SeedSettings>(configuration.GetSection(nameof(SeedSettings)));
```

Inject via `IOptions<T>` — never inject `IConfiguration` directly into services.

---

## Phase 17 — Security

### Identity Configuration

```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequireNonAlphanumeric = true;
options.Password.RequiredLength = 8;

options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.AllowedForNewUsers = true;

options.User.RequireUniqueEmail = true;
options.SignIn.RequireConfirmedEmail = true;
```

### Token Security Rules

- JWT secret minimum 32 characters — validated on startup.
- Refresh tokens generated using `RandomNumberGenerator.GetBytes(64)` → Base64.
- Password reset tokens use Identity's built-in `DataProtectorTokenProvider`.
- All tokens are single-use.

---

## Phase 18 — Seed Data

Implement in `DataAccess/Seeders/`.

### RoleSeeder

```csharp
public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
```

Seeds: `Admin`, `User` roles (from `Roles` enum).

### AdminSeeder

```csharp
public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, SeedSettings settings)
```

- Creates admin account using credentials from `SeedSettings` (from `appsettings.json`).
- Assigns `Admin` role.
- Marks email as confirmed.
- Skips if admin already exists.
- NEVER hardcode credentials.

### Execution

Call seeders in `Program.cs` after `app.Build()`, before `app.Run()`:

```csharp
using var scope = app.Services.CreateScope();
await RoleSeeder.SeedRolesAsync(...);
await AdminSeeder.SeedAdminAsync(...);
```

---

## Phase 19 — AuthController

Route: `api/v{version:apiVersion}/auth`

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| POST | `/register` | No | Register new user |
| POST | `/login` | No | Login, returns JWT + refresh token |
| POST | `/refresh-token` | No | Rotate refresh token |
| POST | `/logout` | Yes | Revoke all user refresh tokens |
| POST | `/confirm-email` | No | Confirm email via OTP |
| POST | `/resend-confirmation` | No | Resend OTP email |
| POST | `/forgot-password` | No | Send password reset email |
| POST | `/reset-password` | No | Reset password with Identity token |
| POST | `/google-login` | No | Login/signup via Google |
| POST | `/change-password` | Yes | Change password (revokes all tokens) |

### Rules

- Controllers ONLY call service methods.
- No business logic, no direct repository calls, no direct DbContext usage.
- Always return `ApiResponse<T>`.
- Apply `[Authorize]` where required.
- Apply `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/[controller]")]`.

---

## Phase 20 — Dependency Injection

Use extension methods for clean `Program.cs`.

### Extension Methods

**`AddDataAccess(IConfiguration configuration)`**

Registers:
- `ApplicationDbContext` (SQL Server)
- `IGenericRepository<>` → `GenericRepository<>`
- `IAuthRepository` → `AuthRepository`
- `IUserRepository` → `UserRepository`
- `IUnitOfWork` → `UnitOfWork`

**`AddBusiness()`**

Registers:
- `IAuthService` → `AuthService`
- `IJwtService` → `JwtService`
- AutoMapper profiles
- FluentValidation validators (assembly scan)
- FluentValidation pipeline integration

**`AddInfrastructure(IConfiguration configuration)`**

Registers:
- `IEmailService` → `EmailService`

**`AddIdentityConfiguration()`**

Registers:
- ASP.NET Core Identity with password/lockout/user settings
- Roles support (`IdentityRole`)

**`AddJwtAuthentication(IConfiguration configuration)`**

Registers:
- JWT Bearer authentication
- Token validation parameters

**`AddSwaggerConfiguration()`**

Registers:
- Swagger gen with JWT support and versioning

**`AddSerilogConfiguration(IConfiguration configuration)`**

Configures:
- Serilog from configuration

---

## Phase 21 — Program.cs

```csharp
// Builder
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(...);

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusiness();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiVersioning(...);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// App
var app = builder.Build();

// Seed data
await SeedData(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(...);
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Middleware Pipeline Order

This order is REQUIRED — do not change it:

1. `ExceptionMiddleware`
2. `UseSerilogRequestLogging`
3. `UseHttpsRedirection`
4. `UseAuthentication`
5. `UseAuthorization`
6. `MapControllers`

---

# Final Checklist

Before marking the project complete, verify every item:

- [ ] Full solution structure matches blueprint
- [ ] Soft delete works (no hard deletes anywhere)
- [ ] Audit fields auto-set in `SaveChangesAsync`
- [ ] JWT issued and validated correctly
- [ ] Refresh token rotation works (old revoked, new issued)
- [ ] All tokens revoked on logout
- [ ] All tokens revoked on password change
- [ ] Email OTP confirmation works end-to-end
- [ ] OTP NOT used for password reset (Identity token used instead)
- [ ] Password reset flow works end-to-end
- [ ] Google login creates or links account correctly
- [ ] FluentValidation runs in pipeline automatically
- [ ] All endpoints return `ApiResponse<T>`
- [ ] All errors return `ErrorResponse` via middleware
- [ ] Serilog logs to file
- [ ] Sensitive data never logged
- [ ] Swagger shows JWT auth and versioning
- [ ] URL versioning works (`/api/v1/...`)
- [ ] Roles seeded on startup
- [ ] Default admin seeded from config (not hardcoded)
- [ ] All settings bound via `IOptions<T>`
- [ ] Password policy consistent between Identity config and FluentValidation
- [ ] Account lockout configured (5 attempts, 5 min)
- [ ] Email confirmation required before login
- [ ] No business logic in controllers
- [ ] No direct DbContext usage outside DataAccess layer
- [ ] No `new` keyword for services (all via DI)

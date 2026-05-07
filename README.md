# ASP.NET Core Web API Starter Template (.NET 10)

Production-ready ASP.NET Core Web API starter template featuring Identity, JWT + refresh token rotation, email confirmation via OTP, password reset, Google login, structured logging, API versioning, and Swagger.

---

## Features

- ASP.NET Core Identity with secure password policy
- JWT authentication + refresh token rotation
- Email confirmation via 6‑digit OTP
- Password reset (Identity tokens)
- Google login
- Soft delete and audit fields
- Serilog logging (console + rolling file)
- API versioning with Swagger
- Clean 3‑tier architecture

---

## Tech Stack

| Technology | Version |
|---|---|
| .NET | 10 |
| ASP.NET Core Identity | 10.x |
| Entity Framework Core | 10.x |
| SQL Server | Latest |
| Serilog | 4.x |
| FluentValidation | 11.x |
| AutoMapper | 13.x |
| Swashbuckle | 6.x |
| Asp.Versioning.Mvc | 8.x |
| Google.Apis.Auth | Latest stable |

---

## Solution Structure



2. **Configure**
Update `src/App.Api/appsettings.json`:
- `ConnectionStrings:DefaultConnection`
- `JwtSettings`
- `EmailSettings`
- `GoogleAuthSettings`
- `SeedSettings`
- `FrontendSettings:BaseUrl`

3. **Run migrations**




---

## Configuration (appsettings.json)

Required sections:

- `ConnectionStrings`
- `JwtSettings`
- `EmailSettings`
- `GoogleAuthSettings`
- `SeedSettings`
- `FrontendSettings`
- `Serilog`

---

## Authentication Endpoints (v1)

Base URL (local): `https://localhost:7281`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/v1/auth/register` | Register |
| POST | `/api/v1/auth/login` | Login |
| POST | `/api/v1/auth/refresh-token` | Refresh token |
| POST | `/api/v1/auth/logout` | Logout |
| POST | `/api/v1/auth/confirm-email` | Confirm email (OTP) |
| POST | `/api/v1/auth/resend-confirmation` | Resend OTP |
| POST | `/api/v1/auth/forgot-password` | Send reset link |
| POST | `/api/v1/auth/reset-password` | Reset password |
| POST | `/api/v1/auth/google-login` | Google login |
| POST | `/api/v1/auth/change-password` | Change password |

All responses are wrapped in `ApiResponse<T>`. Errors are returned as `ErrorResponse`.

---

## Seeded Admin

- Admin account is created from `SeedSettings` in `appsettings.json`.
- Seeding is skipped if `AdminEmail` or `AdminPassword` is empty.

---

## Documentation

See `DEVELOPER_GUIDE.md` for:

- Full configuration guide
- Architecture details
- Complete API request/response examples

---

## License

MIT (or your preferred license)





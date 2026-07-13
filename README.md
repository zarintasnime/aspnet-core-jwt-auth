# CoreAuthAndAuthUsingJWTToken

An ASP.NET Core 8 Web API demonstrating **Authentication & Authorization using JWT (JSON Web Tokens)**, built with:

- ASP.NET Core Identity (user & role management)
- Entity Framework Core (SQL Server / LocalDB)
- JWT Bearer authentication
- Role-based authorization (`Admin`, `User`)
- Swagger / OpenAPI (with a Bearer token "Authorize" button for testing protected endpoints)
- A separate **SharedLIbrary** class library for shared DTOs and models (used by both the API and, potentially, a client app)

## Project structure

```
CoreAuthAndAuthUsingJWTToken/
├── CoreAuthAndAuthUsingJWTToken/     # Main Web API project
│   ├── Controllers/                  # AccountsController, EmployeesController, WeatherForecastController
│   ├── Models/                       # ApplicationUser, AppDbContext
│   ├── Repositories/                 # IUserAccount / UserAccountRepositoty (register/login/JWT generation)
│   ├── Migrations/                   # EF Core migrations
│   └── Program.cs                    # App startup & service configuration
└── SharedLIbrary/                    # Shared DTOs & Models (UserDTO, LoginDTO, Employee, ServiceResponse, ...)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or SQL Server LocalDB (comes with Visual Studio) — or update the connection string to use another SQL Server instance / Docker container

## Setup & run

1. **Clone the repo**
   ```bash
   git clone <your-repo-url>
   cd CoreAuthAndAuthUsingJWTToken
   ```

2. **Update the connection string** in `CoreAuthAndAuthUsingJWTToken/appsettings.json` if you're not using LocalDB:
   ```json
   "ConnectionStrings": {
     "con": "Server=(LocalDB)\\MSSQLLocalDB;Database=Jwt68TokenDB;Trusted_Connection=true;TrustServerCertificate=True;"
   }
   ```

3. **Apply EF Core migrations** (creates the database):
   ```bash
   cd CoreAuthAndAuthUsingJWTToken
   dotnet ef database update
   ```
   (If `dotnet-ef` isn't installed: `dotnet tool install --global dotnet-ef`)

4. **Run the project**
   ```bash
   dotnet run
   ```
   Swagger UI opens automatically at `/swagger` (see `launchSettings.json` for the exact port).

## API overview

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/accounts/register` | POST | none | Register a new user. The **first** registered user automatically becomes `Admin`; everyone after that gets the `User` role. |
| `/api/accounts/login` | POST | none | Login with email & password, returns a JWT. |
| `/api/employees` | GET/POST/PUT/DELETE | `Admin` role | CRUD for employees (with image upload). |

To call a protected endpoint from Swagger: log in via /api/accounts/login, copy the returned token, click Authorize in Swagger UI, and paste the token directly (no need to write 'Bearer ').

## Notes / things to know before deploying this publicly

- The `Jwt:Key` and the SQL connection string in `appsettings.json` are placeholder/local-dev values checked into this demo repo for convenience. **For a real deployment**, move them out of source control (use `dotnet user-secrets`, environment variables, or a secrets manager) and use a long, randomly generated signing key.
- `wwwroot/Pictures` (created at runtime when uploading employee images) is git-ignored so uploaded files don't get committed.

## What was fixed before pushing to GitHub

- `UserAccountRepositoty.cs`: the duplicate-user check, the "did registration actually succeed" check, and — most importantly — the **password verification step on login** had been commented out. This meant *any* password would log a user in as long as the email existed. These checks have been restored.
- `Program.cs: removed a duplicated AddControllers() / Swagger-middleware block, re-enabled the Swagger "Authorize" button using standard HTTP Bearer scheme, and translated the box description to English.
- Added `.gitignore` so `bin/`, `obj/`, and `.vs/` (Visual Studio build & cache folders) are excluded from the repository.

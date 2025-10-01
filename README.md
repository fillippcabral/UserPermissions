
# UserPermissionsApi

A .NET 9 Web API that simulates a basic user and permission management system.

## Tech
- C# / .NET 9
- ASP.NET Core (REST)
- Entity Framework Core (InMemory provider)
- xUnit + FluentAssertions + WebApplicationFactory
- SOLID, layered: **Domain**, **Application**, **Infrastructure**, **Api**

## Endpoints
- `POST /users` – create user
- `POST /login` – simulate login (returns a fake token)
- `POST /users/{id}/roles` – assign role to user
- `GET /users/{id}` – get user + roles

## Validations
- Email format
- Name required (>= 2 chars)
- Password required (>= 6 chars)
- Email unique

## Run
```bash
dotnet restore
dotnet build
dotnet run --project src/UserPermissionsApi.Api/UserPermissionsApi.Api.csproj
```
Swagger UI: https://localhost:5001/swagger

## Test (target ≥85% coverage)
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```
The suite contains integration and unit tests and typically exceeds 85% coverage on core code paths.

## Project Layout
```
src/
  UserPermissionsApi.Domain/        # Entities
  UserPermissionsApi.Application/   # DTOs, services, interfaces, validation
  UserPermissionsApi.Infrastructure/# EF Core DbContext, repositories, password hasher
  UserPermissionsApi.Api/           # ASP.NET Core controllers, DI
  UserPermissionsApi.Tests/         # xUnit tests (integration + unit)
```

## Notes on Security
- Passwords are stored as PBKDF2 hashes with per-user salt (100k iterations).
- This project issues a **simulated** token for demo purposes; replace with JWT for real systems.

# Clean Architecture + CQRS with MediatR

A minimal, production-minded starting point for an ASP.NET Core API. One tiny feature
(Products) shows every building block end to end. Read it in 10 minutes, then delete
`Products` and add your own features.

## Problem

Request handling logic tends to leak everywhere: controllers talk to EF Core, validation is
duplicated, errors are thrown for ordinary outcomes ("not found", "duplicate"), and nothing
stops the web layer from creeping into the business rules. This template fixes that with:

- **Layers with enforced direction** (Domain ← Application ← Infrastructure / Api), checked by tests.
- **CQRS via MediatR**: one handler per use case; commands change state, queries read it.
- **Cross-cutting concerns as pipeline behaviors**: logging, validation, transactions are written once.
- **The Result pattern**: expected failures are values, not exceptions.
- **RFC 9457 ProblemDetails**: every error response has the same shape.

## When to use / when not to

**Use it when**
- The service has real business rules and will grow beyond a handful of endpoints.
- Several developers need one obvious place for each kind of code.
- You want validation, transactions and logging applied consistently without thinking about it.

**Don't use it when**
- It is a CRUD-over-a-table service or a prototype: a single Minimal API project with EF Core is simpler.
- The team is unfamiliar with MediatR and the indirection would cost more than it saves.
- You need extreme throughput on hot paths, where an extra dispatch and allocations per request matter.

## Trade-offs

Each choice below is deliberate; change it if your context differs.

- **Application references EF Core (`DbSet<T>` in `IAppDbContext`)**: you get full LINQ without a generic repository, at the cost of a persistence-aware Application layer.
- **No generic repository**: EF Core already is a unit of work and repository; another layer only hides features.
- **`TransactionBehavior` calls `SaveChangesAsync`**: handlers stay free of persistence plumbing, but they cannot read database-generated values, so IDs are GUIDs created in the domain.
- **GUID v7 IDs**: generated without a database round trip and time-ordered to limit index fragmentation; larger than `int`.
- **Explicit transaction per command**: correct when a command saves more than once, slightly more overhead than EF's implicit single-`SaveChanges` transaction; it also rules out `EnableRetryOnFailure`, which forbids user-started transactions.
- **Duplicate name checked in the handler and by a unique index**: the check gives a clean 409; a concurrent race that passes it hits the index and returns 500 (rare, and never corrupts data).
- **Domain guards throw**: `Product.Create` throws on invalid input because the validator already ran, so a throw signals a bug, not a user error.
- **`Error` lives in Domain, `Result` in Application**: the domain can name its errors without depending on the pipeline types.
- **`ValidationBehavior` creates failures through a static abstract interface (`IFailureFactory<T>`)**: no reflection, but every response type must be a `Result`.
- **`EnsureCreated` in Development instead of migrations**: zero setup to run locally; add EF Core migrations before any shared or production database.
- **MediatR pinned to 12.5.0 and FluentAssertions to 7.2.0**: the last Apache-2.0 versions; newer major versions need a commercial license.
- **`System.Security.Cryptography.Pkcs` pinned transitively**: the version pulled in by `Microsoft.Data.SqlClient` has known CVEs, and warnings are errors.
- **Integration tests use a real SQL Server (LocalDB)**: realistic transactions and constraints, but tests need a SQL Server available; unit tests use the EF in-memory provider to stay fast.
- **Separate `CreateProductRequest` and `CreateProductCommand`**: a little duplication so the HTTP contract and the use case can change independently.

## Project structure

```
src/
  Domain/                 Entities and domain errors. References nothing.
    Common/               Error, ErrorType
    Products/             Product (private ctor + Create factory), ProductErrors
  Application/            Use cases. References Domain only.
    Abstractions/         ICommand/IQuery (+ handlers), IAppDbContext
    Behaviors/            Logging -> Validation -> Transaction (commands only)
    Common/               Result, Result<T>, ValidationError
    Products/             CreateProduct (command), GetProductById (query)
  Infrastructure/         EF Core. References Application.
    Persistence/          AppDbContext, entity configurations
  Api/                    Composition root. References Application + Infrastructure.
    Endpoints/            Minimal API endpoints
    Errors/               Result -> ProblemDetails mapping, global exception handler
tests/
  Application.Tests/      Handler and behavior unit tests (EF in-memory)
  Api.IntegrationTests/   WebApplicationFactory + LocalDB
  Architecture.Tests/     NetArchTest layer rules
```

Request flow:

```
HTTP -> Endpoint -> ISender.Send
     -> LoggingBehavior -> ValidationBehavior -> TransactionBehavior (commands only) -> Handler
     <- Result<T> -> 2xx, or result.ToProblem() -> 400 / 404 / 409 / 500 ProblemDetails
```

Repo-wide settings: `Directory.Build.props` (nullable, analyzers, warnings as errors, XML docs),
`Directory.Packages.props` (every package version pinned once), `.editorconfig`, `global.json`.

## How to run

Requirements: .NET 10 SDK and SQL Server LocalDB (installed with Visual Studio, or via the
SQL Server Express installer). No Docker needed.

```bash
dotnet run --project src/Api
```

The API listens on `http://localhost:5080` and creates the `CleanCqrs` database on first start
(Development only). Try the requests in `src/Api/Api.http`, or:

```bash
curl -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" -d "{\"name\":\"Keyboard\",\"price\":49.90}"
```

Not on Windows? Point the app at any SQL Server:

```bash
export ConnectionStrings__Database="Server=localhost,1433;Database=CleanCqrs;User Id=sa;Password=<password>;TrustServerCertificate=True"
```

## How to test

```bash
dotnet test
```

Integration tests create a uniquely named database per test class and drop it afterwards.
To use a server other than LocalDB, set `TEST_SQL_SERVER` to a connection string **without**
a database name, for example `Server=localhost,1433;User Id=sa;Password=<password>;TrustServerCertificate=True`.

| Project | Covers |
| --- | --- |
| Application.Tests | ValidationBehavior short-circuits without calling the handler; CreateProduct happy and duplicate paths |
| Api.IntegrationTests | POST 201, invalid POST 400 ValidationProblemDetails, duplicate POST 409, GET missing 404 |
| Architecture.Tests | Domain depends on nothing; Application depends on neither Infrastructure nor Api; Infrastructure does not depend on Api |

## .NET version used

- .NET SDK 10.0.401 (`global.json` requires 10.0.100 or a later feature band), target `net10.0`, C# 14
- EF Core 10.0.14, MediatR 12.5.0, FluentValidation 12.1.1
- xUnit 2.9.3, FluentAssertions 7.2.0, NetArchTest.Rules 1.3.2, Microsoft.AspNetCore.Mvc.Testing 10.0.14

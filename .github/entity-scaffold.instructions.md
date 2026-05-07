---
applyTo: "src/**"
---

# Entity Scaffold Instructions

Use this file whenever you need to add a new entity, feature, or endpoint to the project.
Follow every step in order. Do not skip any step. Generate production-ready code only.

---

## Before You Start — Extract Requirements

Read the user's request and identify:

| Requirement | How to get it |
|---|---|
| **Entity name** | Singular PascalCase. Example: `Product`, `Order`, `Category` |
| **Properties** | Name + type + nullable. Infer sensible types if not specified |
| **Endpoints** | Which of the 5 CRUD endpoints are needed. Default: all 5 |
| **Auth required** | Which endpoints need `[Authorize]`. Default: POST, PUT, DELETE |
| **Admin only** | Which endpoints need `[Authorize(Policy = PolicyConstants.RequireAdmin)]` |
| **Relationships** | FK to another entity? Navigation property needed? |
| **Unique fields** | Any field that must be unique? Add index + conflict check in service |

**Default assumptions when not specified:**
- Inherit `BaseEntity` (soft delete included automatically)
- All 5 CRUD endpoints generated
- GET endpoints: no auth. POST, PUT, DELETE: `[Authorize]`
- String max length: 200. Long text: 1000. Decimal: `decimal(18,2)`
- No FK relationships unless explicitly stated

State your assumptions as a comment before generating the first file.

---

## Project Namespaces

Always use these exact namespaces:

| Project | Namespace |
|---|---|
| `App.Api` | `App.Api.Controllers` |
| `App.Business` services | `App.Business.Services` |
| `App.Business` interfaces | `App.Business.Interfaces` |
| `App.Business` validators | `App.Business.Validators` |
| `App.Business` mappings | `App.Business.Mappings` |
| `App.DataAccess` context | `App.DataAccess.Context` |
| `App.DataAccess` configs | `App.DataAccess.Configurations` |
| `App.DataAccess` repositories | `App.DataAccess.Repositories` |
| `App.DataAccess` unit of work | `App.DataAccess.UnitOfWork` |
| `App.Domain` entities | `App.Domain.Entities` |
| `App.Domain` DTOs | `App.Domain.DTOs` |
| `App.Shared` exceptions | `App.Shared.Exceptions` |
| `App.Shared` responses | `App.Shared.Responses` |
| `App.Shared` constants | `App.Shared.Constants` |

---

## Step 1 — Domain Layer

### 1.1 — Entity

**File:** `src/App.Domain/Entities/{EntityName}.cs`

```csharp
namespace App.Domain.Entities;

public class {EntityName} : BaseEntity
{
    // All properties here
    // Use `required` for non-nullable strings
    // Navigation properties at the bottom initialized with = null!
    // Collection navigations: = new List<ChildEntity>()
}

```
**Rules:**
- Always inherit `BaseEntity`
- `BaseEntity` already provides: `Id (Guid)`, `CreatedAt`, `UpdatedAt`, `IsDeleted`,
  `DeletedAt`, `CreatedBy`, `UpdatedBy` — never re-declare any of these
- Use `required` modifier on non-nullable reference type properties
- Navigation properties: `public RelatedEntity RelatedEntity { get; set; } = null!;`
- Pure POCO — no methods, no logic

---

### 1.2 — DTOs

**Files:** `src/App.Domain/DTOs/`

Create these 3 files directly inside `DTOs/` folder (each related in a subfolder):

**`{EntityName}ResponseDto.cs`** — returned to client

```csharp
namespace App.Domain.DTOs;

public class {EntityName}ResponseDto
{
    public Guid Id { get; set; }
    // All readable properties
    // Include CreatedAt
    // If has FK: include FKId and denormalized name (e.g., CategoryName)
    // Never include: IsDeleted, DeletedAt, CreatedBy, UpdatedBy
}

```
**`{EntityName}CreateDto.cs`** — sent by client on POST

```csharp
namespace App.Domain.DTOs;

public class {EntityName}CreateDto
{
    // Fields the client provides on creation
    // No Id, no audit fields
    // Use string.Empty default for non-nullable strings
    // e.g., public string Name { get; set; } = string.Empty;
    // If has FK: include the FK Id (e.g., public Guid CategoryId { get; set; })
}

```
**`{EntityName}UpdateDto.cs`** — sent by client on PUT

```csharp
namespace App.Domain.DTOs;

public class {EntityName}UpdateDto
{
    // All updatable fields
    // Use string.Empty default for non-nullable strings (matches project pattern)
    // Never include Id, audit fields
}

```
---

## Step 2 — DataAccess Layer

### 2.1 — Fluent Configuration

**File:** `src/App.DataAccess/Configurations/{EntityName}Configuration.cs`

```csharp
using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.DataAccess.Configurations;

public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(e => e.Id);

        // Strings:  .IsRequired().HasMaxLength(200)
        // Long text: .HasMaxLength(1000)
        // Decimals: .IsRequired().HasColumnType("decimal(18,2)")
        // Enums:    .IsRequired().HasConversion<string>().HasMaxLength(50)
        // Unique:   builder.HasIndex(e => e.Name).IsUnique();

        // Relationships (if FK exists):
        // builder.HasOne(e => e.Parent)
        //     .WithMany(p => p.Children)
        //     .HasForeignKey(e => e.ParentId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}

```
**Rules:**
- Always configure primary key explicitly with `builder.HasKey`
- Every `string` property must have `HasMaxLength()` — never leave unconfigured
- Decimals always use `HasColumnType("decimal(18,2)")`
- Do NOT add `.HasQueryFilter` — `ApplicationDbContext` already applies it globally
  to all `BaseEntity` types via soft delete filter
- Auto-discovered via `ApplyConfigurationsFromAssembly` — no manual registration needed

---

### 2.2 — DbSet

**File to modify:** `src/App.DataAccess/Context/ApplicationDbContext.cs`

Add this line inside the class (follow the existing DbSet pattern):

```csharp
public DbSet<{EntityName}> {EntityName}s => Set<{EntityName}>();

```
---

### 2.3 — Repository Interface

**File:** `src/App.DataAccess/Repositories/I{EntityName}Repository.cs`

```csharp
using App.Domain.Entities;

namespace App.DataAccess.Repositories;

public interface I{EntityName}Repository : IGenericRepository<{EntityName}>
{
    // Add custom query methods only if generic methods are not enough
    // IGenericRepository already provides:
    //   GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync,
    //   DeleteAsync, FindAsync, ExistsAsync, CountAsync
    // Leave body empty if no custom methods needed
}

```
---

### 2.4 — Repository Implementation

**File:** `src/App.DataAccess/Repositories/{EntityName}Repository.cs`

```csharp
using App.Domain.Entities;

namespace App.DataAccess.Repositories;

public class {EntityName}Repository : GenericRepository<{EntityName}>, I{EntityName}Repository
{
    public {EntityName}Repository(Context.ApplicationDbContext context) : base(context)
    {
    }

    // Implement any custom methods declared in the interface
    // For Include queries: use _context.{EntityName}s.Include(...).FirstOrDefaultAsync(...)
}

```
---

### 2.5 — Unit of Work

**File to modify:** `src/App.DataAccess/UnitOfWork/IUnitOfWork.cs`

Add property (follow existing pattern):

```csharp
I{EntityName}Repository {EntityName}s { get; }

```
**File to modify:** `src/App.DataAccess/UnitOfWork/UnitOfWork.cs`

Add backing field and lazy property (follow existing pattern):

```csharp
private I{EntityName}Repository? _{entityName}Repository;
public I{EntityName}Repository {EntityName}s =>
    _{entityName}Repository ??= new {EntityName}Repository(_context);

```
---

### 2.6 — DI Registration

**File to modify:** `src/App.DataAccess/Extensions/ServiceCollectionExtensions.cs`

Add inside the existing registration method:

```csharp
services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();

```
---

### 2.7 — Migration Reminder

After all DataAccess files are done, output this:

```
Run EF migration:

dotnet ef migrations add Add{EntityName} --project src/App.DataAccess --startup-project src/App.Api
dotnet ef database update --project src/App.DataAccess --startup-project src/App.Api

```
---

## Step 3 — Business Layer

### 3.1 — Service Interface

**File:** `src/App.Business/Interfaces/I{EntityName}Service.cs`

```csharp
using App.Domain.DTOs;

namespace App.Business.Interfaces;

public interface I{EntityName}Service
{
    Task<IEnumerable<{EntityName}ResponseDto>> GetAllAsync(int page, int pageSize);
    Task<{EntityName}ResponseDto> GetByIdAsync(Guid id);
    Task<{EntityName}ResponseDto> CreateAsync({EntityName}CreateDto dto);
    Task<{EntityName}ResponseDto> UpdateAsync(Guid id, {EntityName}UpdateDto dto);
    Task DeleteAsync(Guid id);
}

```
Generate only the methods matching the requested endpoints.

---

### 3.2 — Service Implementation

**File:** `src/App.Business/Services/{EntityName}Service.cs`

```csharp
using App.Business.Interfaces;
using App.DataAccess.UnitOfWork;
using App.Domain.DTOs;
using App.Domain.Entities;
using App.Shared.Exceptions;
using AutoMapper;

namespace App.Business.Services;

public class {EntityName}Service : I{EntityName}Service
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public {EntityName}Service(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<{EntityName}ResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var items = await _unitOfWork.{EntityName}s.GetAllAsync(null, page, pageSize);
        return _mapper.Map<IEnumerable<{EntityName}ResponseDto>>(items);
    }

    public async Task<{EntityName}ResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.{EntityName}s.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("{EntityName} not found.");

        return _mapper.Map<{EntityName}ResponseDto>(entity);
    }

    public async Task<{EntityName}ResponseDto> CreateAsync({EntityName}CreateDto dto)
    {
        // If unique field exists, check for conflict first:
        // var exists = await _unitOfWork.{EntityName}s.ExistsAsync(e => e.Name == dto.Name);
        // if (exists) throw new ConflictException("{EntityName} with this name already exists.");

        var entity = _mapper.Map<{EntityName}>(dto);
        // DO NOT set entity.Id — EF Core handles this automatically

        await _unitOfWork.{EntityName}s.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<{EntityName}ResponseDto>(entity);
    }

    public async Task<{EntityName}ResponseDto> UpdateAsync(Guid id, {EntityName}UpdateDto dto)
    {
        var entity = await _unitOfWork.{EntityName}s.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("{EntityName} not found.");

        _mapper.Map(dto, entity);
        await _unitOfWork.{EntityName}s.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<{EntityName}ResponseDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.{EntityName}s.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("{EntityName} not found.");

        await _unitOfWork.{EntityName}s.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}

```
**Critical rules:**
- NEVER set `entity.Id = Guid.NewGuid()` — EF Core and `BaseEntity` handle this automatically
- NEVER inject `ApplicationDbContext` directly — always use `IUnitOfWork`
- NEVER return raw entities — always map to ResponseDto
- ALWAYS throw `NotFoundException` when entity not found — never return null
- ALWAYS throw `ConflictException` before saving when unique constraint would be violated
- ALWAYS call `_unitOfWork.SaveChangesAsync()` after every write operation
- NEVER catch exceptions silently — let them bubble to `ExceptionMiddleware`

---

### 3.3 — Validators

**File:** `src/App.Business/Validators/{EntityName}CreateDtoValidator.cs`

```csharp
using App.Domain.DTOs;
using FluentValidation;

namespace App.Business.Validators;

public class {EntityName}CreateDtoValidator : AbstractValidator<{EntityName}CreateDto>
{
    public {EntityName}CreateDtoValidator()
    {
        // Required strings:
        // RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        // Optional strings:
        // RuleFor(x => x.Description).MaximumLength(1000);

        // Decimals:
        // RuleFor(x => x.Price).GreaterThan(0);

        // Integers:
        // RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);

        // FK Guids:
        // RuleFor(x => x.CategoryId).NotEmpty();

        // Enums:
        // RuleFor(x => x.Status).IsInEnum();
    }
}

```
**File:** `src/App.Business/Validators/{EntityName}UpdateDtoValidator.cs`

```csharp
using App.Domain.DTOs;
using FluentValidation;

namespace App.Business.Validators;

public class {EntityName}UpdateDtoValidator : AbstractValidator<{EntityName}UpdateDto>
{
    public {EntityName}UpdateDtoValidator()
    {
        // Mirror CreateDto rules
        // RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        // RuleFor(x => x.Price).GreaterThan(0);
    }
}

```
**Rules:**
- `MaximumLength()` in validator MUST match `HasMaxLength()` in Fluent config exactly
- Validators auto-registered via `AddValidatorsFromAssemblyContaining` — never register manually

---

### 3.4 — AutoMapper Profile

**File:** `src/App.Business/Mappings/{EntityName}Profile.cs`

```csharp
using App.Domain.DTOs;
using App.Domain.Entities;
using AutoMapper;

namespace App.Business.Mappings;

public class {EntityName}Profile : Profile
{
    public {EntityName}Profile()
    {
        CreateMap<{EntityName}CreateDto, {EntityName}>();
        CreateMap<{EntityName}UpdateDto, {EntityName}>();
        CreateMap<{EntityName}, {EntityName}ResponseDto>();

        // If ResponseDto has a denormalized nav property (e.g., CategoryName):
        // CreateMap<{EntityName}, {EntityName}ResponseDto>()
        //     .ForMember(dest => dest.CategoryName,
        //                opt => opt.MapFrom(src => src.Category.Name));
    }
}

```
---

### 3.5 — DI Registration

**File to modify:** `src/App.Business/Extensions/ServiceCollectionExtensions.cs`

Add inside the existing registration method:

```csharp
services.AddScoped<I{EntityName}Service, {EntityName}Service>();

```
---

## Step 4 — API Layer

### 4.1 — Controller

**File:** `src/App.Api/Controllers/{EntityName}sController.cs`

```csharp
using App.Business.Interfaces;
using App.Domain.DTOs;
using App.Shared.Constants;
using App.Shared.Responses;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class {EntityName}sController : ControllerBase
{
    private readonly I{EntityName}Service _service;

    public {EntityName}sController(I{EntityName}Service service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<{EntityName}ResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<{EntityName}ResponseDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await _service.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<{EntityName}ResponseDto>>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<{EntityName}ResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<{EntityName}ResponseDto>>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<{EntityName}ResponseDto>.Ok(item));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<{EntityName}ResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<{EntityName}ResponseDto>>> Create(
        [FromBody] {EntityName}CreateDto dto)
    {
        var item = await _service.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            ApiResponse<{EntityName}ResponseDto>.Ok(item, "Created successfully."));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<{EntityName}ResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<{EntityName}ResponseDto>>> Update(
        Guid id,
        [FromBody] {EntityName}UpdateDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<{EntityName}ResponseDto>.Ok(item, "Updated successfully."));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "Deleted successfully."));
    }
}

```
**Rules:**
- Controller name is always plural: `{EntityName}sController`
- Route resolves automatically to `/api/v1/{entityname}s`
- Controller ONLY calls service methods — zero business logic, zero repository access
- Always wrap responses in `ApiResponse<T>.Ok()`
- Never use try/catch — `ExceptionMiddleware` handles all exceptions
- To get current user id inside a controller:
  `var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);`
- For admin-only endpoints:
  `[Authorize(Policy = PolicyConstants.RequireAdmin)]`
- For user-only endpoints:
  `[Authorize(Policy = PolicyConstants.RequireUser)]`

---

## Step 5 — Output Checklist

Print this checklist after all files are generated:

```
=== Scaffold Complete: {EntityName} ===

Domain (App.Domain)
  ✓ src/App.Domain/Entities/{EntityName}.cs
  ✓ src/App.Domain/DTOs/{EntityName}ResponseDto.cs
  ✓ src/App.Domain/DTOs/{EntityName}CreateDto.cs
  ✓ src/App.Domain/DTOs/{EntityName}UpdateDto.cs

DataAccess (App.DataAccess)
  ✓ src/App.DataAccess/Configurations/{EntityName}Configuration.cs
  ✓ src/App.DataAccess/Repositories/I{EntityName}Repository.cs
  ✓ src/App.DataAccess/Repositories/{EntityName}Repository.cs
  ✓ ApplicationDbContext.cs                   ← DbSet added
  ✓ IUnitOfWork.cs                            ← property added
  ✓ UnitOfWork.cs                             ← lazy property added
  ✓ DataAccess/ServiceCollectionExtensions.cs ← DI registered

Business (App.Business)
  ✓ src/App.Business/Interfaces/I{EntityName}Service.cs
  ✓ src/App.Business/Services/{EntityName}Service.cs
  ✓ src/App.Business/Validators/{EntityName}CreateDtoValidator.cs
  ✓ src/App.Business/Validators/{EntityName}UpdateDtoValidator.cs
  ✓ src/App.Business/Mappings/{EntityName}Profile.cs
  ✓ Business/ServiceCollectionExtensions.cs   ← DI registered

API (App.Api)
  ✓ src/App.Api/Controllers/{EntityName}sController.cs

Next Step — Run Migration:
  dotnet ef migrations add Add{EntityName} \
    --project src/App.DataAccess \
    --startup-project src/App.Api

  dotnet ef database update \
    --project src/App.DataAccess \
    --startup-project src/App.Api

Endpoints available:
  GET    /api/v1/{entityname}s
  GET    /api/v1/{entityname}s/{id}
  POST   /api/v1/{entityname}s          [Authorize]
  PUT    /api/v1/{entityname}s/{id}     [Authorize]
  DELETE /api/v1/{entityname}s/{id}     [Authorize]

```
---

## Global Rules — Never Violate

| Rule | Detail |
|---|---|
| No `new` for services | Always inject via constructor — never `new ServiceName()` |
| No `DbContext` outside DataAccess | Services never touch `ApplicationDbContext` directly |
| No raw entities from services | Always map to ResponseDto before returning |
| No logic in controllers | Controllers only call service methods |
| No silent exception catches | Let all exceptions bubble to `ExceptionMiddleware` |
| No hard deletes | `DeleteAsync` = soft delete. SQL DELETE is never used |
| No `entity.Id = Guid.NewGuid()` | EF Core handles Id generation automatically |
| Async everywhere | All methods return `Task` or `Task<T>`. Never use `.Result` or `.Wait()` |
| Typed exceptions only | Use types from `App.Shared.Exceptions` only |
| MaxLength must match | Validator `MaximumLength()` must equal Fluent `HasMaxLength()` |
| IOptions only | Never inject `IConfiguration` directly into services |
| PolicyConstants only | Never hardcode policy name strings — use `PolicyConstants.*` |

---

## Exception Reference

| Situation | Exception |
|---|---|
| Entity not found by id | `throw new NotFoundException("{Entity} not found.")` |
| Unique field conflict | `throw new ConflictException("{Entity} with this name already exists.")` |
| Invalid or expired token | `throw new UnauthorizedException("...")` |
| Access denied | `throw new ForbiddenException("...")` |
| Invalid business rule | `throw new ValidationException("...")` |
| External service failure | `throw new ServerException("...")` |

---

## Example Prompts to Use This File

```
Using #entity-scaffold.instructions.md, add a Product entity
with Name (string, required), Description (string, optional),
Price (decimal), StockQuantity (int).
All CRUD endpoints. POST, PUT, DELETE require [Authorize].

---

Using #entity-scaffold.instructions.md, add a Category entity
with Name (string, required, unique) and IsActive (bool).
GET list and GET by id endpoints only. No auth required.

---

Using #entity-scaffold.instructions.md, add an OrderItem entity
that belongs to Order (FK: OrderId).
Properties: ProductName (string), Quantity (int), UnitPrice (decimal).
All endpoints. POST and DELETE require admin policy only.

```
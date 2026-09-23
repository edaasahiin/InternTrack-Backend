# Data Access Queries

Repository interfaces describe retrieval and persistence operations.

Entity Framework Core queries execute inside the DataAccess layer.

Business services decide what the returned data means for an application operation.

No `IQueryable` crosses the DataAccess boundary.

---

## Query Responsibilities

The DataAccess layer is responsible for:

- Defining database queries
- Executing database queries
- Returning matching entities or scalar results
- Persisting changes

The Business layer is responsible for:

- Interpreting query results
- Applying validation
- Enforcing duplicate rules
- Returning Business-level conflicts or validation errors

This keeps database concerns separate from Business decisions.

---

## Department Name Lookup

Department duplicate-name validation uses:

    IDepartmentRepository.GetByNameIncludingInactiveAsync(
        name,
        excludeId
    )

The method returns:

- A matching `Department`
- Or `null`

It does not return a Business-level boolean such as `NameExistsAsync`.

---

## Department Query Flow

The current flow is:

    DepartmentService
        |
        v
    DepartmentRepository
        |
        v
    DepartmentQueries
        |
        v
    Entity Framework Core
        |
        v
    SQLite

### DepartmentQueries

`InternTrack.DataAccess/Queries/DepartmentQueries.cs` defines the Department name filter.

The query:

- Includes inactive records
- Ignores leading and trailing whitespace
- Compares normalized lowercase names
- Optionally excludes the current Department ID

Input casing uses invariant normalization.

Database casing uses SQLite's existing `lower` translation.

SQLite's existing non-ASCII case-folding limitations therefore remain unchanged.

### DepartmentRepository

`DepartmentRepository` executes the query using:

    FirstOrDefaultAsync

The repository does not decide whether a matching Department represents a Business conflict.

### DepartmentService

`DepartmentService.AddAsync` and `DepartmentService.UpdateAsync` interpret the returned value.

If a matching Department exists, the service returns the existing conflict result.

During update, the current Department ID is excluded from the lookup.

This allows a Department to keep its own name while still detecting another conflicting record.

---

## Matching Behavior

Department name matching:

- Includes inactive Departments
- Ignores leading and trailing whitespace
- Normalizes case
- Supports exclusion of the current Department during update

The lookup does not:

- Change persisted spelling
- Reactivate records
- Modify tracking configuration
- Change normal active-only read behavior

Leading and trailing whitespace in stored names are also ignored by this lookup.

---

## Query Implementation

The Department name lookup uses Entity Framework Core LINQ.

No raw SQL is used.

EF Core and the database provider generate parameterized SQL.

User input is not concatenated into SQL strings.

This lookup is used for validation only.

It is not:

- A database uniqueness constraint
- A concurrency guarantee
- A replacement for database-level transactional protection

---

## Repository Review Decisions

| Repository operation | Decision and responsibility |
| --- | --- |
| Department `NameExistsAsync` | Replaced by `GetByNameIncludingInactiveAsync`. Matching data is returned to `DepartmentService`, where duplicate-name rejection is enforced. |
| User `EmailExistsAsync` | Retained as a scalar email lookup using `AnyAsync`. Unlike `GetByEmailAsync`, it does not load the Intern navigation or involve its query filters. `AuthService` and `InternService` retain the duplicate-email Business decisions. |
| Intern `EmailExistsAsync` | Retained as a scalar email lookup including inactive Interns. It requires no navigation data or avatar population. `InternService` decides whether the result blocks the operation. |
| Intern `ExistsByDepartmentIdAsync` | Retained as an active-Intern relationship query. `DepartmentService` decides whether that relationship blocks deactivation. It deliberately respects the active-only filter. |
| Task retrieval and persistence | Retained. `GetByInternIdAsync` is a legitimate retrieval operation. Active/inactive scope and navigation loading remain explicit in `TaskRepository`. |
| Refresh-token retrieval and revocation | Retained. `GetByTokenAsync` hashes raw input before lookup. `RevokeAllByUserIdAsync` persists revocation. These methods do not represent duplicate-validation rules. |

---

## Query Organization

Simple queries remain close to their execution inside the relevant repository.

The `Queries` folder is used where separating a query definition improves readability and reviewability.

The Department name filter is kept separately because its behavior includes:

- Inactive-record matching
- Whitespace normalization
- Case normalization
- Update exclusion

No generic query framework or additional service layer is introduced.

The goal is to make it clear:

1. What is being queried
2. Where the query executes
3. Where the Business rule is enforced
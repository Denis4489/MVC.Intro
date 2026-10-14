# MVC Tickets Site — Security, Async, and Config Improvements

This pass hardens the ticket-sales MVC application across several dimensions: it replaces hardcoded demo credentials with configuration-driven values, adds `[Authorize]` and `[ValidateAntiForgeryToken]` to every mutating event action, converts all data-access calls to async, and introduces `UseStatusCodePagesWithReExecute` for error handling. The approach is evolutionary — no architectural changes, only targeted fixes over an existing cookie-auth / SQLite / session-cart stack.

**Watch for:** (1) `appsettings.Development.json` ships the demo password in plaintext in the repository — confirmed. (2) The `AppDbContext` still contains a no-arg constructor that bypasses DI and hard-codes a SQLite path — confirmed. (3) `UseStatusCodePagesWithReExecute` is registered outside the `IsDevelopment()` gate, meaning it also fires in production over the `UseExceptionHandler` path — confirmed.

**Verdict**: NEEDS_CHANGES

---

## High-level view

The authentication controller now reads `Demo:Email` / `Demo:Password` from `IConfiguration`, falling back to literals if the keys are absent. The values live in `appsettings.Development.json`, which is typically committed to source control. That file contains the password in plaintext, so the fallback literals and the config values are effectively equivalent exposure — the security improvement is only real if `appsettings.Development.json` is excluded from version control or the password is rotated per environment.

`EventService` is fully async and the null pattern (return early / return null) replaces the prior exception throws cleanly throughout. `EventController` gates every mutating action behind `[Authorize]` and `[ValidateAntiForgeryToken]`. `CartService.AddAsync` and `GetLinesAsync` both handle missing events gracefully.

`AppDbContext` now accepts `DbContextOptions<AppDbContext>` and is registered as `Scoped`, which is the correct lifetime for EF Core. However the no-arg constructor survives alongside it, carrying its own `OnConfiguring` fallback. If tooling or a test ever invokes that constructor, EF silently ignores the connection string from `appsettings.json` and builds its own path.

The `IValidatableObject` validation on `CreateEventViewModel` correctly enforces `AvailableTickets <= Capacity` and `EventDate > Now`. The comparison uses `DateTime.Now` (local time), which will drift against UTC-stored dates if the server is not in the same timezone as the application. Worth noting but not blocking for a local demo site.

The Login view's demo credentials block is wrapped in `env.IsDevelopment()`, so it will not render in production. This depends on the password in `appsettings.Development.json` matching the runtime config, which it does.

`UseStatusCodePagesWithReExecute("/Home/Error/{0}")` is placed unconditionally in `Program.cs`, after the `IsDevelopment()` / `UseExceptionHandler` block. In production, unhandled exceptions will be caught by `UseExceptionHandler` and the re-execute middleware also fires — the order is fine for status-code responses (404, etc.) but the placement should be intentional.

---

<details>
<summary>Issues (4)</summary>

1. **Demo password in committed config** — `appsettings.Development.json` contains `Demo:Password` in plaintext. This file is typically committed to source control, making the credential visible in the repo. Either add it to `.gitignore` / use user-secrets, or accept that this is an intentional demo-only credential and document it.

2. **Orphan no-arg constructor in AppDbContext** — `AppDbContext()` still exists alongside the DI constructor. Any tooling or code that calls it bypasses the `ConnectionStrings:DefaultConnection` from `appsettings.json` and falls back to a hard-coded path in `OnConfiguring`. Remove the no-arg constructor or make it `protected` / `private`.

3. **`UseStatusCodePagesWithReExecute` placement** — The middleware is registered unconditionally. In production this is correct for 4xx responses, but it should be verified that it does not interfere with the `UseExceptionHandler("/Home/Error")` path for unhandled exceptions. Move the call inside the `IsDevelopment()` else-block or leave it unconditional with a comment explaining the intent.

4. **`DateTime.Now` in `IValidatableObject`** — The `EventDate > DateTime.Now` check uses local server time. If dates are stored as UTC (EF Core default for SQLite), events created near a timezone boundary may pass or fail validation unexpectedly. Replace with `DateTime.UtcNow` and store `EventDate` as UTC consistently.

</details>

---

<details>
<summary>Details</summary>

### Demo credentials in configuration and the plaintext exposure gap

`AccountController` injects `IConfiguration` and reads `Demo:Email` and `Demo:Password` with fallback literals: `_configuration["Demo:Email"] ?? "tickets@fan.com"`. The values in `appsettings.Development.json` exactly match those literals, so at runtime there is no functional difference. The improvement is that the literals act as a safety net rather than the canonical source. The actual exposure risk is that `appsettings.Development.json` is almost certainly tracked in git — any developer who clones the repo gets the password. For a demo site this is an accepted trade-off, but it should be made explicit (e.g., a comment in the file, or a `.gitignore` entry).

### `AppDbContext` — dual constructor lifetime risk

The primary constructor `AppDbContext(DbContextOptions<AppDbContext> options) : base(options)` is correct and the `Scoped` registration in `Program.cs` will route through it. The no-arg constructor that follows calls `base(new DbContextOptions<AppDbContext>())` and then relies on `OnConfiguring` to set the connection. EF Core's `OnConfiguring` only runs when the options are not already configured, so the no-arg path will always build a new SQLite connection pointing to `Data/events.db` relative to `Directory.GetCurrentDirectory()`, independent of anything in `appsettings.json`. This is silent — no exception, just a different database. The no-arg constructor should be removed.

### Status-code error page and the middleware ordering

`UseStatusCodePagesWithReExecute("/Home/Error/{0}")` is placed unconditionally before `UseHttpsRedirection`. The `HomeController.Error(int? statusCode)` action receives the code as a route segment and populates `ErrorViewModel.StatusCode`. The 404 branch in `Error.cshtml` renders a user-friendly message. This works correctly for 404s. In production, unhandled exceptions route through `UseExceptionHandler("/Home/Error")` first; the status-code re-execute middleware does not fire for 5xx exceptions, only for responses that exit the pipeline with a 4xx status and no body. The ordering is therefore safe, but the unconditional placement could confuse a future developer — a short comment would prevent that.

### `DateTime.Now` vs UTC in event date validation

`CreateEventViewModel.Validate` compares `Event.EventDate <= DateTime.Now`. SQLite stores `DateTime` as a text or real column; EF Core by default converts to UTC on write. If the server runs in UTC+0 this is harmless. If it runs in UTC+2 (Bulgaria), a date entered as midnight local time is stored as 22:00 the previous day UTC, and the `DateTime.Now` comparison fires against local time while the persisted value is UTC. The bug is latent — it will only manifest when the server timezone differs from the application's assumed timezone. Switching to `DateTime.UtcNow` on the validation side, and ensuring `EventDate` is treated as UTC throughout, closes it.

</details>

---

<details>
<summary>File map</summary>

| File | Change |
|---|---|
| `Controllers/EventController.cs` | `[Authorize]` + `[ValidateAntiForgeryToken]` on all mutating actions; full async |
| `Services/EventService.cs` | All methods async; null-return pattern replaces exceptions |
| `Controllers/AccountController.cs` | `IConfiguration` injection; `Demo:*` config keys with literal fallback |
| `Services/CartService.cs` | `AddAsync` is async; `GetLinesAsync` skips and prunes stale items |
| `Program.cs` | `EventService` registered as Scoped; `UseStatusCodePagesWithReExecute` added |
| `Data/AppDbContext.cs` | Primary constructor accepts `DbContextOptions<AppDbContext>`; no-arg constructor retained |
| `appsettings.json` | `ConnectionStrings:DefaultConnection` added |
| `appsettings.Development.json` | `Demo:Email` and `Demo:Password` section added |
| `Models/CreateEventViewModel.cs` | `IValidatableObject` with capacity and future-date rules |
| `Models/Event.cs` | Regex extended to include `0-9` digits |
| `Views/Account/Login.cshtml` | Demo credentials block gated on `env.IsDevelopment()` |
| `Views/Cart/Checkout.cshtml` | CSS classes renamed from `psg-*` to `tickets-*` |
| `Views/Shared/Components/CartBadge/Default.cshtml` | Badge hidden when count is 0 |
| `Views/Home/Error.cshtml` | Created; handles 404 and generic error display |

Full diff: `git diff main` from repo root.

</details>

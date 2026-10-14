# Implementation Plan — MVC.Intro подобрения

Базиран на: `C:\Users\Students\Desktop\MVC\MVC\.agents\mvc-review.md`  
Проект: `C:\Users\Students\Desktop\MVC\MVC\MVC.Intro`  
Build команда: `dotnet build` в папката на проекта  
Верификация след всяка стъпка: `dotnet build` трябва да завършва без грешки (0 errors).

---

## Приоритет и зависимости

Стъпките са наредени така: критичните поправки → сериозни проблеми → UI/брандинг подобрения.  
Всяка стъпка оставя кода в билдваемо и работещо състояние.

---

- [ ] 1. **Поправи null-проверката и изключенията в `EventService`**

  **Какво:** В `AddEvent` — премести null-проверката на `@event` преди копирането на свойствата.  
  В `GetEventById`, `DeleteEvent` и `UpdateEvent` — замени `throw new ArgumentNullException(...)` с `throw new KeyNotFoundException(...)`, за да отразява правилната семантика ("запис не е намерен" ≠ "null аргумент").  
  В `AddEvent` — след `DecorateEventName` добави XML/code коментар (`// Добавя EVT_ prefix — съобрази EventCatalog ключовете!`), за да предупреди бъдещите разработчици.

  **Точни промени в `Services/EventService.cs`:**
  ```
  public Event AddEvent(Event @event)
  {
      // ПРЕМЕСТЕНО ТУКА — преди употребата:
      if (@event == null) throw new ArgumentNullException(nameof(@event));

      var toAdd = new Event { ... };
      _logger.LogInformation(...);
      // ПРЕМАХНИ старото: if (@event == null) { throw ... }
      ...
  }

  public Event GetEventById(Guid id)
  {
      var @event = _context.Events.FirstOrDefault(e => e.Id == id);
      if (@event == null)
          throw new KeyNotFoundException($"Събитие с Id={id} не е намерено.");
      return @event;
  }
  // Същото за DeleteEvent и UpdateEvent
  ```

  **Файлове:** `Services/EventService.cs`  
  **Верификация:** `dotnet build` — 0 грешки, 0 предупреждения свързани с unreachable code.

---

- [ ] 2. **Добави `[Authorize]` и `[ValidateAntiForgeryToken]` в `EventController`**

  **Какво:** Добави `[Authorize]` атрибут над мутиращите actions: `Create` (GET), `CreateEvent` (POST), `Edit` (GET), `EditEvent` (POST), `Delete` (POST).  
  `Index` и `Details` остават публични.  
  Добави `[ValidateAntiForgeryToken]` над `CreateEvent`, `EditEvent` и `Delete`.

  **Точни промени в `Controllers/EventController.cs`:**
  ```csharp
  [HttpGet]
  [Authorize]
  public IActionResult Create() { ... }

  [HttpGet("{id}")]
  [Authorize]
  public IActionResult Edit(Guid id) { ... }

  [HttpPost]
  [Authorize]
  [ValidateAntiForgeryToken]
  public IActionResult EditEvent(CreateEventViewModel viewModel) { ... }

  [HttpPost]
  [Authorize]
  [ValidateAntiForgeryToken]
  public IActionResult Delete(Guid id) { ... }

  [HttpPost]
  [Authorize]
  [ValidateAntiForgeryToken]
  public IActionResult CreateEvent(CreateEventViewModel viewModel) { ... }
  ```

  **Файлове:** `Controllers/EventController.cs`  
  **Верификация:** `dotnet build` — 0 грешки. Ръчна проверка: достъп до `/Event/Create` без вход трябва да пренасочва към `/Account/Login`.

---

- [ ] 3. **Смени `EventService` от `Transient` на `Scoped` в `Program.cs`**

  **Какво:** Замени `builder.Services.AddTransient<EventService>()` с `builder.Services.AddScoped<EventService>()`.  
  `EventService` зависи от `AppDbContext` (Scoped) — двете трябва да имат еднакъв lifetime.

  **Точна промяна в `Program.cs`:**
  ```csharp
  // ПРЕДИ:
  builder.Services.AddTransient<EventService>();
  // СЛЕД:
  builder.Services.AddScoped<EventService>();
  ```

  **Файлове:** `Program.cs`  
  **Верификация:** `dotnet build` — 0 грешки.

---

- [ ] 4. **Добави `UseStatusCodePagesWithReExecute` за 404 обработка в `Program.cs`**

  **Какво:** Добави `app.UseStatusCodePagesWithReExecute("/Home/Error/{0}")` непосредствено преди `app.UseHttpsRedirection()`.  
  Добави в `HomeController.Error` обработка на HTTP статус код от route-а.  
  Добави в `Views/Shared/Error.cshtml` условен блок, който показва "Страницата не е намерена" при statusCode == 404.

  **Точна промяна в `Program.cs`** — добави след `if (!app.Environment.IsDevelopment()) { ... }`:
  ```csharp
  app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
  ```

  **Промяна в `Controllers/HomeController.cs`** — обнови `Error()`:
  ```csharp
  public IActionResult Error(int? statusCode = null)
  {
      var model = new ErrorViewModel
      {
          RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
          StatusCode = statusCode
      };
      return View(model);
  }
  ```

  **Промяна в `Models/ErrorViewModel.cs`** — добави `public int? StatusCode { get; set; }`.

  **Промяна в `Views/Shared/Error.cshtml`** — добави блок:
  ```html
  @if (Model.StatusCode == 404)
  {
      <h2>404 — Страницата не е намерена</h2>
      <p>Адресът, който търсите, не съществува.</p>
  }
  ```

  **Файлове:** `Program.cs`, `Controllers/HomeController.cs`, `Models/ErrorViewModel.cs`, `Views/Shared/Error.cshtml`  
  **Верификация:** `dotnet build` — 0 грешки. Ръчна проверка: `/nonexistent` трябва да показва страница на `HomeController.Error` вместо празен Kestrel отговор.

---

- [ ] 5. **Graceful handling на изтрити events в `CartService.GetLines()`**

  **Какво:** В `CartService.GetLines()` — при извикване на `_eventService.GetEventById(item.ProductId)` хващай `KeyNotFoundException` (след стъпка 1) и пропускай реда вместо да хвърляш 500. Почистени items трябва да бъдат премахнати от сесията.

  **Точна промяна в `Services/CartService.cs`:**
  ```csharp
  public IReadOnlyList<CartLineViewModel> GetLines()
  {
      var items = GetItems();
      var lines = new List<CartLineViewModel>();
      var validItems = new List<CartItem>();

      foreach (var item in items)
      {
          try
          {
              var @event = _eventService.GetEventById(item.ProductId);
              var catalog = EventCatalog.Get(@event.Name);
              lines.Add(new CartLineViewModel
              {
                  ProductId = @event.Id,
                  Name = @event.Name,
                  Category = catalog.Category,
                  Price = @event.Price,
                  Quantity = item.Quantity
              });
              validItems.Add(item);
          }
          catch (KeyNotFoundException)
          {
              // Събитието е изтрито — пропускаме го мълчаливо
          }
      }

      // Почисти сесията от невалидни items
      if (validItems.Count != items.Count)
      {
          Save(validItems);
      }

      return lines;
  }
  ```

  **Файлове:** `Services/CartService.cs`  
  **Верификация:** `dotnet build` — 0 грешки.

---

- [ ] 6. **Добави cross-field валидация `AvailableTickets <= Capacity` в `Event` модела**

  **Какво:** Имплементирай `IValidatableObject` в `Models/Event.cs`.  
  Добави `[Range(0, 100000)]` атрибут на `AvailableTickets`.  
  В `Validate()` метода — върни грешка ако `AvailableTickets > Capacity`.

  **Точни промени в `Models/Event.cs`:**
  ```csharp
  public class Event : IValidatableObject
  {
      // ... съществуващи свойства ...

      [DisplayName("Налични билети")]
      [Required(ErrorMessage = "Полето е задължително")]
      [Range(0, 100000, ErrorMessage = "Наличните билети трябва да са между 0 и 100000")]
      public int AvailableTickets { get; set; }

      public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      {
          if (AvailableTickets > Capacity)
          {
              yield return new ValidationResult(
                  "Наличните билети не могат да надвишават капацитета.",
                  new[] { nameof(AvailableTickets) }
              );
          }
      }
  }
  ```

  **Файлове:** `Models/Event.cs`  
  **Верификация:** `dotnet build` — 0 грешки.

---

- [ ] 7. **Добави валидация за бъдеща дата на `EventDate` в `Event` модела**

  **Какво:** Създай нов custom validation attribute `Attributes/FutureDateAttribute.cs`.  
  Атрибутът проверява дали датата е `>= DateTime.Today`.  
  Приложи атрибута върху `EventDate` в `Models/Event.cs`.

  **Нов файл `Attributes/FutureDateAttribute.cs`:**
  ```csharp
  using System.ComponentModel.DataAnnotations;

  namespace MVC.Intro.Attributes
  {
      public class FutureDateAttribute : ValidationAttribute
      {
          public FutureDateAttribute()
          {
              ErrorMessage = "Датата на събитието трябва да бъде в бъдещето.";
          }

          public override bool IsValid(object? value)
          {
              if (value is DateTime date)
              {
                  return date.Date >= DateTime.Today;
              }
              return true; // Required атрибутът ще хване null/default
          }
      }
  }
  ```

  **Промяна в `Models/Event.cs`** — добави атрибута:
  ```csharp
  [DisplayName("Дата на събитието")]
  [Required(ErrorMessage = "Полето е задължително")]
  [FutureDate]
  public DateTime EventDate { get; set; }
  ```

  **Важно:** Seed данните в `DbSeeder.cs` имат дати в миналото. Тъй като `DbSeeder` не минава през `ModelState` валидация, seed-ването ще продължи да работи. Тази валидация важи само за формите (Create/Edit).

  **Файлове:** `Attributes/FutureDateAttribute.cs` (нов), `Models/Event.cs`  
  **Верификация:** `dotnet build` — 0 грешки.

---

- [ ] 8. **Разшири regex на `Event.Name` да позволява цифри**

  **Какво:** Промени `RegularExpression` атрибута на `Name` в `Models/Event.cs` — добави `0-9` в character class, за да позволи имена като "Matchday 5" или "Inter Milan 2025".

  **Точна промяна в `Models/Event.cs`:**
  ```csharp
  // ПРЕДИ:
  [RegularExpression(@"^[A-Za-zА-Яа-яЁёЪъЬь\s\-]+$", ErrorMessage = "Името може да съдържа само букви, интервали и тирета")]
  // СЛЕД:
  [RegularExpression(@"^[A-Za-zА-Яа-яЁёЪъЬь0-9\s\-]+$", ErrorMessage = "Името може да съдържа само букви, цифри, интервали и тирета")]
  ```

  **Файлове:** `Models/Event.cs`  
  **Верификация:** `dotnet build` — 0 грешки.

---

- [ ] 9. **Поправи брандинга в `Views/Cart/Checkout.cshtml` — смяна на `psg-*` с `tickets-*`**

  **Какво:** Замени целия `psg-*` CSS и хардкодираното съдържание в `Checkout.cshtml` с `tickets-*` еквиваленти. Подмени "PSG" и "Мерси, Парижанино!" с неутрален текст, съответстващ на общия брандинг.

  **Точно ново съдържание на `Views/Cart/Checkout.cshtml`:**
  ```html
  @{
      ViewData["Title"] = "Поръчката е приета";
  }

  <section class="tickets-section">
      <div class="container">
          <div class="tickets-card-bg text-center py-5">
              <div class="tickets-logo mx-auto mb-3" style="font-size: 3rem;">🎫</div>
              <h1 class="tickets-title">Поръчката е потвърдена!</h1>
              <p class="tickets-muted">
                  Поръчката е приета. Артикули: @ViewBag.ItemCount · Сума: @(((decimal)ViewBag.Total).ToString("0.00")) €
              </p>
              <a class="btn tickets-btn-primary mt-3" asp-controller="Home" asp-action="Index">Обратно към събитията</a>
          </div>
      </div>
  </section>
  ```

  **Файлове:** `Views/Cart/Checkout.cshtml`  
  **Верификация:** `dotnet build` — 0 грешки. Ръчна проверка: страницата `/Cart/Checkout` (след checkout) не показва "PSG" или "Мерси, Парижанино!".

---

- [ ] 10. **Поправи `CartBadge` — скрий badge при 0 и смени `psg-badge` с `tickets-badge`**

  **Какво:** В `Views/Shared/Components/CartBadge/Default.cshtml` — добави условие `@if (Model > 0)` около badge span-а. Смени CSS класа от `psg-badge` на `tickets-badge`. Смени `psg-cart-link` на `tickets-cart-link`.

  **Точно ново съдържание на `Views/Shared/Components/CartBadge/Default.cshtml`:**
  ```html
  @model int
  <a class="nav-link tickets-cart-link" asp-controller="Cart" asp-action="Index">
      🛒 Количка
      @if (Model > 0)
      {
          <span class="tickets-badge">@Model</span>
      }
  </a>
  ```

  **Добави `.tickets-badge` CSS правило в `wwwroot/css/site.css`** — копирай стила от `.psg-badge` (ако съществува) или дефинирай:
  ```css
  .tickets-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 1.25rem;
      height: 1.25rem;
      padding: 0 .35rem;
      font-size: .7rem;
      font-weight: 700;
      border-radius: 999px;
      background: var(--tickets-accent, #e63946);
      color: #fff;
      vertical-align: middle;
      margin-left: .25rem;
  }
  ```

  **Файлове:** `Views/Shared/Components/CartBadge/Default.cshtml`, `wwwroot/css/site.css`  
  **Верификация:** `dotnet build` — 0 грешки. Ръчна проверка: при празна количка badge-ът не се вижда; при добавен артикул — показва се.

---

- [ ] 11. **Премести демо credentials в `appsettings.Development.json` и скрий от продукционния UI**

  **Какво:**  
  1. В `appsettings.Development.json` — добави секция с демо данни:
     ```json
     "Demo": {
       "Email": "tickets@fan.com",
       "Password": "Tickets123!"
     }
     ```
  2. В `AccountController` — инжектирай `IConfiguration` и `IWebHostEnvironment`, вземи credentials от конфигурацията вместо `public const`.  
  3. В `Views/Account/Login.cshtml` — добави условие `@if (ViewBag.ShowDemoCredentials == true)` около демо блока.  
  4. В `AccountController.Login` (GET) — задай `ViewBag.ShowDemoCredentials = env.IsDevelopment()`.

  **Точни промени в `Controllers/AccountController.cs`:**
  ```csharp
  private readonly IConfiguration _config;
  private readonly IWebHostEnvironment _env;

  public AccountController(IConfiguration config, IWebHostEnvironment env)
  {
      _config = config;
      _env = env;
  }

  [HttpGet]
  public IActionResult Login(string? returnUrl = null)
  {
      if (User.Identity?.IsAuthenticated == true)
          return RedirectToAction("Index", "Home");

      ViewData["ReturnUrl"] = returnUrl;
      ViewBag.ShowDemoCredentials = _env.IsDevelopment();
      return View(new LoginViewModel());
  }

  // Login POST — вземи credentials от конфигурацията:
  var demoEmail = _config["Demo:Email"] ?? "tickets@fan.com";
  var demoPassword = _config["Demo:Password"] ?? "Tickets123!";
  if (!string.Equals(model.Email, demoEmail, ...) || model.Password != demoPassword) { ... }
  ```

  **Точна промяна в `Views/Account/Login.cshtml`:**
  ```html
  @if (ViewBag.ShowDemoCredentials == true)
  {
      <div class="tickets-demo mt-4">
          <strong>Демо акаунт</strong>
          <span>Имейл: @ViewBag.DemoEmail</span>
          <span>Парола: (вижте appsettings.Development.json)</span>
      </div>
  }
  ```
  Забележка: не показваме паролата в HTML дори в Development. Само имейла за удобство.

  **Файлове:** `Controllers/AccountController.cs`, `Views/Account/Login.cshtml`, `appsettings.Development.json`  
  **Верификация:** `dotnet build` — 0 грешки.

---

- [ ] 12. **Изчисти миграционния стек**

  **Какво:** Изтрий всички файлове в `Migrations/` папката и генерирай нова миграция, отразяваща реалния `Event` модел. Изтрий старите `.db` файлове (`.db` файловете се възстановяват при следващото стартиране чрез `DbSeeder`).

  **Точни стъпки:**
  1. Изтрий всички файлове в `Migrations/`:
     - `Migrations/20251008163519_Create.cs`
     - `Migrations/20251008163519_Create.Designer.cs`
     - `Migrations/20261007115228_AddImageUrlToProduct.cs`
     - `Migrations/20261007115228_AddImageUrlToProduct.Designer.cs`
     - `Migrations/AppDbContextModelSnapshot.cs`
  2. Изтрий `Data/events.db` и `Data/products.db`.
  3. Изпълни: `dotnet ef migrations add InitialCreate` в папката на проекта.
  4. Изпълни: `dotnet ef database update` (ако `AppDbContext` е конфигуриран за миграции — вж. бележка по-долу).

  **Важна бележка:** Текущият `AppDbContext` ползва `EnsureCreated()` в `DbSeeder`, което е несъвместимо с миграции. Има два варианта:
  - **Вариант А (избран):** Запази `EnsureCreated()` за момента — генерирай миграциите само за да синхронизираш snapshot-а. В `Program.cs` не добавяй `db.Database.Migrate()`.
  - **Вариант Б:** Смени `EnsureCreated()` с `Migrate()` в `DbSeeder` и премахни новата `InitialCreate` migration ако базата вече е създадена.

  Изборът е Вариант А — по-малко промени, snapshot е синхронизиран, `EnsureCreated` продължава да работи.

  **Файлове:** `Migrations/*` (изтриване), `Data/events.db` (изтриване), `Data/products.db` (изтриване)  
  **Верификация:** `dotnet build` — 0 грешки. `dotnet ef migrations list` трябва да покаже `InitialCreate` без стари `Create`/`AddImageUrlToProduct` записи.

---

## Обобщена таблица

| # | Файл(ове) | Тип промяна | Приоритет |
|---|-----------|-------------|-----------|
| 1 | `Services/EventService.cs` | Bug fix (wrong exception type, unreachable code) | Критичен |
| 2 | `Controllers/EventController.cs` | Security (Authorize + CSRF) | Критичен |
| 3 | `Program.cs` | DI lifetime fix (Transient → Scoped) | Сериозен |
| 4 | `Program.cs`, `Controllers/HomeController.cs`, `Models/ErrorViewModel.cs`, `Views/Shared/Error.cshtml` | 404 handling | Сериозен |
| 5 | `Services/CartService.cs` | Bug fix (graceful deleted-event handling) | Сериозен |
| 6 | `Models/Event.cs` | Validation (cross-field AvailableTickets ≤ Capacity) | Подобрение |
| 7 | `Attributes/FutureDateAttribute.cs` (нов), `Models/Event.cs` | Validation (future date) | Подобрение |
| 8 | `Models/Event.cs` | Validation (regex разширен с цифри) | Подобрение |
| 9 | `Views/Cart/Checkout.cshtml` | Branding fix (psg-* → tickets-*) | Подобрение |
| 10 | `Views/Shared/Components/CartBadge/Default.cshtml`, `wwwroot/css/site.css` | Branding fix + UX (скрий badge при 0) | Подобрение |
| 11 | `Controllers/AccountController.cs`, `Views/Account/Login.cshtml`, `appsettings.Development.json` | Security (credentials от конфигурация) | Сериозен |
| 12 | `Migrations/*` (изтриване + regenerate) | Chore (migration cleanup) | Сериозен |

**Не са включени:** async/await рефакторинг на `EventService` (функционално не влияе при текущия трафик, промяната е по-голяма и засяга всички consumers — може да се направи като отделна задача), промяна на `DecorateEventName` (логиката е намеренна, само добавен коментар в стъпка 1), промяна на DB connection string в `appsettings.json` (изисква рефакторинг на `AppDbContext` конструктора — по-голяма промяна без видим ефект за потребителя).

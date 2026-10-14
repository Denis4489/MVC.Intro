# Преглед на MVC.Intro — Пълен анализ

## Резюме

Проектът е ASP.NET Core 8 MVC уебсайт за продажба на спортни билети. Архитектурата е добре разделена на слоеве (Controllers → Services → Data), има ViewModels, cookie-базирана автентикация и session-базирана количка. **Приложението е функционално и работещо**, но има редица сериозни грешки и пропуски, описани по-долу.

**Критични проблеми:** 3  
**Сериозни проблеми:** 7  
**Подобрения / добри практики:** 8

---

## 1. КРИТИЧНИ ГРЕШКИ

### 1.1. Паролата е hardcoded и видима в изходния код и в HTML

**Файл:** `Controllers/AccountController.cs` (редове 11–12), `Views/Account/Login.cshtml` (демо блок)

```csharp
public const string DemoEmail = "tickets@fan.com";
public const string DemoPassword = "Tickets123!";
```

```html
<!-- Login.cshtml -->
<div class="tickets-demo mt-4">
    <strong>Демо акаунт</strong>
    <span>Имейл: tickets@fan.com</span>
    <span>Парола: Tickets123!</span>
</div>
```

Паролата е `public const` — достъпна при декомпилация и буквално изписана в HTML-а на страницата за вход. За демо е приемливо, но ако проектът се развива, паролата трябва да се премести в `appsettings.json` / `User Secrets` и да не се показва в UI на продукционна среда.

---

### 1.2. Миграциите са напълно разминати с реалния модел

**Файлове:** `Migrations/20251008163519_Create.cs`, `Migrations/20261007115228_AddImageUrlToProduct.cs`, `Migrations/AppDbContextModelSnapshot.cs`

Миграциите описват таблица **`Products`** с модел **`Product`** (Id, Name, Price, ImageUrl) — примитивен модел. Реалният `AppDbContext` обаче работи с `DbSet<Event>` и таблица **`Events`** с много повече колони (EventDate, Venue, TicketType, Capacity, AvailableTickets, Description). `AppDbContextModelSnapshot.cs` все още описва `MVC.Intro.Models.Product`.

**Последствие:** `EnsureCreated()` в `DbSeeder.Seed()` заобикаля миграционния механизъм и директно създава таблицата от текущия модел. Това означава, че миграционният стек е **мъртъв** — никога не се изпълнява. При евентуален `dotnet ef database update` се получава конфликт. Snapshot-ът не отговаря на нито един реален клас в проекта.

**Препоръка:** Изтрийте всички миграции, изтрийте `.db` файловете, изпълнете `dotnet ef migrations add InitialCreate` и `dotnet ef database update`, за да синхронизирате snapshot-а с реалния модел.

---

### 1.3. `ArgumentNullException` се ползва неправилно за бизнес логика

**Файл:** `Services/EventService.cs`

```csharp
public Event GetEventById(Guid id)
{
    var @event = _context.Events.FirstOrDefault(e => e.Id == id);
    if (@event == null)
    {
        throw new ArgumentNullException("Event not found"); // ГРЕШНО!
    }
    return @event;
}
```

Същата грешка се повтаря в `DeleteEvent` и `UpdateEvent`. `ArgumentNullException` е за **null аргументи** на метод, не за "запис не е намерен". При несъществуващ Id потребителят получава `500 Internal Server Error` с misleading изключение вместо `404 Not Found`. Правилно е `throw new KeyNotFoundException(...)` или по-добре — да се върне `null` / `Result<T>` и контролерът да върне `NotFound()`.

Допълнително в `AddEvent`:
```csharp
var toAdd = new Event { ... }; // използва @event
_logger.LogInformation(...);   // логва
if (@event == null)            // проверката е СЛЕД употребата — никога не може да е true тук
{
    throw new ArgumentNullException("Event is null");
}
```
Тази null-проверка е недостижим код и не прави нищо полезно.

---

## 2. СЕРИОЗНИ ПРОБЛЕМИ

### 2.1. Липса на `[Authorize]` върху CRUD операциите на EventController

**Файл:** `Controllers/EventController.cs`

```csharp
[Route("[controller]/[action]")]
public class EventController : Controller
{
    public IActionResult Index() { ... }

    [HttpGet]
    public IActionResult Create() { ... }

    [HttpPost]
    public IActionResult CreateEvent(CreateEventViewModel viewModel) { ... }

    [HttpPost]
    public IActionResult Delete(Guid id) { ... }

    [HttpPost]
    public IActionResult EditEvent(CreateEventViewModel viewModel) { ... }
}
```

Всеки анонимен потребител може да създава, редактира и изтрива събития. Добавянето в количка изисква влизане само при checkout (в `CartController`), но управлението на каталога е публично. Трябва `[Authorize]` над контролера или над мутиращите actions.

---

### 2.2. Delete не е защитен срещу CSRF и се изпълнява с GET в RoutingController

**Файл:** `Views/Event/Index.cshtml`

```html
<form asp-action="Delete" asp-controller="Event" asp-route-id="@item.Id" method="post" class="d-inline">
    <button type="submit" class="btn btn-sm btn-outline-danger">Изтрий</button>
</form>
```

Формата е `POST` и използва `asp-action` tag helper, което ще генерира CSRF token автоматично ако контролерът има `[ValidateAntiForgeryToken]`. **Проблемът** е, че `Delete` action-ът **няма** `[ValidateAntiForgeryToken]` атрибут:

```csharp
[HttpPost]
public IActionResult Delete(Guid id) // липсва [ValidateAntiForgeryToken]
{
    _eventService.DeleteEvent(id);
    return RedirectToAction(nameof(Index));
}
```

Същото важи за `CreateEvent` и `EditEvent`. Само `CartController` actions са правилно защитени.

---

### 2.3. `AppDbContext` не използва Dependency Injection правилно

**Файл:** `Data/AppDbContext.cs`

```csharp
public AppDbContext() : base(new DbContextOptions<AppDbContext>())
{
    var basePath = Directory.GetCurrentDirectory();
    // ...
    DbPath = Path.Combine(dataPath, "events.db");
}

protected override void OnConfiguring(DbContextOptionsBuilder options)
{
    if (!options.IsConfigured)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}
```

Конфигурацията на базата данни е хардкодирана в конструктора вместо да идва от `IConfiguration` / `appsettings.json`. Connection string-ът не е в `appsettings.json`. `Directory.GetCurrentDirectory()` може да даде различен резултат при IIS хостинг. Правилният подход е конфигурация чрез `AddDbContext<AppDbContext>(options => options.UseSqlite(configuration.GetConnectionString("Default")))` в `Program.cs`.

---

### 2.4. `EventService` е регистриран като `Transient` но зависи от `AppDbContext` (Scoped)

**Файл:** `Program.cs`

```csharp
builder.Services.AddTransient<EventService>(); // Transient
builder.Services.AddScoped<AppDbContext>();    // Scoped
```

Когато `Transient` сервиз приема `Scoped` зависимост, при определени контексти (напр. singleton, background services) може да се получи "captive dependency" проблем. За уеб-заявки в този проект ефектът е ограничен, но е неправилна архитектура. `EventService` трябва да е `Scoped` (живее колкото заявката), а не `Transient`.

---

### 2.5. `CartService.GetLines()` може да хвърли изключение при изтрит event в количката

**Файл:** `Services/CartService.cs`

```csharp
public IReadOnlyList<CartLineViewModel> GetLines()
{
    return GetItems()
        .Select(item =>
        {
            var @event = _eventService.GetEventById(item.ProductId); // хвърля изключение ако event е изтрит
            var catalog = EventCatalog.Get(@event.Name);
            ...
        })
        .ToList();
}
```

Ако потребителят е добавил събитие в количката, а след това то е изтрито от администратор, при следващото зареждане на количката `GetEventById` хвърля `ArgumentNullException` и потребителят вижда `500` грешка. Трябва graceful handling — пропускане на несъществуващи items или показване на съобщение "Събитието вече не е налично".

---

### 2.6. Routing конфликт — `routing` маршрутът е опасно широк

**Файл:** `Program.cs`

```csharp
app.MapControllerRoute(
    name: "routing",
    pattern: "{controller=Routing}/{*default}",
    defaults: new { controller = "Routing", action = "Default" });
```

Този catch-all маршрут (`{*default}`) се регистрира след `default` маршрута, но тъй като `{controller}` е variable, той може да прихване заявки, ако `default` маршрутът не съвпадне. Стойността `{*default}` е wildcard за остатъка, но именоването `default` на параметъра е объркващо. Освен това `RoutingController` не прави нищо освен да показва статична страница и не се нуждае от специален маршрут.

---

### 2.7. `DecorateEventName` мутира Name тихо при всяко добавяне

**Файл:** `Services/EventService.cs`

```csharp
private void DecorateEventName(Event @event)
{
    @event.Name = EventPrefix + @event.Name;
}
```

При добавяне на ново събитие, ако `Name` не започва с `"EVT_"`, префиксът се добавя автоматично. Потребителят пише "Bayern Munich vs Dortmund", но в базата се записва "EVT_Bayern Munich vs Dortmund". Тогава `EventCatalog.Get("EVT_Bayern Munich vs Dortmund")` не намери запис и връща дефолтна категория "Спорт" с сив акцент вместо правилната. Seed данните от `DbSeeder` нямат prefix, но новосъздадените events ще го имат — несъответствие.

---

## 3. ПОДОБРЕНИЯ И ДОБРИ ПРАКТИКИ

### 3.1. Липса на async/await в целия Data Access слой

Всички EF Core операции са синхронни:

```csharp
// EventService.cs
public List<Event> GetAllEvents() => _context.Events.ToList();        // трябва ToListAsync()
public Event GetEventById(Guid id) => _context.Events.FirstOrDefault(); // FindAsync(id)
_context.SaveChanges();                                                  // SaveChangesAsync()
```

За уеб-приложения с потенциален паралелен трафик, синхронните I/O операции блокират thread pool threads. Всички методи трябва да станат `async Task<T>` и да ползват `async` EF Core варианти.

---

### 3.2. `DbSeeder` се извиква при всеки старт без проверка за production

**Файл:** `Program.cs`

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}
```

Seed-ването се изпълнява при **всеки** старт на приложението. `DbSeeder` прави проверка за съществуващи записи, така че дублиране не се случва, но LINQ заявките към базата при всяко зареждане са ненужни. По-добре: само в Development среда или само ако таблицата е празна с единична Count() заявка.

---

### 3.3. Липса на валидация на `AvailableTickets <= Capacity`

**Файл:** `Models/Event.cs`

```csharp
[Range(1, 100000)]
public int Capacity { get; set; }

public int AvailableTickets { get; set; } // без Range атрибут, без проверка спрямо Capacity
```

Може да се въведе `AvailableTickets = 99999` при `Capacity = 100`. Необходима е валидация `AvailableTickets <= Capacity`, реализирана чрез `IValidatableObject` на модела или custom validation attribute.

---

### 3.4. `EventDate` не се валидира за бъдеще дата

**Файл:** `Models/Event.cs`

```csharp
[Required(ErrorMessage = "Полето е задължително")]
public DateTime EventDate { get; set; }
```

Може да се въведе дата в миналото. Подходящо е да се добави custom attribute или флуент валидация, която проверява `EventDate > DateTime.Now`.

---

### 3.5. Regex в `Event.Name` изключва цифри и специални символи — твърде рестриктивен

**Файл:** `Models/Event.cs`

```csharp
[RegularExpression(@"^[A-Za-zА-Яа-яЁёЪъЬь\s\-]+$", ErrorMessage = "...")]
```

Seed данните имат имена като "Manchester City vs Liverpool" — без цифри. Но реалните футболни срещи могат да съдържат цифри (напр. "Inter Milan vs AC Milan 2025"). `vs` е в обхвата (латински), но имена с номера на кръгове ("Matchday 5") ще се откажат. Regex-ът трябва да включва `0-9` или поне да позволява прост текст без SQL injection вектори.

---

### 3.6. Няма страница за грешка 404

**Файл:** `Program.cs`

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

`UseExceptionHandler` се занимава само с необработени изключения (500). Няма `app.UseStatusCodePagesWithReExecute("/Home/Error/{0}")` или подобно. Директно извикване на несъществуващ URL дава стандартна празна IIS/Kestrel страница.

---

### 3.7. Checkout страницата е брандирана като "PSG" вместо "Tickets"

**Файл:** `Views/Cart/Checkout.cshtml`

```html
<section class="psg-section">
    <div class="psg-card text-center py-5 psg-success">
        <div class="psg-login-crest mx-auto mb-3">PSG</div>
        <h1 class="psg-title">Мерси, Парижанино!</h1>
```

Целият layout ползва CSS класове `tickets-*`, но Checkout страницата ползва стари `psg-*` класове. Хардкоднатото "PSG" и "Мерси, Парижанино!" са остатъци от предишна версия на проекта и не отговарят на общата brand идентичност на сайта.

---

### 3.8. `CartBadge` ViewComponent показва "0" в количката — без визуално скриване

**Файл:** `Views/Shared/Components/CartBadge/Default.cshtml`

```html
<span class="psg-badge">@Model</span>
```

Бейджът е винаги видим, дори когато количката е празна (показва "0"). Стандартната практика е да се скрие badge-ът при 0: `@if (Model > 0) { <span class="badge">@Model</span> }`. Също така ползва `psg-badge` CSS клас вместо `tickets-*` — остатък от предишния брандинг.

---

## 4. АРХИТЕКТУРНИ НАБЛЮДЕНИЯ

| Аспект | Оценка | Коментар |
|--------|--------|----------|
| Разделение на слоеве | ✅ Добро | Controllers → Services → Data е правилно разделено |
| ViewModels | ✅ Добро | `CreateEventViewModel`, `EventViewModel`, `CartLineViewModel` са налице |
| Service Layer | ✅ Налице | `EventService`, `CartService`, `ImageService` |
| DI регистрация | ⚠️ Частично | EventService Transient+Scoped DbContext е проблем |
| Валидация | ⚠️ Частично | Има DataAnnotations, но липсват cross-field валидации |
| Async I/O | ❌ Липсва | Всички DB операции са синхронни |
| Authorization | ❌ Непълна | EventController е публично достъпен |
| Error handling | ⚠️ Частично | Хвърля изключения вместо HTTP статус кодове |
| Migrations | ❌ Разминати | Snapshot е за несъществуващ `Product` модел |
| Logging | ✅ Частично | LogInformation в AddEvent; другаде липсва |

---

## 5. ОБОБЩЕНИ ПРЕПОРЪКИ (по приоритет)

1. **Поправете миграциите** — изтрийте старите, генерирайте нова `InitialCreate` от реалния `Event` модел.
2. **Добавете `[Authorize]`** върху `Create`, `CreateEvent`, `Edit`, `EditEvent`, `Delete` в `EventController`.
3. **Добавете `[ValidateAntiForgeryToken]`** на `Delete`, `CreateEvent`, `EditEvent`.
4. **Преминете към async/await** в `EventService` — `ToListAsync`, `FindAsync`, `SaveChangesAsync`.
5. **Поправете `GetEventById`** — хвърляйте `KeyNotFoundException` или връщайте `null` и обработвайте в контролера.
6. **Поправете `CartService.GetLines()`** — graceful handling при изтрит event.
7. **Вземете connection string от конфигурация** вместо хардкодиран в `AppDbContext`.
8. **Сменете Transient → Scoped** за `EventService`.
9. **Добавете `UseStatusCodePagesWithReExecute`** за 404 обработка.
10. **Изчистете `DecorateEventName`** или го документирайте явно — в момента разваля `EventCatalog` lookup за нови events.
11. **Ребрандирайте Checkout** — подменете `psg-*` класовете и "Мерси, Парижанино!" с неутрален `tickets-*` стил.
12. **Преместете демо credentials** в `appsettings.Development.json` и не ги показвайте в продукционен UI.

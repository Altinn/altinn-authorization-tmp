# ReadOnly Replica Routing

Dette dokumentet beskriver hvordan ReadOnly-replikaer, global round-robin-loadbalansering og per-request routing (hints) er implementert i Altinn.AccessMgmt.

Løsningen gir:

* Global round-robin mellom alle ReadOnly-replikas
* Mulighet til å rute enkelte endepunkt eller kall til spesifikke replikaer
* Mulighet til å inkludere Primary i round-robin-poolen ved behov
* Full støtte for scoped hints selv om selector er singleton
* Null scoping-problemer og null EF-tracking overhead
* Dynamisk konfigurasjon av replika-noder via AppConfig/appsettings

---

## 📂 Arkitektur

```
Controller (Request)
     │
     ▼
IReadOnlyHintService  (scoped)
     │     ▲
     ▼     │
ReadOnlySelector  (singleton)
- global round robin
- per-request hint override
- fallback til primary
     │
     ▼
ReadOnlyDbContextFactory  (pooled)
     │
     ▼
PostgreSQL
┌───────────┬───────────┬───────────┐
│ Replica0  │ Replica1  │ Replica2  │
└───────────┴───────────┴───────────┘
```

---

## ⚙️ Konfigurasjon (appsettings / AppConfig)

Systemet støtter dynamisk antall replikaer, navngitt av deg.

```json
"PostgreSQLSettings": {
  "ConnectionString": "Host=my-primary;Username=app;Password={0}",
  "AuthorizationDbPwd": "mypassword",
  "AuthorizationDbReadOnlyPwd": "<replaces default>"

  "ReadOnlyConnectionStrings": {
    "Replica0": "Host=my-replica-1;Username=app;Password={0}",
    "Replica1": "Host=my-replica-2;Username=app;Password={0}",
    "Replica2": "Host=my-replica-3;Username=app;Password={0}"
  }
},

"DatabaseRouting": {
  "IncludePrimaryInReadOnlyPool": false,
  "EnableReadOnlyHints": true
}
```

**Forklaring:**

* `ReadOnlyConnectionStrings`
  Valgfritt antall replikaer, navngitt av deg.

* `IncludePrimaryInReadOnlyPool`

  * `true`: primary inngår i round-robin
  * `false`: primary brukes kun ved hint eller fallback

* `EnableReadOnlyHints`
  Aktiverer eller deaktiverer hint-basert routing.

---

## 🧩 DI Registrering

```csharp
services.AddScoped<IReadOnlyHintService, ReadOnlyHintService>();

services.AddSingleton<IReadOnlySelector>(sp =>
{
    return new ReadOnlySelector(sp);
});

services.AddPooledDbContextFactory<ReadOnlyDbContext>((sp, opt) =>
{
    AddReadOnlyDbContext(sp, opt, options);
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    opt.EnableSensitiveDataLogging(false);
});
```

---

## 🧠 Hvordan ReadOnlySelector fungerer

### 1. Hint har høyeste prioritet

```csharp
_hintService.SetHint("Replica2");
```

→ Query går mot Replica2 uansett.

### 2. Global Round Robin hvis ingen hint

Global round robin (singleton + static index):

```
Replica0 → Replica1 → Replica2 → Replica0 → ...
```

### 3. Fallback til Primary

Hvis:

* ingen readonly-noder finnes
* round-robin pool er tom
* hint ikke eksisterer

→ bruk Primary-connectionstring.

---

## 🎮 Bruk av Hint

### A) I kode (services, handlers, bakgrunnsjobber)

```csharp
_hintService.SetHint("Replica1");

var result = await connectionQuery.GetConnectionsAsync(filter);
```

### B) Via attribute på controller-endpoint

```csharp
[UseReplica("Replica2")]
[HttpGet("audit")]
public async Task<IActionResult> GetAudit()
{
    return Ok(await _connectionQuery.GetConnectionsAsync(new()));
}
```

Deretter i MVC-setup:

```csharp
options.Filters.Add<UseReplicaActionFilter>();
```

---

## 🔌 Hvordan ReadOnlyDbContext velger korrekt replika

`AddReadOnlyDbContext()` må gjøre:

```csharp
var selector = sp.GetRequiredService<IReadOnlySelector>();
opt.UseNpgsql(selector.GetConnectionString());
```

Dette garanterer at *hver* ReadOnlyDbContext får riktig connectionstring.

---

## 🧩 Oversikt over komponentene

| Komponent                   | Scope     | Ansvar                                     |
| --------------------------- | --------- | ------------------------------------------ |
| `ReadOnlyHintService`       | Scoped    | Holder hint per request                    |
| `ReadOnlySelector`          | Singleton | Velger connectionstring (RR + hint)        |
| `ReadOnlyDbContextFactory`  | Singleton | Lager ReadOnlyDbContext basert på selector |
| `ReadOnlyConnectionStrings` | Config    | Liste over replikaer                       |
| `UseReplicaAttribute`       | Metadata  | Marker endepunkt for hinting               |
| `UseReplicaActionFilter`    | Scoped    | Setter hint fra attributt                  |

---

## ✔ Oppsummering

Dette systemet gir:

* Ekte global round-robin mellom replika-noder
* Per-request routing til valgt node via hint
* Primary-fallback
* Ingen DI-scoping-problemer
* Null EF-tracking overhead
* Ren og fleksibel konfigurasjon
* Lett utvidelig med nye replikaer uten kodeendring

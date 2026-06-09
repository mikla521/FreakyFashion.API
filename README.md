# Freaky Fashion API

ASP.NET Core 8 Web API för e-handelssajten Freaky Fashion.

## Kom igång

### Krav
- .NET 8 SDK
- SQL Server (LocalDB räcker för lokal utveckling)

### Köra lokalt

```bash
cd FreakyFashion.API

# Återställ paket
dotnet restore

# Skapa databas (kör migrations)
dotnet ef database update

# Starta API:et
dotnet run
```

API:et startar på `https://localhost:5001`.  
Scalar-dokumentation finns på `/scalar/v1`.

### Skapa migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Endpoints

### Autentisering
| Metod | URL | Beskrivning | Auth |
|-------|-----|-------------|------|
| POST | `/api/auth/login` | Logga in, få JWT-token | – |

### Produkter
| Metod | URL | Beskrivning | Auth |
|-------|-----|-------------|------|
| GET | `/api/products` | Lista produkter (paginering: `?page=1&pageSize=10`) | – |
| GET | `/api/products?slug=xxx` | Hämta produkt via slug | – |
| GET | `/api/products/{id}` | Hämta produkt via ID | – |
| POST | `/api/products` | Skapa produkt | ✅ JWT |
| PATCH | `/api/products/{id}` | Uppdatera produkt (JSON Patch) | ✅ JWT |
| DELETE | `/api/products/{id}` | Radera produkt | ✅ JWT |

### Kategorier
| Metod | URL | Beskrivning | Auth |
|-------|-----|-------------|------|
| GET | `/api/categories` | Lista alla kategorier med produkter | – |
| GET | `/api/categories?slug=xxx` | Hämta kategori via slug | – |
| GET | `/api/categories/{id}` | Hämta kategori via ID | – |
| POST | `/api/categories` | Skapa kategori | ✅ JWT |
| PATCH | `/api/categories/{id}` | Uppdatera kategori (JSON Patch) | ✅ JWT |
| DELETE | `/api/categories/{id}` | Radera kategori | ✅ JWT |
| DELETE | `/api/categories/{cid}/products/{pid}` | Ta bort produkt från kategori | ✅ JWT |

### Varukorg
| Metod | URL | Beskrivning | Header |
|-------|-----|-------------|--------|
| GET | `/api/cart` | Hämta varukorg | `X-Session-Id` |
| POST | `/api/cart/items` | Lägg till produkt | `X-Session-Id` |
| PUT | `/api/cart/items/{id}` | Uppdatera antal | `X-Session-Id` |
| DELETE | `/api/cart` | Töm varukorg | `X-Session-Id` |

## Konfiguration

Känsliga värden sätts som **App Settings** i Azure (eller user-secrets lokalt):

| Nyckel | Beskrivning |
|--------|-------------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Secret` | Minst 32 tecken lång hemlig nyckel |
| `Auth__Username` | Admin-användarnamn |
| `Auth__Password` | Admin-lösenord |
| `ApplicationInsights__ConnectionString` | Application Insights |

## Docker

```bash
# Bygg image
docker build -t freakyfashion-api .

# Kör container
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Secret="..." \
  freakyfashion-api
```

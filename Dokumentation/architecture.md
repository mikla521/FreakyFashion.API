# Arkitektur

## Översikt

Freaky Fashion är en e-handelsplattform med ett REST API byggt i ASP.NET Core 10, körs i Azure Container Apps och använder Azure SQL Database som datakälla.

---

## Systemarkitektur

```
┌─────────────────────────────────────────────────────────────┐
│                        Azure                                │
│                                                             │
│   ┌─────────────────┐        ┌──────────────────────────┐  │
│   │  Azure Container│        │   Azure SQL Database     │  │
│   │  Apps           │◄──────►│   FreakyFashionDb        │  │
│   │  freakyfashion- │        └──────────────────────────┘  │
│   │  api            │                                       │
│   │                 │        ┌──────────────────────────┐  │
│   │  (container)    │───────►│   Application Insights   │  │
│   └────────▲────────┘        │   Loggar & telemetri     │  │
│            │                 └──────────────────────────┘  │
│   ┌────────┴────────┐                                       │
│   │  Azure Container│                                       │
│   │  Registry (ACR) │                                       │
│   │  Docker images  │                                       │
│   └─────────────────┘                                       │
└─────────────────────────────────────────────────────────────┘
         ▲
         │  HTTPS
         │
┌────────┴────────┐       ┌──────────────────────────────┐
│   Klient        │       │   Azure DevOps               │
│   (Postman /    │       │   Repo + CI/CD Pipeline      │
│   Frontend)     │       │   Push → Build → Deploy      │
└─────────────────┘       └──────────────────────────────┘
```

---

## CI/CD-flöde

```
Utvecklare pushar till main
         │
         ▼
  Azure DevOps Repo
         │
         ▼
  Pipeline startar automatiskt
         │
         ├── Stage 1: Build & Test
         │     dotnet restore
         │     dotnet build
         │     dotnet test
         │
         ├── Stage 2: Docker
         │     docker build
         │     docker push → ACR (tag: build-id + latest)
         │
         └── Stage 3: Deploy
               az containerapp update
               (ny image + miljövariabler sätts)
```

---

## Teknisk stack

| Lager | Teknik |
|-------|--------|
| API-ramverk | ASP.NET Core 10 |
| Databas-ORM | Entity Framework Core 10 |
| Databas | Azure SQL Database |
| Autentisering | JWT Bearer Tokens |
| API-dokumentation | Scalar (OpenAPI) |
| Containerisering | Docker |
| Container-registry | Azure Container Registry |
| Hosting | Azure Container Apps |
| Monitoring | Azure Application Insights |
| CI/CD | Azure DevOps Pipelines |

---

## API-struktur

```
FreakyFashion.API/
├── Controllers/
│   ├── AuthController.cs        # POST /api/auth/login
│   ├── ProductsController.cs    # CRUD /api/products
│   ├── CategoriesController.cs  # CRUD /api/categories
│   └── CartsController.cs       # /api/cart
├── Data/
│   └── AppDbContext.cs           # EF Core DbContext
├── DTOs/
│   └── Dtos.cs                   # Request/Response-modeller
├── Entities/
│   ├── Product.cs
│   ├── Category.cs
│   └── Cart.cs
└── Services/
    ├── TokenService.cs           # JWT-generering
    └── SlugHelper.cs             # URL-slug från namn
```

---

## Databasmodell

```
Product ──────────────── ProductCategories ──── Category
  id                           product_id            id
  name                         category_id           name
  description                                        image
  price                                              url_slug
  image
  url_slug

Cart ──────────── CartItem ──── Product
  id                  id
  session_id          cart_id
  created_at          product_id
                      quantity
```

---

## Säkerhet

- **JWT-autentisering** krävs för alla skrivoperationer (POST, PATCH, DELETE)
- **Hemligheter** lagras som krypterade pipeline-variabler i Azure DevOps och injiceras som miljövariabler i containern vid deploy – aldrig i källkoden
- **HTTPS** enforced av Azure Container Apps
- **Varukorg** identifieras via `X-Session-Id`-header (session-baserad, ingen inloggning krävs)

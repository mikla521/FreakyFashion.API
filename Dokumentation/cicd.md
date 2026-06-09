# CI/CD med Azure DevOps

Den här guiden beskriver hur du sätter upp kontinuerlig integration och driftsättning (CI/CD) för Freaky Fashion API med Azure DevOps.

## Översikt

Varje push till `main`-branchen triggar en pipeline med tre steg:

```
Push till main
      │
      ▼
┌─────────────┐
│  Stage 1    │  dotnet restore → build → test
│  Build      │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Stage 2    │  docker build → push till ACR
│  Docker     │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Stage 3    │  az containerapp update → ny image live
│  Deploy     │
└─────────────┘
```

---

## Steg 1 – Skapa projekt i Azure DevOps

1. Gå till [dev.azure.com](https://dev.azure.com) och logga in med skolkontot
2. Klicka **New organization** om du inte har en, eller använd befintlig
3. Klicka **New project**
   - Name: `FreakyFashion`
   - Visibility: `Private`
4. Klicka **Create**

---

## Steg 2 – Pusha kod till Azure Repos

I terminalen, i projektmappen:

```bash
git init
git add .
git commit -m "initial commit"
git remote add origin https://dev.azure.com/<ORG>/FreakyFashion/_git/FreakyFashion
git push -u origin main
```

Byt ut `<ORG>` mot ditt organisationsnamn i Azure DevOps.

---

## Steg 3 – Skapa Service Connections

Service connections ger pipelinen behörighet att prata med Azure och ACR.

### 3a. Service connection till Azure (ARM)

1. Gå till **Project Settings → Service connections → New service connection**
2. Välj **Azure Resource Manager**
3. Välj **Service principal (automatic)**
4. Välj din prenumeration och resource group `freakyfashion-rg`
5. Namnge den: `freakyfashion-service-connection`
6. Kryssa i **Grant access permission to all pipelines**
7. Klicka **Save**

### 3b. Service connection till ACR (Docker Registry)

1. Gå till **Project Settings → Service connections → New service connection**
2. Välj **Docker Registry**
3. Välj **Azure Container Registry**
4. Välj din prenumeration och `freakyfashionacr`
5. Namnge den: `freakyfashionacr`
6. Klicka **Save**

---

## Steg 4 – Skapa pipeline-variabler (hemligheter)

Dessa värden ska **aldrig** checkas in i koden. De lagras krypterat i Azure DevOps.

1. Gå till **Pipelines → New pipeline** (eller välj din pipeline)
2. Klicka **Variables** (uppe till höger)
3. Lägg till följande variabler och markera dem som **Secret** (lås-ikonen):

| Variabelnamn | Värde | Secret |
|---|---|---|
| `DB_CONNECTION_STRING` | Connection string från Azure SQL | ✅ |
| `JWT_SECRET` | Minst 32 tecken lång hemlig sträng | ✅ |
| `AUTH_USERNAME` | Admin-användarnamn (t.ex. `admin`) | – |
| `AUTH_PASSWORD` | Admin-lösenord | ✅ |
| `APPINSIGHTS_CONNECTION_STRING` | Connection string från Application Insights | ✅ |

---

## Steg 5 – Skapa pipeline från YAML

1. Gå till **Pipelines → New pipeline**
2. Välj **Azure Repos Git**
3. Välj repot `FreakyFashion`
4. Välj **Existing Azure Pipelines YAML file**
5. Välj `/azure-pipelines.yml`
6. Klicka **Continue → Run**

Pipelinen körs nu för första gången.

---

## Steg 6 – Verifiera att det fungerar

1. Gå till **Pipelines** och kontrollera att alla tre stages blir gröna
2. Gå till **Azure Portal → Container Apps → freakyfashion-api → Revision management** och verifiera att en ny revision skapades
3. Testa API:et via Application URL:en (finns under Overview på Container App)

```bash
curl https://<din-app>.azurecontainerapps.io/api/products
```

---

## Felsökning

**Stage 1 misslyckas – NuGet restore**
Kontrollera att `.csproj`-sökvägen i `azure-pipelines.yml` stämmer med din mappstruktur.

**Stage 2 misslyckas – Docker push**
Kontrollera att service connection-namnet i `azure-pipelines.yml` (`acrName`) exakt matchar det du skapade i steg 3b.

**Stage 3 misslyckas – az containerapp update**
Kontrollera att service connection-namnet (`azureSubscription`) och resursnamnens stavning i variablerna stämmer.

**API svarar 500**
Kontrollera loggar i **Azure Portal → Container Apps → freakyfashion-api → Log stream**, eller i Application Insights under **Failures**.

---

## Databas-migrations i molnet

Applikationen kör `db.Database.Migrate()` automatiskt vid uppstart (konfigurerat i `Program.cs`), så databasen migreras automatiskt när containern startar.

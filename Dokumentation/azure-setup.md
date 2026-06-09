# Azure-uppsättning

Den här guiden beskriver hur du sätter upp alla Azure-resurser som krävs för att köra Freaky Fashion API i molnet.

## Förutsättningar

- Azure-konto (skolkonto med Azure for Students)
- [Azure CLI](https://learn.microsoft.com/sv-se/cli/azure/install-azure-cli) installerat lokalt
- Docker Desktop installerat lokalt

---

## 1. Logga in i Azure CLI

```bash
az login
```

Välj ditt skolkonto i webbläsaren som öppnas.

---

## 2. Skapa Resource Group

En resource group samlar alla resurser för projektet på ett ställe.

```bash
az group create \
  --name freakyfashion-rg \
  --location swedencentral
```

---

## 3. Skapa Azure SQL Database

### 3a. Skapa SQL Server

```bash
az sql server create \
  --name freakyfashion-sql \
  --resource-group freakyfashion-rg \
  --location swedencentral \
  --admin-user sqladmin \
  --admin-password <DITT_STARKA_LÖSENORD>
```

### 3b. Öppna brandväggen för Azure-tjänster

```bash
az sql server firewall-rule create \
  --resource-group freakyfashion-rg \
  --server freakyfashion-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### 3c. Skapa databasen

```bash
az sql db create \
  --resource-group freakyfashion-rg \
  --server freakyfashion-sql \
  --name FreakyFashionDb \
  --edition Basic \
  --capacity 5
```

### 3d. Spara connection string

Gå till **Azure Portal → SQL Database → FreakyFashionDb → Connection strings** och kopiera ADO.NET-strängen. Den ser ut ungefär så här:

```
Server=tcp:freakyfashion-sql.database.windows.net,1433;Initial Catalog=FreakyFashionDb;Persist Security Info=False;User ID=sqladmin;Password=<LÖSENORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Spara strängen – den används som pipeline-variabel senare.

---

## 4. Skapa Application Insights

```bash
az monitor app-insights component create \
  --app freakyfashion-insights \
  --resource-group freakyfashion-rg \
  --location swedencentral \
  --kind web
```

Hämta connection string:

```bash
az monitor app-insights component show \
  --app freakyfashion-insights \
  --resource-group freakyfashion-rg \
  --query connectionString \
  --output tsv
```

Spara connection string – används som pipeline-variabel senare.

---

## 5. Skapa Azure Container Registry (ACR)

```bash
az acr create \
  --name freakyfashionacr \
  --resource-group freakyfashion-rg \
  --sku Basic \
  --admin-enabled true
```

> **Obs:** `--admin-enabled true` krävs för att Azure DevOps ska kunna pusha images.

Hämta inloggningsuppgifter (behövs när du skapar Service Connection i Azure DevOps):

```bash
az acr credential show \
  --name freakyfashionacr \
  --resource-group freakyfashion-rg
```

---

## 6. Skapa Azure Container Apps

### 6a. Skapa Container Apps Environment

```bash
az containerapp env create \
  --name freakyfashion-env \
  --resource-group freakyfashion-rg \
  --location swedencentral
```

### 6b. Skapa Container App

Ersätt `<ACR_PASSWORD>` med lösenordet från steg 5.

```bash
az containerapp create \
  --name freakyfashion-api \
  --resource-group freakyfashion-rg \
  --environment freakyfashion-env \
  --image freakyfashionacr.azurecr.io/freakyfashion-api:latest \
  --registry-server freakyfashionacr.azurecr.io \
  --registry-username freakyfashionacr \
  --registry-password <ACR_PASSWORD> \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 3 \
  --cpu 0.5 \
  --memory 1.0Gi
```

> API:et blir publikt åtkomligt via en HTTPS-URL som genereras automatiskt. Hitta den under **Azure Portal → Container Apps → freakyfashion-api → Overview → Application URL**.

---

## Sammanfattning av resurser

| Resurs | Namn | Nivå |
|--------|------|------|
| Resource Group | freakyfashion-rg | – |
| SQL Server | freakyfashion-sql | – |
| SQL Database | FreakyFashionDb | Basic |
| Application Insights | freakyfashion-insights | – |
| Container Registry | freakyfashionacr | Basic |
| Container Apps Env | freakyfashion-env | Consumption |
| Container App | freakyfashion-api | Consumption |

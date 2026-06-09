# Lokal utveckling

Den här guiden beskriver hur du sätter upp och kör Freaky Fashion API lokalt.

## Förutsättningar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sv-se/sql-server/sql-server-downloads) eller SQL Server LocalDB (ingår i Visual Studio)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) eller [VS Code](https://code.visualstudio.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (valfritt, för att testa containern lokalt)

---

## 1. Klona repot

```bash
git clone https://dev.azure.com/<ORG>/FreakyFashion/_git/FreakyFashion
cd FreakyFashion
```

---

## 2. Konfigurera appsettings

Öppna `FreakyFashion.API/appsettings.Development.json` och kontrollera att connection string stämmer med din lokala SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FreakyFashionDev;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "dev-secret-key-minimum-32-characters!!",
    "Issuer": "FreakyFashionAPI",
    "Audience": "FreakyFashionClient"
  },
  "Auth": {
    "Username": "admin",
    "Password": "admin123"
  }
}
```

> `appsettings.Development.json` ska **inte** checkas in med riktiga lösenord. Lägg till den i `.gitignore` om du lägger till känsliga värden.

---

## 3. Skapa databasen

### Alternativ A – Package Manager Console (Visual Studio)

1. Öppna Package Manager Console: **Tools → NuGet Package Manager → Package Manager Console**
2. Kontrollera att Default project är `FreakyFashion.API`
3. Kör:

```powershell
Add-Migration InitialCreate
Update-Database
```

### Alternativ B – Terminal

```bash
cd FreakyFashion.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 4. Starta API:et

### Visual Studio
Tryck **F5** eller klicka på **Run**.

### Terminal
```bash
cd FreakyFashion.API
dotnet run
```

API:et startar på `https://localhost:5001` (eller `http://localhost:5000`).

---

## 5. Utforska API:et med Scalar

Öppna webbläsaren och gå till:

```
https://localhost:5001/scalar/v1
```

Här ser du all dokumentation och kan testa endpoints direkt.

---

## 6. Testa med Postman

### Logga in och få JWT-token

```
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

Kopiera `access_token` från svaret och lägg till som header på skyddade anrop:

```
Authorization: Bearer <token>
```

### Testa varukorgen

Varukorgen identifieras via headern `X-Session-Id`. Hitta på ett eget UUID och använd det konsekvent:

```
X-Session-Id: f47ac10b-58cc-4372-a567-0e02b2c3d479
```

---

## 7. Köra med Docker lokalt (valfritt)

```bash
# Bygg image
docker build -t freakyfashion-api .

# Kör container
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=FreakyFashionDev;Trusted_Connection=True;TrustServerCertificate=True;" \
  -e Jwt__Secret="dev-secret-key-minimum-32-characters!!" \
  -e Auth__Username="admin" \
  -e Auth__Password="admin123" \
  freakyfashion-api
```

> `host.docker.internal` refererar till din lokala dator från inuti containern.

API:et är nu tillgängligt på `http://localhost:8080`.

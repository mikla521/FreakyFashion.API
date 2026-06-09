# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FreakyFashion.API/FreakyFashion.API.csproj FreakyFashion.API/
RUN dotnet restore FreakyFashion.API/FreakyFashion.API.csproj

COPY . .
WORKDIR /src/FreakyFashion.API
RUN dotnet publish -c Release -o /app/publish

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FreakyFashion.API.dll"]

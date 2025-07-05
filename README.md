# 🚗 Garagge Web API

Garagge to nowoczesna, modularna aplikacja Web API napisana w .NET 8, służąca do zarządzania pojazdami. Projekt oparty na architekturze Clean Architecture, gotowy do samodzielnego hostowania lub wdrożenia w chmurze jako SaaS.

---

## ✨ Funkcje

- ✅ Rejestrowanie pojazdów i historii serwisowej  
- ✅ Obsługa użytkowników i ról (JWT)  
- ✅ REST API z dokumentacją OpenAPI  
- ✅ Modułowa architektura (Domain, Application, Infrastructure, API)  
- ✅ Wsparcie dla środowisk Dev / Staging / Prod  
- ✅ Gotowe do konteneryzacji z Docker + Docker Compose

---

## 🛠️ Stos technologiczny

- [.NET 8 Web API](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- Entity Framework Core  
- PostgreSQL / SQLite (w zależności od środowiska)  
- Clean Architecture  
- JWT + ASP.NET Identity  
- Serilog + Seq (logowanie)  
- Docker & Docker Compose  

---

## 🚀 Szybki start

### ✅ Wymagania

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/) (opcjonalnie)
- [PostgreSQL](https://www.postgresql.org/) (jeśli nie używasz Dockera)

### 🔧 Uruchomienie lokalne (bez Dockera)

```bash
# Przygotuj środowisko
dotnet restore

# Uruchom migracje (jeśli wymagane)
dotnet ef database update --project Infrastructure

# Uruchom API
dotnet run --project Api

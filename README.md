# IPA_Manager

IPA Manager Tool für Lehrlinge - Eine Webapplikation zur Nachverfolgung des Fortschritts bei der individuellen praktischen Arbeit (IPA).

## Technologie-Stack

- **Frontend:** Blazor Server (.NET 9)
- **Backend:** ASP.NET Core
- **Datenbank:** MySQL
- **Testing:** NUnit, Playwright (E2E), Moq
- **CI/CD:** GitHub Actions
- **Containerisierung:** Docker

## Lokale Entwicklung

### Voraussetzungen

- .NET 9 SDK
- Docker (für MySQL)

### Datenbank starten

```bash
cd database
docker-compose up -d
```

### Anwendung starten

```bash
cd Ipa.Manager
dotnet run
```

### Tests ausführen

```bash
dotnet test
```

### Code formatieren (Linter-Fehler beheben)

Um Code-Formatierungsfehler zu beheben:

```bash
dotnet format
```

Dies formatiert alle Dateien automatisch gemäss den Projekt-Standards.

## CI/CD Pipeline

Die Pipeline ist in zwei Workflows aufgeteilt:

### 1. PR-Validation (`pullrequest-validation.yaml`)

Wird bei jedem Pull Request ausgeführt:

| Step | Beschreibung |
|------|--------------|
| Checkout | Repository auschecken |
| Setup .NET | .NET 9 SDK installieren |
| Restore | NuGet Packages wiederherstellen |
| Build | Solution kompilieren |
| **Linter** | Code-Formatierung prüfen (`dotnet format --verify-no-changes`) |
| Install Playwright | Browser für E2E-Tests installieren |
| Run tests | Unit- und E2E-Tests ausführen |
| Upload test results | Test-Ergebnisse bei Fehler hochladen |

### 2. Deploy (`deploy.yaml`)

Wird bei Push auf `main` ausgeführt:

| Job | Beschreibung |
|-----|--------------|
| **dotnet-build** | Solution kompilieren |
| **test** | Linter + alle Tests ausführen |
| **build-and-push-image** | Docker Image bauen und auf ghcr.io pushen |

### Pipeline-Ablauf

```
PR erstellt/aktualisiert
        │
        ▼
┌───────────────────┐
│   PR-Validation   │
│  ┌─────────────┐  │
│  │    Build    │  │
│  └──────┬──────┘  │
│         ▼         │
│  ┌─────────────┐  │
│  │   Linter    │──┼──► Fehler? → PR blockiert
│  └──────┬──────┘  │
│         ▼         │
│  ┌─────────────┐  │
│  │    Tests    │──┼──► Fehler? → PR blockiert
│  └─────────────┘  │
└───────────────────┘
        │
        ▼ (nach Merge)
┌───────────────────┐
│      Deploy       │
│  ┌─────────────┐  │
│  │    Build    │  │
│  └──────┬──────┘  │
│         ▼         │
│  ┌─────────────┐  │
│  │ Linter+Test │  │
│  └──────┬──────┘  │
│         ▼         │
│  ┌─────────────┐  │
│  │ Docker Push │  │
│  └─────────────┘  │
└───────────────────┘
```

### Qualitätssicherung

- **Linter:** `dotnet format` prüft Code-Formatierung
- **Unit Tests:** Testen einzelne Services isoliert
- **E2E Tests:** Playwright testet die komplette Anwendung im Browser
- **Testabdeckung:** Unit + E2E Tests decken Auth, Projekte, Kriterien ab

## Projektstruktur

```
IPA_Manager/
├── .github/workflows/     # CI/CD Pipelines
├── Ipa.Manager/           # Hauptanwendung
│   ├── Auth/              # Authentifizierung
│   ├── Components/        # Wiederverwendbare UI-Komponenten
│   ├── Database/          # EF Core Context, Migrations, criteria.json
│   ├── Models/            # Datenmodelle
│   ├── Pages/             # Blazor Pages
│   └── Services/          # Business Logic
├── Ipa.Manager.Tests/     # Tests
│   ├── E2E/               # Playwright E2E Tests
│   └── UnitTests/         # Unit Tests
└── database/              # Docker Compose für MySQL
```

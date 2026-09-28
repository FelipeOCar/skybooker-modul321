# SkyBooker

**Felipe Carvalho · Modul 321 · Praktische Projektarbeit**

Flugverwaltung und Passagierbuchungen mit drei ASP.NET-Core-Microservices. Das Projekt enthält den bearbeitbaren C#-Quellcode, eine Visual-Studio-Solution, VS-Code-Konfiguration, Docker Compose, automatisierte Tests und Abgabeunterlagen.

## Was macht die Anwendung?

SkyBooker ist ein Backend für die Verwaltung von Flügen und Buchungen. Die Bedienung erfolgt über **Swagger** im Browser oder über **Postman**. Eine separate Website mit Buchungsformularen ist nicht Bestandteil des Projekts.

Der **AuthService** registriert Benutzer und stellt beim Login einen JWT-Zugangstoken aus. Der **FlightService** speichert und liefert Fluginformationen. Der **BookingService** nimmt Buchungen für den angemeldeten Benutzer entgegen. Vor dem Speichern fragt er den FlightService per HTTP ab, ob die Flugnummer existiert und der Flug noch nicht abgeflogen ist. Jeder Dienst verwendet seine eigene Datenbank.

Der typische Ablauf lautet: **registrieren, anmelden, Token übernehmen, Flug anlegen oder auswählen, buchen und eigene Buchungen abrufen**. Die folgende Anleitung erklärt diesen Ablauf nach dem Start der Dienste Schritt für Schritt.

## Inhaltsverzeichnis

- [Schnellstart mit Visual Studio](#schnellstart-mit-visual-studio)
- [Start mit Visual Studio Code](#start-mit-visual-studio-code)
- [Komplett in Docker starten](#komplett-in-docker-starten)
- [Voraussetzungen](#voraussetzungen)
- [Die Anwendung in Swagger bedienen](#die-anwendung-in-swagger-bedienen)
- [Testen](#testen)
- [Umsetzung und Funktionsumfang](#umsetzung)
- [Weiterbearbeiten und auf GitHub abgeben](#weiterbearbeiten-und-abgeben)

## Schnellstart mit Visual Studio

1. ZIP vollständig entpacken, beispielsweise nach `C:\Projekte\SkyBooker_Felipe_Carvalho`.
2. Docker Desktop starten, **Linux-Container** verwenden.
3. **`Start_Entwicklung.cmd`** ausführen. Warten auf «Datenbanken bereit».
4. **`SkyBooker.sln` doppelklicken**. Nicht nur den Ordner öffnen.
5. Rechtsklick auf die **Solution** → **Startprojekte konfigurieren / Eigenschaften** → **Mehrere Startprojekte**.
6. **AuthService**, **FlightService** und **BookingService** auf **Starten** setzen. **SkyBooker.Shared** und **SkyBooker.Tests** auf **Keine** lassen.
7. **F5** drücken. Bei den Webprojekten das Startprofil **http** verwenden.

| Dienst | Swagger | Datenbank |
| --- | --- | --- |
| AuthService | http://127.0.0.1:32510/swagger | SQLite, Tabelle `User` |
| FlightService | http://127.0.0.1:32511/swagger | MongoDB, Collection `flights` |
| BookingService | http://127.0.0.1:32512/swagger | SQL Server, Tabelle `Booking` |

Es gibt keine fest eingerichteten Benutzer. Zuerst registrieren, dann anmelden. Am einfachsten: **`Test_API.cmd`** ausführen oder die gesamte Postman-Collection starten.

## Start mit Visual Studio Code

1. Ebenfalls zuerst **`Start_Entwicklung.cmd`** ausführen.
2. **`SkyBooker.code-workspace`** öffnen und die empfohlenen C#-Erweiterungen installieren.
3. Unter **Ausführen und Debuggen** das Profil **SkyBooker – alle Dienste** auswählen.
4. **F5** drücken. Alle drei Dienste starten gemeinsam.

Einzelne Dienste lassen sich ebenfalls über die gleichnamigen Profile starten. Der Build läuft jeweils vor dem Start. Quellcode direkt in `src/` ändern, bei Bedarf Debugging stoppen und mit F5 neu starten.

## Komplett in Docker starten

**`Start_Docker.cmd`** führt die Einrichtung aus, baut die Images und startet alle Container. Visual Studio und ein lokal installiertes .NET SDK sind für diesen Modus nicht nötig. Docker Desktop muss laufen. Der erste Start benötigt Internet und kann einige Minuten dauern.

**`Stop_Docker.cmd`** beendet die Container und behält die Daten. Für den Wechsel von Docker zu F5 wieder `Start_Entwicklung.cmd` ausführen. Das stoppt die API-Container und lässt die Datenbanken laufen. Vor einem Wechsel von F5 zu Docker die drei lokalen APIs mit Shift+F5 stoppen.

## Voraussetzungen

- Windows x64 mit Docker Desktop und aktivierter Virtualisierung/WSL 2. Der SQL-Server-Container in dieser Lösung benötigt x64.
- Für F5: Visual Studio 2022 ab 17.14 mit **ASP.NET und Webentwicklung** und **.NET 9 SDK**, oder VS Code mit .NET 9 SDK und C#-Erweiterung.
- Für Git-Verwaltung: Git für Windows. Für die geforderte API-Demo: Postman.
- Ausreichend Arbeitsspeicher für Docker und die IDE, praktisch mindestens 8 GB RAM am Rechner; SQL Server benötigt davon eigenen freien Speicher.
- Die Ports `32510`, `32511`, `32512`, `32517` und `32533` müssen frei sein.

## Die Anwendung in Swagger bedienen

Alle drei Dienste müssen laufen. In Swagger den gewünschten Endpunkt öffnen, **Try it out** wählen, die Eingaben ausfüllen und **Execute** drücken. Die Antwort erscheint darunter mit HTTP-Status und Antwortdaten.

### 1. Benutzer registrieren

Im AuthService unter http://127.0.0.1:32510/swagger den Endpunkt **POST `/api/register`** öffnen:

```json
{
  "username": "felipe",
  "eMail": "felipe@example.com",
  "password": "SkyBooker!2026"
}
```

Das sind lokale Beispieldaten. Ein eigenes Passwort verwenden. Es benötigt 12–128 Zeichen, Gross- und Kleinbuchstaben, eine Ziffer und ein Sonderzeichen. Bei Erfolg kommt **201** zurück. Benutzername und E-Mail müssen eindeutig sein. Bei **409** einen anderen Namen und eine andere E-Mail verwenden oder mit dem bestehenden Konto anmelden. Die Datenbank speichert einen Passwort-Hash.

### 2. Anmelden und Token übernehmen

Im selben Swagger **POST `/api/login`** ausführen:

```json
{
  "username": "felipe",
  "password": "SkyBooker!2026"
}
```

Die Antwort enthält **`token`** und **`passengerId`**. Beide Werte für die nächsten Schritte übernehmen. Der Token gilt 15 Minuten. Danach erneut anmelden. Der ebenfalls vorhandene **GET `/api/login`** unterstützt Basic Auth; die genaue Verwendung steht in [Start und Fehlersuche](docs/START_UND_FEHLERSUCHE.md#jwt-in-swagger).

### 3. FlightService und BookingService autorisieren

http://127.0.0.1:32511/swagger und http://127.0.0.1:32512/swagger öffnen. In **beiden** Oberflächen oben auf **Authorize** klicken und beim Schema **Bearer** nur den Token einfügen, ohne Anführungszeichen und ohne das Wort `Bearer`. Mit **Authorize** bestätigen und das Fenster schliessen. Ohne gültigen Token antworten diese Dienste mit **401**.

### 4. Einen Flug anlegen oder auswählen

Im FlightService **POST `/api/flight`** ausführen:

```json
{
  "flightId": "LX123",
  "airlineName": "SWISS",
  "source": "Basel",
  "destination": "London",
  "departure_time": "2030-10-01T10:00:00Z",
  "arrival_time": "2030-10-01T12:00:00Z",
  "available_seats": 100
}
```

Die Beispielzeiten bei Bedarf ersetzen: Abflug in der Zukunft, Ankunft nach dem Abflug. `Z` kennzeichnet UTC. Start und Ziel müssen unterschiedlich sein. Die Flugnummer ist eindeutig; bei **409** eine neue verwenden. Erfolg: **201**.

Mit **GET `/api/flight`** alle Flüge lesen. **GET `/api/flight/{id}`** verwendet die automatisch erzeugte Datenbank-ID aus dem Feld `id`. Die Buchung verwendet hingegen die fachliche Flugnummer aus `flightId`, beispielsweise `LX123`. Ein bereits vorhandener zukünftiger Flug kann direkt gebucht werden.

### 5. Eine Buchung erstellen

Im BookingService **POST `/api/booking`** ausführen. **`passengerId` zwingend durch den eigenen Wert aus der Login-Antwort ersetzen**. Die `1` im folgenden Beispiel ist nur ein Platzhalter:

```json
{
  "flightId": "LX123",
  "passengerId": 1,
  "passengerFirstname": "Felipe",
  "passengerLastname": "Carvalho",
  "ticketCount": 2
}
```

Bei Erfolg antwortet die API mit **201** und der gespeicherten Buchung samt eigener `id`. Der BookingService prüft Eingaben und Benutzerzuordnung, fragt den Flug per HTTP ab und speichert anschliessend in SQL Server. Ungültige Daten ergeben **400**, eine fremde PassengerId **403**, ein unbekannter oder bereits abgeflogener Flug **422**. Bei einem nicht erreichbaren FlightService kommt **503**, ohne eine Buchung zu speichern.

### 6. Eigene Buchungen ansehen

**GET `/api/booking`** liefert die Buchungen des angemeldeten Benutzers. **GET `/api/booking/{id}`** zeigt eine einzelne eigene Buchung anhand ihrer Buchungs-ID. Fremde Buchungen werden nicht angezeigt. Für eine einzelne nicht sichtbare oder nicht existierende Buchung kommt **404**.

Die Anwendung unterstützt Erstellen und Lesen. Bearbeiten, Stornieren, Bezahlung und automatische Sitzplatzreduzierung sind nicht implementiert. Die Details zum gewählten Umfang stehen unter [Umsetzung](#umsetzung).

### Schneller Gesamttest

**`Test_API.cmd`** führt den Ablauf mit neuen Testdaten automatisch aus. Alternativ `postman/SkyBooker.postman_collection.json` in Postman importieren und im Collection Runner alle Requests in Reihenfolge ausführen. Dabei werden Token und IDs automatisch weitergegeben. Erwartet werden 20 erfolgreiche Statusprüfungen, darunter bewusst ausgelöste Fehlerantworten.

## Testen

| Aktion | Ausführung |
| --- | --- |
| Unit- und API-Integrationstests | `Test_Unit.cmd` oder `dotnet test SkyBooker.sln -c Release` |
| Die laufenden APIs prüfen | `Test_API.cmd` |
| Postman | `postman/SkyBooker.postman_collection.json` importieren und gesamte Collection in Reihenfolge starten |
| Einzelne Aufrufe in der IDE | `api/SkyBooker.http` |
| Automatische Prüfung nach GitHub-Push | `.github/workflows/ci.yml` |

Der API-Test legt pro Durchlauf einen neuen Testbenutzer, einen Flug und eine Buchung an. Die Daten bleiben für die Live-Demo erhalten. Die Prüfergebnisse und Grenzen stehen in [docs/TESTPROTOKOLL.md](docs/TESTPROTOKOLL.md).

## Umsetzung

- JWT schützt sämtliche Flight- und Booking-Endpunkte. Tokens gelten 15 Minuten.
- FluentValidation prüft Registrierungen, Login-Daten, Flüge und Buchungen.
- Passwörter werden mit `PasswordHasher` gesalzen gehasht, nicht im Klartext gespeichert.
- **AO1:** Serilog protokolliert Anfragen und fachliche Ereignisse in allen drei Diensten.
- **AO4:** BookingService validiert die Flugnummer über einen typisierten HTTP-Client beim FlightService.
- Jeder Dienst besitzt seine Datenbank und nutzt Entity Framework Core. Für MongoDB wird der offizielle EF-Core-Provider verwendet.

**Umfang der Buchung:** Das Projekt prüft, ob der Flug existiert und noch nicht abgeflogen ist. Die optionale Sitzplatzreservierung/Überbuchungssperre AO5 ist nicht implementiert. `available_seats` ist Flugstammdatum und wird bei Buchungen nicht reduziert.

## Weiterbearbeiten und abgeben

- [Start, Konfiguration und Fehlersuche](docs/START_UND_FEHLERSUCHE.md)
- [Architektur, Datenmodell und Entscheidungen](docs/ARCHITEKTUR.md)
- [Bewertungskriterien und Nachweise](docs/BEWERTUNG.md)
- [GitHub veröffentlichen und weiterarbeiten](docs/GITHUB.md)
- [Zeitplanung und Arbeitsaufteilung](docs/ZEITPLAN.md)
- [Live-Demo und Vortragsablauf](docs/DEMO.md)
- `docs/SkyBooker_Zeitplan.xlsx`: bearbeitbarer Gantt-Plan
- `docs/SkyBooker_Praesentation.pptx`: Präsentation mit Sprechernotizen

Das lokale Git-Repository enthält echte Commits der Erstellung. Es ist noch nicht mit GitHub verbunden. Vor dem ersten eigenen Commit die Git-E-Mail gemäss `docs/GITHUB.md` einstellen.

**Auftragsquelle:** https://github.com/INFEFZ/Modul321/blob/main/G4_Exams/2-practical-work.md

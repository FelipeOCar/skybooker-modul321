# GitHub und Weiterbearbeitung

## Was bereits enthalten ist

Die ZIP enthält ein lokales Git-Repository mit echten Commits der Erstellung. Es gibt noch kein Remote-Repository und keinen Push. Die Änderungen sind mit tatsächlichen Commit-Zeitpunkten festgehalten; die 12-Stunden-Planung ist kein nachträglich erfundener Arbeitsrapport.

Die Projektzuordnung lautet **Felipe Carvalho**. Die mitgelieferte lokale Git-E-Mail `felipe-carvalho@local.invalid` ist ausdrücklich ein Platzhalter und kein GitHub-Konto.

## Erster Upload

1. Auf GitHub ein **leeres** Repository mit dem Namen **`skybooker-modul321`** erstellen. Als Beschreibung passt: `Modul 321 – SkyBooker mit ASP.NET Core, JWT, MongoDB, SQL Server und SQLite – Felipe Carvalho`. Sichtbarkeit gemäss Vorgabe der Lehrperson wählen. Keine zusätzliche README, keine zusätzliche .gitignore und keine Lizenzdatei erzeugen, damit das neue Repository leer bleibt.
2. Das ZIP vollständig entpacken. Im Ordner **`SkyBooker_Felipe_Carvalho`**, in dem die Datei **`SkyBooker.sln`** liegt, ein Terminal öffnen. Im Windows-Explorer diesen Ordner öffnen, in die Adressleiste `powershell` eingeben und Enter drücken. Git für Windows muss installiert sein.
3. Name und eigene, auf GitHub verifizierte E-Mail setzen. Alternativ die persönliche GitHub-noreply-Adresse aus den GitHub-Einstellungen verwenden:

```powershell
git config user.name "Felipe Carvalho"
git config user.email "DEINE_EIGENE_GITHUB_EMAIL"
git status
git log --oneline
```

4. Die URL des eigenen Repositorys einsetzen und pushen:

```powershell
git remote add origin https://github.com/DEIN_BENUTZERNAME/skybooker-modul321.git
git push -u origin main
```

Die Platzhalter in den Befehlen ersetzen. Der Push überträgt den bestehenden Commit-Verlauf. Die GitHub-Webfunktion «Upload files» würde diesen Verlauf nicht übernehmen. In Visual Studio kann alternativ die vorhandene Git-Verwaltung zum Veröffentlichen des lokalen Repositorys verwendet werden.

Beim ersten Push kann Git eine Anmeldung im Browser verlangen. Mit dem GitHub-Konto anmelden, dem das Repository gehört. Anschliessend die Repository-Seite neu laden: `README.md`, `src`, `tests`, `docs` und die Solution-Datei sollten direkt auf der obersten Ebene sichtbar sein. Den Projektinhalt hochladen, nicht die ZIP-Datei als einzelne Datei.

Falls `git status` meldet, dass dies kein Git-Repository sei, zuerst prüfen, ob das Terminal wirklich im Ordner mit `SkyBooker.sln` geöffnet wurde und das komplette ZIP entpackt ist. Das Paket enthält bereits ein Git-Repository; eine erneute Initialisierung ist für dieses Paket nicht erforderlich.

## Weitere Änderungen

Quellcode in Visual Studio oder VS Code bearbeiten, testen und fachlich zusammengehörige Änderungen committen:

```powershell
dotnet test SkyBooker.sln -c Release
git status
git add src tests docs
git commit -m "feat: Beschreibung der eigenen Änderung"
git push
```

Neue oder geänderte Startskripte, Konfigurationen und Lockfiles bei Bedarf ebenfalls gezielt hinzufügen. Vor dem Commit `git diff --cached` prüfen. `.env`, `skybooker.local.json`, Datenbanken, Logs und Build-Ausgaben bleiben durch `.gitignore` ausgeschlossen.

## GitHub Actions

Nach dem Push läuft `.github/workflows/ci.yml`:

- Job **test** stellt gesperrte Paketversionen wieder her und führt die automatisierten Tests aus.
- Job **compose** erzeugt temporäre Zugangsdaten, baut die Container und prüft 20 API-Aufrufe gegen MongoDB, SQL Server und SQLite.

Der Compose-Job löscht am Ende nur seine kurzlebigen CI-Volumes. Das lokale `Stop_Docker.cmd` behält die Daten. Eine erfolgreiche CI-Ausführung auf GitHub ist erst nach einem tatsächlichen Push nachgewiesen. Vor der Abgabe das Ergebnis unter **Actions** prüfen.

Eine Container-Registry ist für den lokalen Betrieb nicht nötig. `docker compose up --build` erzeugt lokale Images und startet die Dienste. Die Veröffentlichung von Images auf Docker Hub oder GHCR erfolgt hier nicht automatisch.

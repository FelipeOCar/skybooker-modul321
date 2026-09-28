# Präsentation und Live-Demo

**Felipe Carvalho · Ziel: etwa 17–18 Minuten**

## Vor dem Vortrag

1. `Start_Docker.cmd` oder den F5-Modus vollständig starten.
2. `Test_Unit.cmd` und `Test_API.cmd` durchführen.
3. Postman-Collection importieren. Den Runner einmal erfolgreich ausführen und danach für die Demo einen neuen vollständigen Durchlauf starten.
4. Swagger der drei Dienste und das Terminal mit `docker compose logs -f auth-service flight-service booking-service` bereithalten.
5. In PowerPoint die Sprechernotizen nutzen. Das Fazit mit eigenen Erfahrungen ergänzen.

## Zeitaufteilung

| Abschnitt | Dauer |
| --- | ---: |
| Auftrag, Architektur und Plan | 3 Minuten |
| Datenmodell und ORM | 2 Minuten |
| JWT, Validierung und Fehlerverhalten | 3 Minuten |
| Zusatzanforderungen und Docker | 2 Minuten |
| Tests und Git-Verlauf | 1 Minute |
| Live-Demo | 5 Minuten |
| Fazit und Ausblick | 1–2 Minuten |

## Demo in Postman

1. **Registrierung**: Request 01. User-ID und Antwort ohne Passwort zeigen.
2. **Login**: Request 04. JWT und PassengerId erklären. Optional den geforderten GET-Login mit Request 05 zeigen.
3. **Schutz**: Request 06 ohne JWT ergibt 401.
4. **Flug**: Request 08 legt den Flug an, Request 10 liest ihn über seine Datenbank-ID.
5. **Validierung**: Request 14 ergibt 400 wegen TicketCount 0.
6. **Kommunikation**: Request 15 ergibt 422 wegen unbekannter Flugnummer; der BookingService fragt dafür den FlightService ab.
7. **Buchung**: Request 17 erstellt eine Buchung, Request 19 liest sie.
8. **Logging**: FlightLookup und Request-Status in den Konsolen/Containerlogs zuordnen.

Die Collection komplett in Reihenfolge ausführen, damit die automatisch gesetzten Variablen vorhanden sind. Für eine verkürzte Demo die vorbereitenden Requests 01 und 04 zuerst ausführen.

## Optionaler Ausfallnachweis

Nur im vollständigen Docker-Modus:

```powershell
docker compose stop flight-service
```

Dann Request 17 wiederholen: erwartet 503 und keine neue Buchung. Anschliessend:

```powershell
docker compose start flight-service
```

Auf den gesunden Container warten, bevor die nächste Buchung erfolgt. Der Test demonstriert die Fehlerbehandlung, keinen Überbuchungsschutz.

## Typische Fachfragen

**Warum drei Datenbanken?** Vorgabe der Aufgabe und getrennte Datenverantwortung pro Dienst.

**Warum Hashing statt Passwortverschlüsselung?** Beim Login wird der Hash geprüft. Das ursprüngliche Passwort muss nicht zurückgewonnen werden. Salt und Passwort-Hash machen gleiche Passwörter nicht als gleiche gespeicherte Werte erkennbar.

**Was schützt ein JWT?** Die Signatur schützt die Integrität der Claims. Die Nutzlast ist lesbar. Issuer, Audience und Ablaufzeit werden ebenfalls validiert.

**Warum 401 und 403?** 401 bedeutet fehlende/ungültige Authentifizierung. 403 bedeutet eine gültige Identität ohne Berechtigung für die betreffende PassengerId.

**Was passiert bei einem Dienstausfall?** Der HTTP-Client hat ein Zeitlimit. Der BookingService antwortet mit 503 und speichert keine Buchung.

**Was verhindert Überbuchung?** Diese Erweiterung ist nicht Bestandteil dieser Lösung. Für AO5 wären eine atomare Sitzplatzreservierung und ein konsistenter Fehler-/Kompensationsablauf nötig.

**Warum keine Fremdschlüssel über alle Datenbanken?** Die Dienste besitzen getrennte Datenbanken. Gültigkeit über die Dienstgrenze wird durch API-Kommunikation geprüft.

**Wie geht es mit Schemaänderungen weiter?** `EnsureCreated` initialisiert das erste Schema. Für Änderungen an bestehenden Daten ist ein geplanter Migrationsweg erforderlich.

## Fazit vorbereiten

Fachliche Erkenntnisse: Dienstgrenzen benötigen klare Verträge; Validierung und Fehlerfälle gehören zur API; ein fehlerfreier Build allein beweist noch keinen korrekten Betrieb mit allen Datenbanken. Eigene Erfahrungen aus dem lokalen Start und der Vorführung ergänzen, statt nicht selbst erlebte Probleme zu behaupten.

# Chatt-applikation (SignalR)

En enkel chattapplikation där flera användare kan ansluta och skicka meddelanden i realtid.
Byggd med ASP.NET Core 8 och SignalR. Klienten är en enkel HTML/JavaScript-sida.

## Krav
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

## Köra lokalt
```bash
# 1. Skapa och lita på utvecklingscertifikatet (self-signed) - görs en gång
dotnet dev-certs https --trust

# 2. Starta servern
dotnet run --launch-profile https
```
Öppna https://localhost:7100 i flera webbläsarflikar, välj olika användarnamn och chatta.

> Anslutningen laddar SignalR-klienten från cdnjs, så internet krävs vid första sidladdningen.

## Funktioner
- Flera användare chattar i realtid (SignalR)
- Användarnamn krävs för att chatta (måste vara unikt, max 20 tecken)
- WebSockets med automatisk fallback (SignalR väljer transport)
- Meddelanden skickas till alla anslutna och visas i allas gränssnitt
- Lista över vilka som är online, samt meddelande när någon går med/lämnar
- TLS (HTTPS) med self-signed utvecklingscertifikat

## Säkerhet

### Vad TLS skyddar
- **Konfidentialitet:** ingen på nätverket kan läsa meddelandena mellan webbläsare och server.
- **Integritet:** trafiken kan inte ändras utan att det upptäcks.
- **Serverautentisering:** klienten kan verifiera att den pratar med rätt server (med ett riktigt certifikat).

### Vad TLS inte skyddar
- Servern ser alla meddelanden i klartext (det är ingen end-to-end-kryptering).
- Skyddet gäller bara transporten, inte lagrade data eller en komprometterad server/klient.
- Self-signed certifikat är inte verifierade av en tredje part (CA), därför måste man själv lita på det. Det är okej i utveckling men inte i produktion.
- Ingen riktig användarautentisering: vem som helst kan välja vilket (ledigt) namn som helst.

### Övriga val
- Servern bestämmer avsändarens namn, klienten kan inte låtsas vara någon annan.
- Servern validerar längd på namn och meddelanden.
- Klienten visar text med `textContent` för att förhindra XSS.

## Projektstruktur
| Fil | Syfte |
|---|---|
| `Program.cs` | Startar servern, aktiverar SignalR, HTTPS och statiska filer |
| `ChatHub.cs` | Serverlogik: användarnamn, skicka meddelanden, koppla ner |
| `wwwroot/index.html` | Klienten (UI + SignalR-anslutning) |
| `Properties/launchSettings.json` | HTTPS-adress för lokal körning |

## Hur jag har använt AI
Jag har använt AI (Claude) som hjälp under arbetet. AI:n tog fram en första version av koden och förklarade hur SignalR, WebSockets och TLS fungerar. Efteråt gick jag igenom koden för att förstå hur den fungerar, bland annat hur ChatHub tar emot och skickar vidare meddelanden, hur användarnamn kontrolleras av servern och hur klienten ansluter. Jag körde applikationen lokalt med flera webbläsarflikar och testade att skicka meddelanden, att välja ett upptaget användarnamn och att koppla ner en användare. I webbläsarens DevTools kontrollerade jag att anslutningen använder WebSockets över TLS (wss://). Jag kan förklara koden och de tekniska valen vid redovisningen.
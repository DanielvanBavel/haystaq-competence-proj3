# haystaq-competence-proj3 - BezorgBaas

> Competence-dag, opdracht 3: **"Testautomatisering schrijven kost veel programmeerwerk."**
> De opdracht staat in [MISSION.md](MISSION.md). Dit bestand beschrijft de applicatie.

BezorgBaas is een maaltijdbezorgplatform: restaurants zoeken en filteren, gerechten
met opties in een winkelmandje leggen, afrekenen met een bezorgadres, tijdvak en
betaling, de bestelling volgen, en aan de andere kant bestellingen afhandelen als
restaurant.

## Stack

| Laag | Technologie |
| --- | --- |
| Backend | C# / .NET 9, ASP.NET Core, EF Core (Npgsql) |
| Database | PostgreSQL 16, schema en seed als SQL-scripts |
| Frontend | React 18 + TypeScript, Vite, nginx |
| Tests | Playwright (TypeScript) |
| Architectuur | Domain Driven Design: Domain, Application, Infrastructure, Api |

Zie [docs/architecture.md](docs/architecture.md).

## Snel starten

```bash
docker compose up -d --build
```

- UI: <http://localhost:3003>
- API: <http://localhost:8083/api/restaurants>
- Health: <http://localhost:8083/health>
- Postgres: `localhost:5435`, database/gebruiker/wachtwoord `bezorgbaas`

Poorten bezet? Zie [.env.example](.env.example).

Alles terugzetten naar de begintoestand:

```bash
curl -X POST http://localhost:8083/api/test-support/reset
```

## Tests draaien

```bash
cd tests && npm install && npx playwright install chromium && npm test
```

Er staat één handgeschreven test in `tests/specs`: de gouden voorbeeldtest voor
US-01. Alle andere stories moeten nog. Zie [tests/README.md](tests/README.md).

## Wat de app kan

| Scherm | Wat er gebeurt |
| --- | --- |
| Restaurants | zoeken met debounce van 300 ms, filteren op keuken, bezorgtijd en open/gesloten |
| Restaurantpagina | menukaart per categorie, gerecht openen in een dialoog, maat kiezen (verplicht), extra's aanvinken, aantal instellen, melding die vanzelf verdwijnt |
| Winkelmandje | blijft bewaard in localStorage, telt regels op, wisselt van restaurant |
| Afrekenen | adresformulier met postcodecontrole, bezorgmoment uit een lijst, actiecode toepassen, betalen in een **iframe** van de betaaldienst |
| Bestelling | statuspagina die zichzelf ververst, annuleren zolang het mag |
| Restaurantbeheer | bestellingen accepteren, afwijzen en door de statussen zetten |

Deze schermen zijn met opzet niet allemaal even makkelijk te automatiseren:
er zit een dialoog in, een iframe, een debounce, een melding die verdwijnt en
een pagina die zichzelf ververst.

## Testgegevens

| Onderwerp | Waarde |
| --- | --- |
| Restaurants | 6, waarvan De Groene Pan gesloten is |
| Uitverkocht | Truffelpizza bij Pizzeria De Vuurplaat |
| Minimaal bestelbedrag pizzeria | 15.00, bezorgkosten 2.50, gratis vanaf 30.00 |
| Actiecodes | `WELKOM10`, `GRATISBEZORGD`, `VIJFEURO`, `PIZZA20`, `ZOMER2024` (verlopen), `OPOP` (nog één keer) |
| Geweigerde kaart | elk nummer dat eindigt op `0000` |
| Reset | `POST /api/test-support/reset` |

## API in het kort

| Methode | Pad | Doel |
| --- | --- | --- |
| GET | `/api/restaurants` | Zoeken en filteren |
| GET | `/api/restaurants/{slug}` | Restaurant met menukaart |
| GET | `/api/cuisines`, `/api/delivery-slots` | Keuzelijsten |
| POST | `/api/orders/quote` | Prijs berekenen zonder te bestellen |
| POST | `/api/orders` | Bestelling plaatsen |
| GET | `/api/orders/{orderNumber}` | Bestelling opvragen |
| GET | `/api/orders?email=` | Bestellingen van een klant |
| GET | `/api/restaurants/{id}/orders` | Bestellingen van een restaurant |
| POST | `/api/orders/{id}/advance` | Naar de volgende status |
| POST | `/api/orders/{id}/cancel` | Annuleren |
| POST | `/api/payments/authorize` | Betaling bij de nagebootste betaaldienst |
| POST | `/api/test-support/reset` | Gegevens terugzetten |

Fouten zijn hier juist wél duidelijk: `{"code":"order.below_minimum","message":"Het
minimale bestelbedrag bij Pizzeria De Vuurplaat is 15.00 euro."}`. Je kunt er dus
op asserten.

## Zonder Docker draaien (optioneel)

```bash
docker compose up -d db
```

```bash
cd backend && DB_PORT=5435 DB_HOST=localhost dotnet run --project src/BezorgBaas.Api
```

```bash
cd frontend && npm install && npm run dev
```

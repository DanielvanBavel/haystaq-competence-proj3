# Playwright-tests

De applicatie moet draaien voordat je test:

```bash
docker compose up -d --build
```

## Lokaal draaien (aanbevolen)

```bash
cd tests && npm install && npx playwright install chromium
```

```bash
npm test
```

Rapport bekijken:

```bash
npm run report
```

## Zonder Node op je laptop

```bash
docker run --rm --network host -v "%cd%\tests:/work" -w /work mcr.microsoft.com/playwright:v1.49.1-noble bash -lc "npm install && npx playwright test"
```

De image is groot (ongeveer 1,5 GB). Als iedereen dat tegelijk doet, is de
wifi op de zaak sneller op dan je lief is.

## Wat er al staat

```
tests/
├── playwright.config.ts        baseURL, timeouts, rapportage
├── support/fixtures.ts         reset van de gegevens voor elke test
├── support/pages.ts            page objects voor home, restaurant en afrekenen
└── specs/us-01-bestellen.spec.ts   de gouden voorbeeldtest
```

De fixture roept voor elke test `POST /api/test-support/reset` aan en leegt het
winkelmandje. Zonder die reset worden tests afhankelijk van elkaar.

## Afspraken

- Wachten op gedrag, nooit op tijd. Geen `waitForTimeout`.
- Controleer wat de gebruiker ziet, niet wat de API teruggeeft - tenzij de story
  daar expliciet over gaat.
- Gebruik `data-testid` waar het staat, en anders rollen en tekst. Niet alle
  elementen hebben een testid, en dat blijft ook zo.
- Eén story per bestand, met het nummer in de bestandsnaam.
- Page objects bevatten geen assertions over inhoud; die horen in de test. De
  uitzondering zijn wachtmomenten die het scherm stabiel maken.

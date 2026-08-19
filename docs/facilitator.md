# Facilitator - achtergrond bij opdracht 3

> Voor de begeleiding van de dag. Bevat de antwoorden op de open stories.

## De verborgen regels

| Regel | Waarde | Waar |
| --- | --- | --- |
| Limiet contant betalen (US-08) | totaal **tot en met 50,00 euro** | `Order.CashLimit` |
| Maat verplicht bij gerechten met maten | precies één maat | `MenuItem.PriceWith` |
| Annuleren mag | alleen bij Placed en Accepted | `Order.Cancel` |
| Statusvolgorde | Placed → Accepted → Preparing → OnTheWay → Delivered | `Order.Allowed` |
| Postcode | `1234 AB`, spatie optioneel bij invoer | `DeliveryAddress` |
| Aantal per regel | 1 tot en met 20 | `OrderLine.For` |
| Kaart geweigerd | nummer eindigt op `0000` | `PaymentsController` |
| Bezorgmomenten vandaag | pas vanaf 45 minuten vanaf nu | `CatalogController.Slots` |

## De twee onvolledige stories

**US-11 (gesloten restaurant).** De applicatie laat je de menukaart bekijken en
zelfs items in je mandje leggen bij een gesloten restaurant. Pas bij het
plaatsen van de bestelling volgt `409 restaurant.closed`. Dat is verdedigbaar,
maar het is niet wat de meeste mensen verwachten: je kunt een heel mandje vullen
voordat je het hoort. Een goede analyse benoemt dat als bevinding, niet als test.

**US-12 (ander restaurant).** Het winkelmandje wordt **zonder melding geleegd**
zodra je iets toevoegt bij een ander restaurant (`CartContext.add`). Er is geen
pop-up. Een agent die de story letterlijk volgt, schrijft een test op een
bevestigingsdialoog die niet bestaat. Dat is precies het gesprek dat je wilt:
test je de story of de applicatie?

## Waar generatoren op stuk lopen

1. **Het betaal-iframe.** Vereist `frameLocator('[data-testid="payment-iframe"]')`.
   Wie de velden op de pagina zelf zoekt, vindt niets.
2. **De debounce van 300 ms in het zoekveld.** Vaste wachttijden maken de test
   traag of instabiel; wachten op het resultaat werkt wel.
3. **De melding na toevoegen** verdwijnt na 3 seconden. Een assertie die te laat
   komt, faalt zonder dat er iets mis is.
4. **De statuspagina ververst elke 5 seconden.** Wie zelf gaat pollen, krijgt
   dubbele aanroepen en flakiness.
5. **Prijzen met opties.** Margherita groot met extra kaas is 14,00 per stuk;
   twee stuks is 28,00 en dan gelden de bezorgkosten nog (28,00 < 30,00).
   Een generator die de prijs "ongeveer" berekent, valt hier door de mand.
6. **De maatkeuze in de dialoog.** Zonder maat volgt `400 option.size_required`.

## Suggestie voor de tijdsindeling

- Laat iedereen eerst met de hand US-03 schrijven. Zonder die ervaring kunnen ze
  de gegenereerde tests niet beoordelen.
- Vraag halverwege om `npx playwright test --repeat-each=5`. Flakiness komt pas
  dan naar boven.
- Bewaar US-11 en US-12 voor het einde en gebruik ze voor de discussie: wat doe
  je als de story niet klopt met de applicatie?

## Handige commando's

```bash
docker compose logs -f api
```

```bash
docker compose exec db psql -U bezorgbaas -d bezorgbaas -c "select order_number, status, total from customer_order"
```

```bash
curl -X POST http://localhost:8083/api/test-support/reset
```

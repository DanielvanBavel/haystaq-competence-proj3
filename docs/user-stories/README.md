# User stories BezorgBaas

Twaalf stories, zoals ze in het echt bij je binnenkomen: sommige netjes in
Gherkin, sommige als lopende tekst uit een refinement, en een paar die niet af
zijn. Dat laatste is met opzet.

| Story | Onderwerp | Vorm | Bijzonderheid |
| --- | --- | --- | --- |
| US-01 | Bestellen en betalen | tekst | uitgewerkt als voorbeeldtest in `tests/specs` |
| US-02 | Zoeken en filteren | tekst | debounce van 300 ms |
| US-03 | Minimaal bestelbedrag | tekst | |
| US-04 | Bezorgkosten en gratis bezorgen | tekst | |
| US-05 | Actiecodes | Gherkin | scenario outline met voorbeelden |
| US-06 | Uitverkochte gerechten | Gherkin | |
| US-07 | Betaling geweigerd | Gherkin | speelt zich af in een iframe |
| US-08 | Contant betalen | tekst | grensbedrag staat er niet in |
| US-09 | Bestelling volgen en annuleren | tekst | statuspagina ververst zichzelf |
| US-10 | Restaurantbeheer | tekst | |
| US-11 | Gesloten restaurant | tekst | acceptatiecriteria zijn onvolledig |
| US-12 | Winkelmandje bij een ander restaurant | tekst | gedrag is niet beschreven |

## Wat de tester ermee moet

Niet elke story is even geschikt om te automatiseren, en niet elke story is
compleet. Bij US-11 en US-12 zul je moeten vaststellen wat de applicatie
werkelijk doet en of dat wenselijk is. Een agent die daar zonder aarzelen een
test voor schrijft, test zijn eigen aanname.

## Testgegevens

| Gegeven | Waarde |
| --- | --- |
| Restaurant met opties en minimumbedrag | Pizzeria De Vuurplaat (minimaal 15.00, bezorgkosten 2.50, gratis vanaf 30.00) |
| Gesloten restaurant | De Groene Pan |
| Uitverkocht gerecht | Truffelpizza bij Pizzeria De Vuurplaat |
| Actiecodes | `WELKOM10` (10%, vanaf 20.00, eenmalig per klant), `GRATISBEZORGD` (vanaf 15.00), `VIJFEURO` (5.00 korting vanaf 25.00), `PIZZA20` (alleen bij de pizzeria), `ZOMER2024` (verlopen), `OPOP` (nog één keer te gebruiken) |
| Kaart die wordt geweigerd | elk nummer dat eindigt op `0000` |
| Reset van de gegevens | `POST http://localhost:8083/api/test-support/reset` |

# language: nl
Functionaliteit: Actiecodes
  Als marketeer wil ik acties kunnen voeren met kortingscodes
  zodat klanten vaker bestellen.

  Achtergrond:
    Gegeven ik heb een winkelmandje bij "Pizzeria De Vuurplaat"

  Scenario: Geldige code geeft korting
    Gegeven mijn subtotaal is 25,00 euro
    Als ik de actiecode "WELKOM10" toepas
    Dan zie ik een korting van 2,50 euro
    En het totaalbedrag is met 2,50 euro verlaagd

  Scenario: Code onder het minimumbedrag telt niet mee
    Gegeven mijn subtotaal is 16,50 euro
    Als ik de actiecode "WELKOM10" toepas
    Dan zie ik de melding dat de code geldt vanaf 20,00 euro
    En er wordt geen korting toegepast
    En ik kan de bestelling gewoon afronden

  Scenario: Verlopen code
    Gegeven mijn subtotaal is 30,00 euro
    Als ik de actiecode "ZOMER2024" toepas
    Dan zie ik de melding dat de code is verlopen
    En er wordt geen korting toegepast

  Scenario: Code die bij een ander restaurant hoort
    Gegeven ik heb een winkelmandje bij "Sushi Noord"
    En mijn subtotaal is 35,00 euro
    Als ik de actiecode "PIZZA20" toepas
    Dan zie ik de melding dat de code niet geldt bij dit restaurant

  Scenario: Dezelfde code twee keer gebruiken
    Gegeven ik heb eerder besteld met de actiecode "WELKOM10" op "sanne@example.com"
    En mijn subtotaal is 25,00 euro
    Als ik opnieuw bestel met "WELKOM10" op hetzelfde e-mailadres
    Dan wordt de bestelling geweigerd met de melding dat de code al gebruikt is

  Abstract Scenario: Verschillende soorten korting
    Gegeven mijn subtotaal is <subtotaal> euro
    Als ik de actiecode "<code>" toepas
    Dan is de korting <korting> euro

    Voorbeelden:
      | code          | subtotaal | korting |
      | WELKOM10      | 25,00     | 2,50    |
      | VIJFEURO      | 26,00     | 5,00    |
      | GRATISBEZORGD | 20,00     | 2,50    |

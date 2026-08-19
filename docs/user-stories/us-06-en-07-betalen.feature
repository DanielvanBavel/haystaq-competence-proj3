# language: nl
Functionaliteit: Uitverkochte gerechten en betalingen
  Als bezoeker wil ik duidelijk zien wat kan en wat niet
  zodat ik niet halverwege vastloop.

  Scenario: US-06 Uitverkocht gerecht kan niet besteld worden
    Gegeven ik bekijk de pagina van "Pizzeria De Vuurplaat"
    Dan zie ik bij "Truffelpizza" de vermelding "Uitverkocht"
    En de knop "Toevoegen" bij "Truffelpizza" is uitgeschakeld

  Scenario: US-07 Betaling met creditcard wordt geweigerd
    Gegeven ik heb voor 30,50 euro in mijn winkelmandje bij "Pizzeria De Vuurplaat"
    En ik heb mijn naam, e-mailadres en bezorgadres ingevuld
    Als ik betaalmethode "Creditcard" kies
    En ik in het betaalscherm kaartnummer "4111 1111 1111 0000" invul
    En ik de betaling bevestig
    Dan zie ik de melding dat de betaling is geweigerd
    En ik kan de bestelling niet plaatsen

  Scenario: US-07 Betaling met creditcard slaagt
    Gegeven ik heb voor 30,50 euro in mijn winkelmandje bij "Pizzeria De Vuurplaat"
    En ik heb mijn naam, e-mailadres en bezorgadres ingevuld
    Als ik betaalmethode "Creditcard" kies
    En ik in het betaalscherm kaartnummer "4111 1111 1111 1111" invul
    En ik de betaling bevestig
    Dan zie ik de melding dat de betaling is bevestigd
    En kan ik de bestelling plaatsen

  Scenario: US-07 iDEAL zonder bankkeuze
    Gegeven ik sta op de afrekenpagina met betaalmethode "iDEAL"
    Als ik de betaling bevestig zonder een bank te kiezen
    Dan zie ik in het betaalscherm de melding dat ik een bank moet kiezen

# Let op: het betaalscherm is een iframe van de betaaldienst. De velden daarin
# horen niet bij de pagina zelf.

# Architectuur

BezorgBaas is een modulaire monoliet in .NET, ingedeeld volgens Domain Driven
Design. Vier projecten, met afhankelijkheden die maar één kant op wijzen:

```
BezorgBaas.Api  ->  BezorgBaas.Infrastructure  ->  BezorgBaas.Application  ->  BezorgBaas.Domain
```

| Project | Verantwoordelijkheid |
| --- | --- |
| `BezorgBaas.Domain` | Aggregates, waarde-objecten en regels. Kent geen EF Core en geen HTTP. |
| `BezorgBaas.Application` | Use cases en leesmodellen. Coordineert aggregates via de poorten. |
| `BezorgBaas.Infrastructure` | EF Core: mapping en repositories. |
| `BezorgBaas.Api` | Controllers, foutafhandeling en de nagebootste betaaldienst. |

## Bounded contexts

| Context | Aggregate root | Bevat | Belangrijkste regels |
| --- | --- | --- | --- |
| Catalog | `Restaurant` | `MenuItem`, `MenuItemOption` | open of gesloten, minimaal bestelbedrag, bezorgkosten met gratis-drempel, verplichte maatkeuze |
| Ordering | `Order` | `OrderLine`, `OrderStatusChange`, `DeliveryAddress` | postcodeformaat, statusovergangen, annuleren tot in de keuken, limiet voor contant betalen |
| Promotions | `PromoCode` | - | geldigheidsduur, minimumbedrag, restaurantbinding, eenmalig per klant |

De poorten (`IRestaurantRepository`, `IOrderRepository`, `IPromoCodeRepository`)
staan in `BezorgBaas.Domain/Ports.cs`. De applicatielaag kent alleen die
interfaces.

## Waarde-objecten

- `Money` - bedrag met twee decimalen, nooit negatief, met eigen rekenregels.
  Wordt met een EF-conversie op een `numeric(8,2)` gemapt.
- `DeliveryAddress` - straat, huisnummer, postcode, plaats. Valideert het
  Nederlandse postcodeformaat en normaliseert naar `1234 AB`. Gemapt als owned
  entity in dezelfde tabel.

## Database

Het schema staat in `db/init/01_schema.sql` en wordt door Postgres uitgevoerd bij
de eerste start. EF Core maakt niets aan en gebruikt geen migraties: de mapping
volgt het schema. `db/init/02_seed.sql` bevat de testgegevens en wordt ook door
`POST /api/test-support/reset` opnieuw uitgevoerd.

Let op één EF-detail dat in dit project belangrijk is: alle sleutels zijn
`ValueGeneratedNever()`. Het domein maakt de `Guid` zelf aan. Zonder die instelling
denkt EF Core dat een nieuw statushistorie-record al bestaat en wordt het een
UPDATE die niets raakt.

## Foutmodel

Het domein gooit `DomainException` met een `Kind` en een `Code`. De middleware in
`Program.cs` vertaalt dat naar HTTP:

| Kind | HTTP | Voorbeeld |
| --- | --- | --- |
| Invalid | 400 | `address.postal_code_invalid` |
| Conflict | 409 | `order.below_minimum`, `payment.cash_limit` |
| NotFound | 404 | `restaurant.not_found` |

Anders dan in opdracht 1 zijn deze meldingen expres wél informatief: je moet er
tests op kunnen schrijven.

## Frontend

React met React Router. De winkelmandje-status staat in een context en wordt in
`localStorage` bewaard. Betalen gebeurt in `public/payment-mock.html`, dat als
iframe wordt geladen en het resultaat via `postMessage` terugstuurt naar de
afrekenpagina - net zoals een echte betaaldienst dat doet.

Niet alle elementen hebben een `data-testid`. Dat is een bewuste keuze: een
testgenerator moet ook met rollen en teksten overweg kunnen.

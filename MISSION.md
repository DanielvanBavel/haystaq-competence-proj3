# Opdracht 3 - Tests die zichzelf schrijven

**Probleem uit de praktijk:** *"Testautomatisering schrijven kost veel programmeerwerk."*
Er liggen twaalf user stories. Ze automatiseren kost dagen, en tegen de tijd dat
je klaar bent, zijn er alweer nieuwe stories.

**AI-richting:** een agent die Playwright-tests genereert vanuit user stories en
Gherkin-scenario's - en die tests draaien ook echt.

---

## 1. De situatie

BezorgBaas draait (zie [README.md](README.md)). In
[docs/user-stories](docs/user-stories) staan twaalf stories: sommige als nette
Gherkin, sommige als lopende tekst, en twee die niet af zijn.

Er is één test: `tests/specs/us-01-bestellen.spec.ts`. Die is met de hand
geschreven en is je maatstaf. Alle andere stories moeten nog.

De applicatie werkt mee waar dat mag (`/api/test-support/reset`, duidelijke
foutcodes) en werkt tegen waar dat realistisch is: een dialoog, een iframe voor
de betaling, een zoekveld met debounce, een melding die na drie seconden
verdwijnt en een statuspagina die zichzelf ververst.

## 2. Wat je bouwt

Een **testgenerator**: een agent, MCP-server of skill die van een story een
werkende Playwright-test maakt. Minimale scope:

| Component | Wat het moet doen |
| --- | --- |
| **Story inlezen** | Zowel Gherkin als lopende tekst aankunnen, en de acceptatiecriteria eruit halen. |
| **Applicatie verkennen** | Ontdekken welke selectors er zijn. Niet gokken: kijken. Via de DOM, de broncode van `frontend/src`, of door de app te bedienen. |
| **Test genereren** | Een spec die past bij de conventies uit `tests/README.md` en de bestaande page objects hergebruikt. |
| **Draaien en herstellen** | De test uitvoeren, de fout lezen en zichzelf corrigeren. Een gegenereerde test die niet draait, telt niet. |
| **Page objects onderhouden** | Nieuwe schermen krijgen een page object; bestaande worden hergebruikt, niet gedupliceerd. |

Voorstel voor MCP-tools:

```
list_stories()                  -> stories met id, vorm en status
read_story(id)                  -> de tekst
inspect_page(url)               -> DOM-structuur, testids, rollen en teksten
run_tests(pattern?)             -> draait Playwright, geeft resultaat en foutmelding terug
write_spec(id, content)         -> schrijft naar tests/specs
reset_app()                     -> POST /api/test-support/reset
```

## 3. Aanpak in fases

Reken op ongeveer vier uur.

**Fase 0 - De maatstaf begrijpen (30 min)**
Draai de gouden test. Lees hem regel voor regel. Schrijf met de hand de test voor
US-03 (minimaal bestelbedrag). Klok hoe lang je erover doet.

**Fase 1 - Eén story, één test (45 min)**
Laat de agent US-06 genereren (uitverkocht gerecht). Klein, duidelijk, weinig
stappen. Laat hem draaien tot hij groen is.

**Fase 2 - De lastige interacties (60 min)**
US-07 gaat over de betaling in een iframe, US-02 over de debounce. Hier gaat een
naïeve generator onderuit met `waitForTimeout`. Zorg dat je agent weet hoe het
wel moet, bijvoorbeeld door `tests/README.md` als context mee te geven.

**Fase 3 - Op schaal (45 min)**
Genereer US-02 tot en met US-10 in één run. Meet: hoeveel tests zijn in één keer
groen, hoeveel na één correctieronde, hoeveel nooit?

**Fase 4 - De onvolledige stories (30 min)**
US-11 en US-12 zijn niet af. Laat de agent eerst vaststellen wat de applicatie
doet en expliciet benoemen welke aanname hij maakt. Een agent die hier zonder
aarzelen een test voor schrijft, test zijn eigen fantasie.

**Fase 5 - Demo (15 min)**
Nieuwe story erbij, agent erop, groene test. En laat zien wat er misging.

## 4. Definition of done

- [ ] Voor minimaal zes stories staat er een gegenereerde test die groen draait.
- [ ] De gegenereerde tests gebruiken de bestaande page objects en fixtures.
- [ ] Er staat nergens een vaste wachttijd in de gegenereerde tests.
- [ ] De agent draait de tests zelf en herstelt zijn eigen fouten.
- [ ] Voor US-11 en US-12 heeft de agent zijn aannames opgeschreven in plaats van
      ze te verzinnen.
- [ ] Een collega kan een nieuwe story toevoegen en er met één commando een test
      voor laten maken.
- [ ] Alles staat in deze repo, in een branch met een pull request.

## 5. De lat: wat een goede gegenereerde test doet

- Begint bij een schone omgeving (fixture, niet handmatig).
- Controleert wat de gebruiker ziet, niet wat de API teruggeeft.
- Faalt om de juiste reden: als de bedrag-assertie verkeerd is, moet de melding
  dat duidelijk maken.
- Blijft leesbaar: iemand die de story kent, herkent de stappen terug.
- Draait tien keer achter elkaar met hetzelfde resultaat.

Probeer dat laatste ook echt: `npx playwright test --repeat-each=5`.

## 6. Stretch goals

- Laat de agent de dekking bepalen: welke acceptatiecriteria zijn nog niet
  getest?
- Laat hem ontbrekende `data-testid`-attributen voorstellen als diff op de
  frontend, in plaats van fragiele selectors te gebruiken.
- Voeg visuele regressie toe voor één scherm, en beoordeel of dat waarde heeft.
- Genereer naast UI-tests ook API-tests voor dezelfde story, en bepaal welke
  laag welk criterium hoort te bewaken.
- Laat de agent een testrapport schrijven dat de product owner begrijpt.

## 7. Valkuilen

- **Selectors verzinnen.** Laat de agent de pagina echt bekijken.
- **De iframe vergeten.** Het betaalscherm is een aparte context
  (`frameLocator`).
- **`waitForTimeout` als oplossing voor flakiness.** Wachten op gedrag, niet op
  tijd.
- **Geen reset tussen tests.** Dan zijn ze afhankelijk van de volgorde.
- **Alles in één test proppen.** Eén story, één test, meerdere assertions.
- **Groen is niet goed.** Een test die niets controleert, is ook groen.

## 8. Wat je oplevert

1. Werkende code in deze repo: generator, MCP-server of skill.
2. De gegenereerde tests in `tests/specs`, groen.
3. Een demo van maximaal 10 minuten.
4. Eén alinea: hoeveel tests waren in één keer goed, waar ging het mis, en zou
   je dit een collega aanraden?

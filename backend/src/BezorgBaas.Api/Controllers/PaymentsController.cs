using BezorgBaas.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace BezorgBaas.Api.Controllers;

/// <summary>
/// Nagebootste betaalprovider. Wordt aangeroepen vanuit de iframe op de
/// afrekenpagina, precies zoals een echte PSP dat zou doen.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    public record AuthorizeRequest(string Method, string? CardNumber, string? Bank, decimal Amount);

    public record AuthorizeResponse(string Status, string? Reference, string? Message);

    [HttpPost("authorize")]
    public ActionResult<AuthorizeResponse> Authorize([FromBody] AuthorizeRequest request)
    {
        string method = (request.Method ?? string.Empty).ToLowerInvariant();

        if (method == "ideal")
        {
            DomainException.Require(!string.IsNullOrWhiteSpace(request.Bank), "payment.bank_required",
                "Kies je bank.");
            return Ok(new AuthorizeResponse("approved", NewReference("IDL"), null));
        }

        if (method == "card")
        {
            string digits = new((request.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            DomainException.Require(digits.Length is >= 12 and <= 19, "payment.card_invalid",
                "Vul een geldig kaartnummer in.");

            // Kaarten die eindigen op 0000 worden altijd geweigerd. Handig om te testen.
            if (digits.EndsWith("0000"))
            {
                return Ok(new AuthorizeResponse("declined", null, "De betaling is geweigerd door je bank."));
            }
            return Ok(new AuthorizeResponse("approved", NewReference("CRD"), null));
        }

        if (method == "cash")
        {
            return Ok(new AuthorizeResponse("approved", null, "Contant betalen bij de deur."));
        }

        throw DomainException.Invalid("payment.method_unknown", "Kies een geldige betaalmethode.");
    }

    private static string NewReference(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
}

using Microsoft.AspNetCore.Mvc;

namespace BezorgBaas.Api.Controllers;

/// <summary>
/// Alleen voor testomgevingen: de database terugzetten naar de begintoestand.
/// Zo kan elke testrun met dezelfde gegevens beginnen.
/// </summary>
[ApiController]
[Route("api/test-support")]
public class TestSupportController : ControllerBase
{
    private readonly DatabaseAdmin _admin;

    public TestSupportController(DatabaseAdmin admin)
    {
        _admin = admin;
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        string seedPath = Environment.GetEnvironmentVariable("SEED_FILE") ?? "/app/db-init/02_seed.sql";
        await _admin.ResetAsync(seedPath);
        return Ok(new { status = "reset", seed = Path.GetFileName(seedPath) });
    }
}

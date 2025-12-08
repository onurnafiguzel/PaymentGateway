using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Providers;

namespace ProviderPayTR.Api.Controllers;

[ApiController]
[Route("api/provider-paytr")]
public class PayTRController : ControllerBase
{
    [HttpPost("initiate")]
    public IActionResult Initiate([FromBody] ProviderPaymentRequest request)
    {
        Console.WriteLine($"PAYTR received payment: {request.PaymentId}");
        return Ok(new { success = true });
    }
}

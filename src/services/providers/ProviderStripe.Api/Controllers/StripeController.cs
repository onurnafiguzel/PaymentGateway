using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Providers;

namespace ProviderStripe.Api.Controllers;

[ApiController]
[Route("api/provider-stripe")]
public class StripeController : ControllerBase
{
    [HttpPost("initiate")]
    public IActionResult Initiate([FromBody] ProviderPaymentRequest request)
    {
        Console.WriteLine($"Stripe received payment: {request.PaymentId}");
        return Ok(new { success = true });
    }
}

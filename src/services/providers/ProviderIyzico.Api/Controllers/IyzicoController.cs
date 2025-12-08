using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Providers;

namespace ProviderIyzico.Api.Controllers;

[ApiController]
[Route("api/provider-iyzico")]
public class IyzicoController : ControllerBase
{
    [HttpPost("initiate")]
    public IActionResult Initiate([FromBody] ProviderPaymentRequest request)
    {
        Console.WriteLine($"Iyzico received payment: {request.PaymentId}");
        return Ok(new { success = true });
    }
}


using Gateway.Api.Models.Payments;
using Gateway.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentOrchestratorClient client) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await client.GetAllAsync();
        Console.WriteLine("TEST-ONUR");
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await client.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentRequest request)
    {
        var result = await client.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdatePaymentStatusRequest request)
    {
        var ok = await client.UpdateStatusAsync(id, request.Status);
        return ok ? NoContent() : NotFound();
    }
}

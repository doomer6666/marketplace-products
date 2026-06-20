using Marketplace.Products.Application;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Products.Api.Controllers;

[ApiController]
[Route("api/v1/dev-tools")]
public class DevToolsController(IDevToolsService devToolsService, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("generate-fake-products")]
    public async Task<IActionResult> GenerateFakeProducts([FromQuery] int count)
    {
        if (!env.IsDevelopment())
        {
            return Forbid();
        }

        await devToolsService.GenerateFakeProducts(count);

        return Ok(new { message = $"Successfully generated and queued {count} products for sync." });
    }
}
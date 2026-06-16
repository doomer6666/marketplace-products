using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Products.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProductList([FromQuery] ProductFilterDto filter)
    {
        var products = await productService.GetFilteredProductList(filter);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById([FromRoute] Guid id)
    {
        var product = await productService.GetProductById(id);
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(CreateProductDto dto)
    {
        var id = await productService.CreateProduct(dto);
        return CreatedAtAction(nameof(GetProductById), new { id }, new { id });
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Product>> UpdateProductById([FromRoute] Guid id, [FromBody] UpdateProductDto dto)
    {
        var updatedProduct = await productService.UpdateProductById(id, dto);
        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductById(Guid id)
    {
        await productService.DeleteProductById(id);
        return NoContent();
    }
}
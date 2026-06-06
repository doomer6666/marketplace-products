using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Products.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProductList([FromQuery] ProductFilterDto filter)
    {
        var products = await productService.GetFilteredProductList(filter);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id)
    {
        try
        {
            var product = await productService.GetProductById(id);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        var id = await productService.CreateProduct(dto);
        return CreatedAtAction(nameof(GetProductById), new { id }, new { id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateProductById([FromRoute] Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var result = await productService.UpdateProductById(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductById(Guid id)
    {
        await productService.DeleteProductById(id);
        return Ok();
    }
}
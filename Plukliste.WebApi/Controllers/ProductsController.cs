using Microsoft.AspNetCore.Mvc;
using Plukliste.Services;
using Plukliste.Data.Entities;

namespace Plukliste.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IStockService _stockService;

    public ProductsController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _stockService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(string id)
    {
        var product = await _stockService.GetProductAsync(id);
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    [HttpPut("{id}/stock")]
    public async Task<ActionResult> UpdateStock(string id, [FromBody] UpdateStockRequest request)
    {
        var success = await _stockService.UpdateStockAsync(id, request.NewQuantity, request.Notes);
        if (!success)
            return NotFound();
        
        return Ok();
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<List<StockTransaction>>> GetTransactions([FromQuery] string? productId = null, [FromQuery] int limit = 50)
    {
        var transactions = await _stockService.GetTransactionHistoryAsync(productId, limit);
        return Ok(transactions);
    }
}

public record UpdateStockRequest(int NewQuantity, string? Notes);

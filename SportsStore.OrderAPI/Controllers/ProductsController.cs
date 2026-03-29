using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        OrderDbContext context,
        IMapper mapper,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedProductsDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? category = null)
    {
        _logger.LogInformation("GetAll products endpoint called - Page: {Page}, PageSize: {PageSize}, Category: {Category}", page, pageSize, category);

        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var productDtos = _mapper.Map<List<ProductDto>>(products);

        return Ok(new PagedProductsDto
        {
            Products = productDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        _logger.LogInformation("GetById product endpoint called - ProductId: {ProductId}", id);

        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        _logger.LogInformation("GetCategories endpoint called");

        var categories = await _context.Products
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }
}

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Queries;

public record GetOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedResult<OrderDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.Select(o =>
        {
            var dto = _mapper.Map<OrderDto>(o);
            dto.CustomerName = o.Customer?.Name ?? "";
            dto.Email = o.Customer?.Email ?? "";
            return dto;
        }).ToList();

        return new PaginatedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

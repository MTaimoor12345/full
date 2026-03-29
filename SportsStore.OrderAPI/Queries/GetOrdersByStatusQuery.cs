using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Queries;

public record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<List<OrderDto>>;

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrderDto>>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersByStatusQueryHandler(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => o.Status == request.Status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(o =>
        {
            var dto = _mapper.Map<OrderDto>(o);
            dto.CustomerName = o.Customer?.Name ?? "";
            return dto;
        }).ToList();
    }
}

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Queries;

public record GetOrdersByEmailQuery(string Email) : IRequest<List<OrderDto>>;

public class GetOrdersByEmailQueryHandler : IRequestHandler<GetOrdersByEmailQuery, List<OrderDto>>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersByEmailQueryHandler(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByEmailQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => o.Customer != null && o.Customer.Email == request.Email)
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

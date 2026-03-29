using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Queries;

public record GetCustomerOrdersQuery(int CustomerId) : IRequest<List<OrderDto>>;

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, List<OrderDto>>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public GetCustomerOrdersQueryHandler(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId)
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

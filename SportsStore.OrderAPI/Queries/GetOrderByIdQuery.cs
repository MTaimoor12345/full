using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Queries;

public record GetOrderByIdQuery(int OrderId) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
            return null;

        var orderDto = _mapper.Map<OrderDto>(order);
        orderDto.CustomerName = order.Customer?.Name ?? "";
        return orderDto;
    }
}

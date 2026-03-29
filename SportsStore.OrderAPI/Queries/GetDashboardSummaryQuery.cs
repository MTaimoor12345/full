using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly OrderDbContext _context;

    public GetDashboardSummaryQueryHandler(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders.ToListAsync(cancellationToken);

        // Define completed statuses (orders that have been paid)
        var completedStatuses = new[] 
        { 
            OrderStatus.Completed, 
            OrderStatus.PaymentApproved,
            OrderStatus.ShippingPending,
            OrderStatus.ShippingCreated 
        };
        
        // Define pending statuses
        var pendingStatuses = new[]
        {
            OrderStatus.Submitted,
            OrderStatus.InventoryPending,
            OrderStatus.InventoryConfirmed,
            OrderStatus.PaymentPending
        };
        
        // Define failed/cancelled statuses
        var failedStatuses = new[]
        {
            OrderStatus.Failed,
            OrderStatus.InventoryFailed,
            OrderStatus.PaymentFailed
        };

        var summary = new DashboardSummaryDto
        {
            TotalOrders = orders.Count,
            CompletedOrders = orders.Count(o => completedStatuses.Contains(o.Status)),
            FailedOrders = orders.Count(o => failedStatuses.Contains(o.Status)),
            PendingOrders = orders.Count(o => pendingStatuses.Contains(o.Status)),
            TotalRevenue = orders.Where(o => completedStatuses.Contains(o.Status)).Sum(o => o.TotalAmount),
            
            // Additional breakdown for React Admin
            InventoryPendingOrders = orders.Count(o => o.Status == OrderStatus.InventoryPending),
            PaymentPendingOrders = orders.Count(o => o.Status == OrderStatus.PaymentPending),
            ShippedOrders = orders.Count(o => o.Status == OrderStatus.ShippingCreated || o.Status == OrderStatus.ShippingPending),
            CancelledOrders = orders.Count(o => failedStatuses.Contains(o.Status)),
            
            OrdersByStatus = orders.GroupBy(o => o.Status)
                                   .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return summary;
    }
}

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int FailedOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    
    // Additional properties for React Admin
    public int InventoryPendingOrders { get; set; }
    public int PaymentPendingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int CancelledOrders { get; set; }
    
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
}

using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ItemCancelled: SaleId={SaleId}, ItemId={ItemId}, Product={ProductName} ({ProductId})",
            notification.SaleId,
            notification.ItemId,
            notification.ProductName,
            notification.ProductId);

        return Task.CompletedTask;
    }
}

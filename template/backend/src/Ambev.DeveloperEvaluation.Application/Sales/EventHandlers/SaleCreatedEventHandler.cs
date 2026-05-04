using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}, Customer={CustomerName} ({CustomerId}), Total={TotalAmount}",
            notification.SaleId,
            notification.SaleNumber,
            notification.CustomerName,
            notification.CustomerId,
            notification.TotalAmount);

        return Task.CompletedTask;
    }
}

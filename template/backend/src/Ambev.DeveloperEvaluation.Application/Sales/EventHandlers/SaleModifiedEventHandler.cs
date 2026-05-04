using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}, NewTotal={TotalAmount}",
            notification.SaleId,
            notification.SaleNumber,
            notification.TotalAmount);

        return Task.CompletedTask;
    }
}

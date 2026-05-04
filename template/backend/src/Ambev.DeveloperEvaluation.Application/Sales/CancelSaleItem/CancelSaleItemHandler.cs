using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelSaleItemHandler> _logger;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IMediator mediator, ILogger<CancelSaleItemHandler> logger)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Cannot cancel an item in a cancelled sale.");

        var item = sale.Items.FirstOrDefault(i => i.Id == command.ItemId)
            ?? throw new KeyNotFoundException($"Item with ID {command.ItemId} not found in sale {command.SaleId}.");

        sale.CancelItem(command.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _mediator.Publish(new ItemCancelledEvent(sale.Id, item.Id, item.ProductId, item.ProductName), cancellationToken);
        _logger.LogInformation("ItemCancelled event published for item {ItemId} in sale {SaleId}", item.Id, sale.Id);

        return new CancelSaleItemResult { Success = true };
    }
}

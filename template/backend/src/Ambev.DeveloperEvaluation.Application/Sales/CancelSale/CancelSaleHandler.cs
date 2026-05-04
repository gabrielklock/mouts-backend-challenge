using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelSaleHandler> _logger;

    public CancelSaleHandler(ISaleRepository saleRepository, IMediator mediator, ILogger<CancelSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Sale is already cancelled.");

        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _mediator.Publish(new SaleCancelledEvent(sale.Id, sale.SaleNumber), cancellationToken);
        _logger.LogInformation("SaleCancelled event published for sale {SaleId} - {SaleNumber}", sale.Id, sale.SaleNumber);

        return new CancelSaleResult { Success = true };
    }
}

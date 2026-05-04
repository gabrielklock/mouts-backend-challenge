using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator, ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale.");

        sale.SaleNumber = command.SaleNumber;
        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;

        var items = command.Items.Select(i => new SaleItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList();
        sale.UpdateItems(items);

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _mediator.Publish(new SaleModifiedEvent(updated.Id, updated.SaleNumber, updated.TotalAmount), cancellationToken);
        _logger.LogInformation("SaleModified event published for sale {SaleId} - {SaleNumber}", updated.Id, updated.SaleNumber);

        return _mapper.Map<UpdateSaleResult>(updated);
    }
}

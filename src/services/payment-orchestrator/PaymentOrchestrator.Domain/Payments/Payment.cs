using Shared.Kernel.Domain.Entities;
using Shared.Kernel.Domain.Primitives;
using Shared.Kernel.Domain.Results;
using System;

namespace PaymentOrchestrator.Domain.Payments;

public class Payment : AggregateRoot<int>
{
    public string MerchantId { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private Payment() { } // EF Core için

    private Payment(string merchantId, decimal amount, string currency)
    {
        MerchantId = merchantId;
        Amount = amount;
        Currency = currency;
    }

    // Factory method (Create Payment)
    public static Result<Payment> Create(string merchantId, decimal amount, string currency = "TRY")
    {
        Guard.AgainstNull(merchantId, nameof(merchantId));

        if (amount <= 0)
            return Result<Payment>.Failure(
                new("payment.invalid_amount", "Amount must be greater than zero."));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result<Payment>.Failure(
                new("payment.invalid_currency", "Currency must be a 3-letter ISO code."));

        var payment = new Payment(merchantId, amount, currency)
        {
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return Result<Payment>.Success(payment);
    }

    // Domain Logic → Update Status
    public Result UpdateStatus(PaymentStatus newStatus)
    {
        if (Status == PaymentStatus.Succeeded)
            return Result.Failure(
                new("payment.already_succeeded", "A succeeded payment cannot change status."));

        if (Status == PaymentStatus.Cancelled)
            return Result.Failure(
                new("payment.cancelled", "A cancelled payment cannot change status."));

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}

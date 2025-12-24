using Shared.Kernel.Domain.Primitives;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Domain.Payments;

public sealed class Payment 
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string MerchantId { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? ProviderTransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private Payment() { } // EF Core için

    private Payment(string merchantId, decimal amount, string currency)
    {
        MerchantId = merchantId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // -------------------------------------------
    // FACTORY METHOD (Create)
    // -------------------------------------------
    public static Result<Payment> Create(string merchantId, decimal amount, string currency = "TRY")
    {
        Guard.AgainstNull(merchantId, nameof(merchantId));

        if (amount <= 0)
            return Result<Payment>.Failure(new("payment.invalid_amount", "Amount must be > 0."));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result<Payment>.Failure(new("payment.invalid_currency", "Currency must be 3 letters."));

        return Result<Payment>.Success(new Payment(merchantId, amount, currency));
    }

    // -------------------------------------------
    // DOMAIN BEHAVIORS
    // -------------------------------------------

    // Fraud / Provider Fail
    public Result MarkAsFailed(string reason)
    {
        //if (Status == PaymentStatus.Succeeded)
        //    return Result.Failure(new("payment.already_succeeded", "Succeeded payment cannot fail."));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // Provider success
    public Result MarkAsCompleted(string providerTransactionId)
    {
        //if (Status == PaymentStatus.Failed)
        //    return Result.Failure(new("payment.failed_already", "A failed payment cannot be completed."));

        ProviderTransactionId = providerTransactionId;
        Status = PaymentStatus.Succeeded;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // Manual (optional)
    public Result UpdateStatus(PaymentStatus newStatus)
    {
        //if (Status == PaymentStatus.Succeeded)
        //    return Result.Failure(new("payment.succeeded_locked", "A succeeded payment cannot change."));

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}

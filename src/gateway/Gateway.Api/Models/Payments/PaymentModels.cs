namespace Gateway.Api.Models.Payments;

public record CreatePaymentRequest(
    string MerchantId,
    decimal Amount,
    string Currency,
    string Provider);

public record UpdatePaymentStatusRequest(
    string Status);

public record PaymentResponse(
    int Id,
    string MerchantId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);


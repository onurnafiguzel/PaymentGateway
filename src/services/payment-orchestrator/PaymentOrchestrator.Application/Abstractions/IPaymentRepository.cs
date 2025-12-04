using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);    
}

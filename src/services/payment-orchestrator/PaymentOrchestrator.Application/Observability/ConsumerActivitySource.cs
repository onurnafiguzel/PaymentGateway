using System.Diagnostics;

namespace PaymentOrchestrator.Application.Observability;

public static class ConsumerActivitySource
{
    public const string SourceName = "PaymentOrchestrator.Consumers";

    public static readonly ActivitySource Instance =
        new ActivitySource(SourceName);
}

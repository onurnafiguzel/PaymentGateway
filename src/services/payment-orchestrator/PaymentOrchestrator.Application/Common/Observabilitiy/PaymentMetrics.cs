using System.Diagnostics.Metrics;

namespace PaymentOrchestrator.Application.Common.Observabilitiy;

public static class PaymentMetrics
{
    public const string MeterName = "payment.orchestrator";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Histogram<double> PaymentDurationSeconds =
        Meter.CreateHistogram<double>(
            name: "payment_duration_seconds",
            unit: "s",
            description: "Total duration of payment saga in seconds");
}

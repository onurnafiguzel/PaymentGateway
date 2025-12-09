using System.Diagnostics;

namespace Shared.Messaging.Events;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("PaymentOrchestrator");
}


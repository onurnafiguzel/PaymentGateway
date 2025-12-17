using System;
using System.Diagnostics;

namespace Shared.Kernel.Tracing;

public static class TraceIdExtensions
{
    public static Guid ToGuid(this ActivityTraceId traceId)
    {
        Span<byte> bytes = stackalloc byte[16];
        traceId.CopyTo(bytes);
        return new Guid(bytes);
    }
}

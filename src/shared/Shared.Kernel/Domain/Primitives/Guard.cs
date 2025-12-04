namespace Shared.Kernel.Domain.Primitives;

public static class Guard
{
    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name);
    }

    public static void Against(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} cannot be empty.");
    }
}

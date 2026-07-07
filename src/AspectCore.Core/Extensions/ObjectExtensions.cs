#nullable enable
namespace AspectCore.Core.Extensions;

internal static class ObjectExtensions
{
    /// <summary>
    /// Determines whether the null state of two objects is identical.
    /// </summary>
    /// <param name="a">The first object to compare.</param>
    /// <param name="b">The second object to compare.</param>
    /// <returns>Returns true if both parameters are null or both are not null; otherwise, false.</returns>
    public static bool IsSameNullState<T>(this T? a, T? b)
    {
        return (a is null) == (b is null);
    }
}

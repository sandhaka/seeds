using CollectionsExtensions.Models;

namespace CollectionsExtensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Once<T>(this IEnumerable<T> source) =>
        new EnumerableOnce<T>(source);
}
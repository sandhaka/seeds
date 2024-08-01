using CollectionsExtensions.Models;

namespace CollectionsExtensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Once<T>(this IEnumerable<T> source) =>
        new EnumerableOnceSequence<T>(source);
}
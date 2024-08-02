using CollectionsExtensions.Models;

namespace CollectionsExtensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Once<T>(this IEnumerable<T> source) =>
        new EnumerableOnce<T>(source);
    
    public static IReadOnlyCollection<string> Format<T>(this IEnumerable<T> source, int padding = 4, bool addHeader = true) =>
        new Formattable<T>(source).Format(padding, addHeader);
}
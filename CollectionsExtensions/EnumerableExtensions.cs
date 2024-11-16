using CollectionsExtensions.Models;

namespace CollectionsExtensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Ensures that the given sequence can only be enumerated once. Any further attempt to enumerate
    /// the sequence will result in an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to be enumerated only once.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that can only be enumerated once.</returns>
    public static IEnumerable<T> Once<T>(this IEnumerable<T> source) =>
        new EnumerableOnce<T>(source);

    /// <summary>
    /// Converts the given sequence into an instance of <see cref="IDescribable{T}"/>,
    /// allowing the sequence to be described with headers and formatted output.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to be converted into a describable format.</param>
    /// <returns>An instance of <see cref="IDescribable{T}"/> that represents the describable sequence.</returns>
    public static IDescribable<T> ToDescribable<T>(this IEnumerable<T> source) =>
        new Describable<T>(source);
}
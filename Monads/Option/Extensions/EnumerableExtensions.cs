namespace Monads.Option.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Retrieves the first element of a sequence as an Option, or returns Option.None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="sequence">The sequence to retrieve the first element from.</param>
    /// <returns>An Option that contains the first element of the sequence if it exists; otherwise, None.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> sequence) where T : class =>
        sequence.Select(x => Option<T>.Some(x))
            .DefaultIfEmpty(Option<T>.None())
            .First();

    /// <summary>
    /// Retrieves the first element of a sequence as an Option, or returns Option.None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="sequence">The sequence to retrieve the first element from.</param>
    /// <returns>An Option that contains the first element of the sequence if it exists; otherwise, None.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> sequence, Func<T, bool> predicate) where T : class =>
        sequence.Where(predicate).FirstOrNone();
}

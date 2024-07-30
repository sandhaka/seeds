namespace Monads.Optional.Extensions;

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
    /// Retrieves the first value of a sequence as a ValueOption, or returns ValueOption.None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="sequence">The sequence to retrieve the first value from.</param>
    /// <returns>A ValueOption that contains the first value of the sequence if it exists; otherwise, None.</returns>
    public static ValueOption<T> FirstValueOrNone<T>(this IEnumerable<T> sequence) where T : struct =>
        sequence.Select(x => ValueOption<T>.Some(x))
            .DefaultIfEmpty(ValueOption<T>.None())
            .First();

    /// <summary>
    /// Retrieves the first element of a sequence as an Option, or returns Option.None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="sequence">The sequence to retrieve the first element from.</param>
    /// <param name="predicate">Predicate</param>
    /// <returns>An Option that contains the first element of the sequence if it exists; otherwise, None.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> sequence, Func<T, bool> predicate) where T : class =>
        sequence.Where(predicate).FirstOrNone();

    /// <summary>
    /// Retrieves the first non-null value from a sequence as a ValueOption, or returns ValueOption.None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="sequence">The sequence to retrieve the first value from.</param>
    /// <param name="predicate">Predicate</param>
    /// <returns>A ValueOption that contains the first non-null value of the sequence if it exists; otherwise, None.</returns>
    public static ValueOption<T> FirstValueOrNone<T>(this IEnumerable<T> sequence, Func<T, bool> predicate) where T : struct =>
        sequence.Where(predicate).FirstValueOrNone();
}

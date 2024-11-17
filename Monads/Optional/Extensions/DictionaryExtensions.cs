namespace Monads.Optional.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Tries to retrieve the value associated with the specified key from the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search for the key.</param>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>
    /// An Option that contains the retrieved value if the key was found in the dictionary,
    /// or None if the key was not found.
    /// </returns>
    public static Option<TValue> TryGetOption<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class =>
        dictionary.TryGetValue(key, out TValue? value)
            ? Option<TValue>.Some(value)
            : Option<TValue>.None();

    /// <summary>
    /// Tries to retrieve the value associated with the specified key from the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search for the key.</param>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>
    /// A ValueOption that contains the retrieved value if the key was found in the dictionary,
    /// or None if the key was not found.
    /// </returns>
    public static ValueOption<TValue> TryGetOptionValue<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : struct =>
        dictionary.TryGetValue(key, out TValue value)
            ? ValueOption<TValue>.Some(value)
            : ValueOption<TValue>.None();
}

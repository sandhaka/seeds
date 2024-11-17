namespace Monads.Optional;

public struct ValueOption<T> : IEquatable<ValueOption<T>> where T : struct
{
    private T? _content;

    public static ValueOption<T> Some(T obj) => new() { _content = obj };
    public static ValueOption<T> None() => new();

    public Option<TResult> Map<TResult>(Func<T, TResult> map) where TResult : class =>
        _content.HasValue ? Option<TResult>.Some(map(_content.Value)) : Option<TResult>.None();
    public ValueOption<TResult> MapValue<TResult>(Func<T, TResult> map) where TResult : struct =>
        new() { _content = _content.HasValue ? map(_content.Value) : null };
    
    public T Reduce(T orElse) => _content ?? orElse;
    public T Reduce(Func<T> orElse) => _content ?? orElse();

    public ValueOption<T> Where(Func<T, bool> predicate) =>
        _content.HasValue && predicate(_content.Value) ? this : ValueOption<T>.None();
    
    public override bool Equals(object? other) => other is ValueOption<T> option && Equals(option);
    public override int GetHashCode() => _content?.GetHashCode() ?? 0;
    public bool Equals(ValueOption<T> other) =>
        _content.HasValue ? other._content.HasValue && _content.Value.Equals(other._content.Value)
            : !other._content.HasValue;
    public bool Equals(T other) => _content.HasValue && _content.Value.Equals(other);

    public static bool operator ==(ValueOption<T> a, ValueOption<T> b) => a.Equals(b);
    public static bool operator !=(ValueOption<T> a, ValueOption<T> b) => !(a.Equals(b));
    public static bool operator ==(ValueOption<T> a, T b) => a.Equals(b);
    public static bool operator !=(ValueOption<T> a, T b) => !a.Equals(b);
    public static implicit operator ValueOption<T>(T? value) => value is null ? None() : Some(value.Value);
    public override string ToString() => (_content is null ? string.Empty : _content.ToString()) ?? string.Empty;
}
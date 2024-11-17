using System.Diagnostics.CodeAnalysis;

namespace Monads.Optional;

public struct Option<T> : IEquatable<Option<T>> where T : class
{
    private T? _content;
    
    public static Option<T> None() => new();
    public static Option<T> Some(T value) => new() { _content = value };

    public Option<TResult> Map<TResult>(Func<T, TResult> map) where TResult : class => 
        new() { _content = _content is not null ? map(_content) : null };
    public ValueOption<TResult> MapValue<TResult>(Func<T, TResult> map) where TResult : struct =>
        _content is not null ? ValueOption<TResult>.Some(map(_content)) : ValueOption<TResult>.None();
    
    public T Reduce(T orElse) => _content ?? orElse;
    public T Reduce(Func<T> orElse) => _content ?? orElse();
    
    public Option<T> Where(Func<T, bool> predicate) => 
        _content is not null && predicate(_content) ? this : Option<T>.None();
    
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Option<T> other && Equals(other);
    public override int GetHashCode() => _content?.GetHashCode() ?? 0;
    public bool Equals(Option<T> other) => _content is null ? other._content is null : _content.Equals(other._content);
    public bool Equals(T? other) => _content is null ? other is null : _content.Equals(other);
    
    public static bool operator ==(Option<T>? a, Option<T>? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Option<T>? a, Option<T>? b) => !(a == b);
    public static bool operator ==(Option<T>? a, T? b) => a is null ? b is null : a.Value.Equals(b);
    public static bool operator !=(Option<T>? a, T? b) => !(a == b);
    public static implicit operator Option<T>(T? value) => value is null ? None() : Some(value);
    public override string ToString() => (_content is null ? string.Empty : _content.ToString()) ?? string.Empty;
}
namespace Monads.Option;

public abstract class Option<T>
{
    public static implicit operator Option<T>(T value) =>
        new Some<T>(value);

    public static implicit operator Option<T>(None none) =>
        new None<T>();

    public abstract Option<TResult> Map<TResult>(Func<T, TResult> map);
    public abstract Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map);
    public abstract T Reduce();
    public abstract T Reduce(T whenNone);
    public abstract T Reduce(Func<T> whenNone);
    public abstract void IfNotNull(Action<T> act);
    public abstract bool IsNone { get; }
}

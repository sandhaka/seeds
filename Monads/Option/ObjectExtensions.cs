namespace Monads.Option;

public static class ObjectExtensions
{
    public static Option<T> When<T>(this T obj, bool condition) =>
        condition ? new Some<T>(obj) : None.Value;

    public static Option<T> When<T>(this T obj, Func<T, bool> predicate) =>
        obj.When(predicate(obj));

    public static Option<T> NoneIfNull<T>(this T obj) =>
        obj.When(!ReferenceEquals(obj, null));
}

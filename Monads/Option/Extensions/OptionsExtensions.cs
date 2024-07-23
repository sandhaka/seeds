namespace Monads.Option.Extensions;

public static class OptionsExtensions
{
    public static Option<TR> Select<T, TR>(this Option<T> option, Func<T, TR> selector) => option.Map(selector);
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) => 
        option.Bind(content => predicate(content) ? option : new None<T>());
}
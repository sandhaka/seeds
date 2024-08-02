using System.Collections;

namespace CollectionsExtensions.Models;

internal sealed class EnumerableOnce<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _sequence;
    private bool _used;

    public EnumerableOnce(IEnumerable<T> sequence)
    {
        _sequence = sequence;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (_used) throw new InvalidOperationException("Sequence is already used");
        
        _used = true;
        return _sequence.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
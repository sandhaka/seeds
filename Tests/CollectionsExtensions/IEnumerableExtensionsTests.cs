// ReSharper disable PossibleMultipleEnumeration

using CollectionsExtensions;

namespace Tests.CollectionsExtensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ShouldEnumerateOnlyOnce()
    {
        var max = int.MinValue;

        IEnumerable<int> sequence = Enumerable.Range(1, 1024)
            .Once();

        foreach (var i in sequence)
            max = Math.Max(max, i);

        Assert.Throws<InvalidOperationException>(() =>
        {
            max = int.MinValue;
            
            foreach (var i in sequence)
                max = Math.Max(max, i);
        });
    }
}
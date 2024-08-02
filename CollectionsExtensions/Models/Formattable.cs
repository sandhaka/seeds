using System.Buffers;
using System.Collections;
using System.Reflection;

namespace CollectionsExtensions.Models;

internal sealed class Formattable<T>(IEnumerable<T> enumerable) : IEnumerable<T>
{
    private const int LengthLimit = 32;
    private readonly string _nullString = string.Intern("<null>");
    private readonly ArrayPool<string[]> _rowsPool = ArrayPool<string[]>.Shared;
    private readonly ArrayPool<string> _columnsPool = ArrayPool<string>.Shared;

    private readonly IReadOnlyList<PropertyInfo> _propertiesInfos = typeof(T)
        .GetProperties()
        .Where(p => !p.GetAccessors().Any(a => a.IsStatic))
        .ToList()
        .AsReadOnly();

    public IEnumerator<T> GetEnumerator() => enumerable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // TODO: Split
    public IReadOnlyCollection<string> Format(int padding = 4, bool addHeader = true)
    {
        var rowIndex = 0;
        var columnsWidth = Enumerable.Repeat(padding, _propertiesInfos.Count).ToArray();
        var rows = _rowsPool.Rent(enumerable.Count() + 1);
        
        try
        {
            if (addHeader)
            {
                rows[rowIndex] = _propertiesInfos
                    .Select(x => Trim(x.Name.ToUpperInvariant().PadRight(x.Name.Length + padding))!)
                    .ToArray();
            
                rowIndex++;
            }
            
            foreach (var itemRow in enumerable)
            {
                var columnIndex = 0;
                rows[rowIndex] = _columnsPool.Rent(_propertiesInfos.Count);
                foreach (var propertyInfo in _propertiesInfos)
                {
                    var propertyValue = propertyInfo.GetValue(itemRow);
                    var stringValue = Trim(propertyValue?.ToString()) ?? _nullString;
                    columnsWidth[columnIndex] = stringValue.Length + 4 > columnsWidth[columnIndex] ? 
                        stringValue.Length + 4 : 
                        columnsWidth[columnIndex];
                    rows[rowIndex][columnIndex++] = stringValue.PadRight(stringValue.Length + padding);
                }
                
                
                rowIndex++;
            }

            // TODO: reformat with for loops
            
            return rows
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                .Where(columns => columns is not null)
                .Select(columns =>
                    string.Join(
                        '|',
                        columns
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                            .Where(column => column is not null)
                            .Select((c, i) => c.PadRight(columnsWidth[i]))
                    )
                )
                .ToList()
                .AsReadOnly();
        }
        finally
        {
            _rowsPool.Return(rows, clearArray: true);
        }
    }

    private static string? Trim(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length > LengthLimit ? $"{value[..(LengthLimit - 3)]}..." : value;
    }
}
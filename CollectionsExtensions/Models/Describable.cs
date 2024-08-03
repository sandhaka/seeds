using System.Buffers;
using System.Collections;
using System.Reflection;

namespace CollectionsExtensions.Models;

public interface IDescribable<out T> : IEnumerable<T>
{
    IEnumerable<string> Header { get; }
    IReadOnlyCollection<string> Format();
}

internal sealed class Describable<T>(IEnumerable<T> enumerable) : IDescribable<T>
{
    private const int LengthLimit = 20;
    private readonly string _nullRepresentation = string.Intern("<null>");
    
    private readonly ArrayPool<string[]> _rowsPool = ArrayPool<string[]>.Shared;
    private readonly ArrayPool<string> _columnsPool = ArrayPool<string>.Shared;
    
    private IReadOnlyList<PropertyInfo>? _propertyInfos;

    private IReadOnlyList<PropertyInfo> PropertyInfos => 
        _propertyInfos ??= typeof(T).GetProperties()
            .Where(p => !p.GetAccessors().Any(a => a.IsStatic))
            .ToList()
            .AsReadOnly();

    public IEnumerator<T> GetEnumerator() => enumerable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<string> Header => PropertyInfos
        .Select(x => Trim(x.Name.ToUpperInvariant())!);
    
    public IReadOnlyCollection<string> Format()
    {
        var rowsNumber = enumerable.Count() + 1;
        var columnsNumber = PropertyInfos.Count;
        var rows = _rowsPool.Rent(rowsNumber);
        
        try
        {
            rows[0] = Header.Select(AlignValue).ToArray();
            PopulateRows(rows, columnsNumber);
            return rows
                .Take(rowsNumber)
                .Select(columns => string.Join('|', columns.Take(columnsNumber)))
                .ToList()
                .AsReadOnly();
        }
        finally
        {
            foreach (var column in rows[1..rowsNumber].Where(c => (string[]?) c is not null))
                _columnsPool.Return(column, clearArray: true);
            _rowsPool.Return(rows, clearArray: true);
        }
    }
    
    private void PopulateRows(string[][] rows, int columnsNumber)
    {
        var rowIndex = 1;
        foreach (var item in enumerable)
        {
            rows[rowIndex] = _columnsPool.Rent(columnsNumber);
            PopulateColumns(rows[rowIndex], item);
            rowIndex++;
        }
    }

    private void PopulateColumns(string[] columns, T item)
    {
        var columnIndex = 0;
        foreach (var propertyInfo in PropertyInfos)
        {
            string propertyValue = Trim(propertyInfo.GetValue(item)?.ToString()) ?? _nullRepresentation;
            var stringValue = AlignValue(propertyValue);
            columns[columnIndex++] = stringValue;
        }
    }
    
    private static string? Trim(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length > LengthLimit ? $"{value[..(LengthLimit - 3)]}..." : value;
    }

    private static string AlignValue(string value) =>
        string.Format($"{value,LengthLimit}");
}
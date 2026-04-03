using System;

namespace RaptorSheets.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnOrderAttribute : Attribute
{
    public int Order { get; }

    public ColumnOrderAttribute(int order)
    {
        Order = order;
    }

    public ColumnOrderAttribute(string headerName) // convenience: map by header name if needed
    {
        Order = -1;
    }
}

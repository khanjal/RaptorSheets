using System;

namespace RaptorSheets.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class InputAttribute : Attribute
{
    public bool IsInput { get; }

    public InputAttribute(bool isInput = true)
    {
        IsInput = isInput;
    }
}

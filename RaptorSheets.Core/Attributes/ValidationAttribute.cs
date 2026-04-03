using System;

namespace RaptorSheets.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ValidationAttribute : Attribute
{
    public bool EnableValidation { get; }
    public string? Pattern { get; }

    public ValidationAttribute(bool enableValidation = true)
    {
        EnableValidation = enableValidation;
        Pattern = null;
    }

    public ValidationAttribute(string pattern)
    {
        EnableValidation = true;
        Pattern = pattern;
    }

    public ValidationAttribute(bool enableValidation, string pattern)
    {
        EnableValidation = enableValidation;
        Pattern = pattern;
    }
}

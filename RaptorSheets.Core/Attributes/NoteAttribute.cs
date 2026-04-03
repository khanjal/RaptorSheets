using System;

namespace RaptorSheets.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NoteAttribute : Attribute
{
    public string Note { get; }

    public NoteAttribute(string note)
    {
        Note = note ?? throw new ArgumentNullException(nameof(note));
    }
}

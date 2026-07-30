using System.Text;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Turns a live-read <see cref="SheetModel"/> (see <see cref="ISheetOperations.GetLiveSheetStructureAsync"/>)
/// into a best-effort <c>[Column]</c>-decorated C# class stub - a starting point for strongly-typing a
/// hand-built tab, not a finished mapping. Pure/stateless: no DI, no I/O.
/// </summary>
public static class EntityClassGenerator
{
    private const string StringType = "string";
    private const string DecimalType = "decimal?";

    private static readonly HashSet<string> CSharpKeywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
        "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
        "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
        "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
        "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
        "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
        "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
        "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    ];

    private static readonly HashSet<Format> DecimalFormats =
    [
        Format.ACCOUNTING, Format.CURRENCY, Format.NUMBER, Format.DISTANCE, Format.PERCENT
    ];

    private static readonly HashSet<Format> StringFormats =
    [
        Format.DATE, Format.WEEKDAY, Format.DURATION, Format.TIME
    ];

    /// <summary>PascalCase + "Entity" suffix, e.g. "driver notes" -&gt; "DriverNotesEntity".</summary>
    public static string SuggestClassName(string sheetOrTabName)
    {
        var name = ToPascalCase(sheetOrTabName, "Sheet");
        return name.EndsWith("Entity", StringComparison.Ordinal) ? name : $"{name}Entity";
    }

    public static string Generate(string className, SheetModel structure)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            className = SuggestClassName(structure.Name);
        }

        var sb = new StringBuilder();
        sb.AppendLine("using RaptorSheets.Core.Attributes;");
        sb.AppendLine("using RaptorSheets.Core.Entities;");
        sb.AppendLine("using RaptorSheets.Core.Enums;");
        sb.AppendLine();
        sb.AppendLine($"// Generated from a live read of the \"{structure.Name}\" sheet - a best-effort");
        sb.AppendLine("// starting point, not a finished mapping. Review property types, and any");
        sb.AppendLine("// \"Validation detected\" comments, before using this for real.");
        sb.AppendLine($"public class {className} : SheetRowEntityBase");
        sb.AppendLine("{");

        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < structure.Headers.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            AppendProperty(sb, structure.Headers[i], i, usedNames);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void AppendProperty(StringBuilder sb, SheetCellModel header, int index, HashSet<string> usedNames)
    {
        var propertyName = UniquePropertyName(header.Name, index, usedNames);
        var propertyType = ResolvePropertyType(header);
        var isInput = string.IsNullOrEmpty(header.Formula);

        if (!string.IsNullOrEmpty(header.Validation))
        {
            sb.AppendLine($"    // Validation detected: {header.Validation}");
        }

        sb.AppendLine($"    [Column({BuildColumnArgs(header, isInput)})]");

        var defaultValue = propertyType == StringType ? " = \"\";" : "";
        sb.AppendLine($"    public {propertyType} {propertyName} {{ get; set; }}{defaultValue}");
    }

    private static string BuildColumnArgs(SheetCellModel header, bool isInput)
    {
        var args = new List<string> { EscapeStringLiteral(header.Name), $"isInput: {(isInput ? "true" : "false")}" };

        if (header.Format is not null and not Format.DEFAULT)
        {
            args.Add($"formatType: Format.{header.Format}");
        }
        else if (!string.IsNullOrEmpty(header.FormatPattern))
        {
            // No recognized Format enum match, but a live pattern was detected - worth preserving.
            args.Add($"formatPattern: {EscapeStringLiteral(header.FormatPattern)}");
        }

        if (!string.IsNullOrEmpty(header.Note))
        {
            args.Add($"note: {EscapeStringLiteral(header.Note)}");
        }

        return string.Join(", ", args);
    }

    private static string ResolvePropertyType(SheetCellModel header)
    {
        if (header.Validation == "BOOLEAN")
        {
            return "bool";
        }

        if (header.Format is { } format and not Format.DEFAULT)
        {
            if (DecimalFormats.Contains(format))
            {
                return DecimalType;
            }

            if (StringFormats.Contains(format))
            {
                // FieldType.DateTime/Time/Duration all read back as formatted strings (see
                // GenericSheetMapper.GetValueFromSheet) - every shipped domain entity types these
                // columns "string", not DateTime, for exactly that reason.
                return StringType;
            }
        }

        return header.RawFormatType switch
        {
            "DATE" => StringType,
            "NUMBER" or "CURRENCY" or "PERCENT" => DecimalType,
            _ => StringType
        };
    }

    private static string UniquePropertyName(string headerName, int index, HashSet<string> usedNames)
    {
        var baseName = ToPascalCase(headerName, $"Column{index}");
        if (CSharpKeywords.Contains(baseName))
        {
            baseName = $"@{baseName}";
        }

        var candidate = baseName;
        var suffix = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = $"{baseName}{suffix++}";
        }

        return candidate;
    }

    private static string ToPascalCase(string value, string fallback)
    {
        var words = value
            .Split([' ', '_', '-', '/', '.', '(', ')', '&', '\'', '"'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => new string(word.Where(char.IsLetterOrDigit).ToArray()))
            .Where(word => word.Length > 0)
            .ToList();

        if (words.Count == 0)
        {
            return fallback;
        }

        var pascal = string.Concat(words.Select(w => char.ToUpperInvariant(w[0]) + w[1..]));

        return char.IsDigit(pascal[0]) ? $"{fallback}_{pascal}" : pascal;
    }

    // Notes in particular are often real multi-paragraph documentation (blank-line-separated) - a
    // literal newline inside a C# regular string literal is a compile error (CS1010), so those must
    // become escaped \n, not pass through verbatim.
    private static string EscapeStringLiteral(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n")
            .Replace("\r", "\\n")
            .Replace("\t", "\\t");

        return $"\"{escaped}\"";
    }
}

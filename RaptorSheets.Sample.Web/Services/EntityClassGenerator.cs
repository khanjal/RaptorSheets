using System.Globalization;
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

    /// <summary>Shared by every generated class - written once per file. Public so a bulk multi-class
    /// file (see <see cref="Generate"/>'s includeUsings param) can prepend it exactly once instead of
    /// once per class.</summary>
    public static readonly string UsingsHeader = string.Join(Environment.NewLine,
        "using RaptorSheets.Core.Attributes;",
        "using RaptorSheets.Core.Entities;",
        "using RaptorSheets.Core.Enums;",
        "", "");

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

    /// <summary>PascalCase a sheet/tab name for use as a property name on a generated Sheets container
    /// (see <see cref="GenerateContainer"/>) - same casing as <see cref="SuggestClassName"/> but
    /// without the forced "Entity" suffix: a property named "Setup" holding
    /// <c>List&lt;SetupEntity&gt;</c> reads better than "SetupEntity" holding the same list.</summary>
    public static string SuggestPropertyName(string tabName) => ToPascalCase(tabName, "Sheet");

    /// <summary>PascalCase only, no "Sheets"/"Entity" suffix - the shared base a bulk multi-class
    /// file's container (<c>{base}Sheets</c>) and top-level entity (<c>{base}Entity</c>) both derive
    /// from in <see cref="GenerateContainer"/>. Deliberately a derived default (from the connection's
    /// own label) rather than something the user types up front - "semi generic," meant to be renamed
    /// by hand afterward.</summary>
    public static string SuggestContainerBaseName(string connectionLabel) => ToPascalCase(connectionLabel, "Generated");

    /// <summary>Aggregates already-generated row-entity classes into the shape
    /// <c>SheetManagerBase&lt;TEntity&gt;</c> actually needs to do anything with them - a
    /// <c>{baseName}Sheets</c> container (one <c>List&lt;TRowEntity&gt;</c> property per entry) plus a
    /// bare <c>{baseName}Entity : SheetEntityBase&lt;{baseName}Sheets&gt;</c> wrapper, mirroring e.g.
    /// <c>RaptorSheets.Gig.Entities.GigSheets</c>/<c>SheetEntity</c> exactly. <paramref name="baseName"/>
    /// is a derived default (see <see cref="SuggestContainerBaseName"/>), not the real intended domain
    /// name - the header comment says so explicitly since there's no other prompt telling the user to
    /// rename it.</summary>
    public static string GenerateContainer(string baseName, IReadOnlyList<(string PropertyName, string ClassName)> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// \"{baseName}\" is a derived placeholder (from the connection's label), not a");
        sb.AppendLine($"// real domain name - rename {baseName}Sheets/{baseName}Entity to fit before using them.");
        sb.AppendLine($"public class {baseName}Sheets");
        sb.AppendLine("{");

        for (var i = 0; i < entries.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine($"    public List<{entries[i].ClassName}> {entries[i].PropertyName} {{ get; set; }} = [];");
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"public class {baseName}Entity : SheetEntityBase<{baseName}Sheets>");
        sb.AppendLine("{");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <param name="sampleRows">Optional raw values for this same sheet (row 0 assumed to be the
    /// header row, matching structure.Headers' own row-0 assumption) - used only as a fallback when a
    /// column has no Google Sheets format/validation metadata to go on (common on a hand-built sheet
    /// nobody ever explicitly formatted), to tell a genuinely numeric column apart from the default
    /// "string" guess. Ignored entirely for any column format metadata already resolves.</param>
    /// <param name="includeUsings">False for a bulk multi-class file, where every class's own copy of
    /// the same 3 <c>using</c> lines would land after earlier classes once concatenated - illegal in
    /// C# (a using directive must precede every type declaration in the file). The caller is
    /// responsible for writing <see cref="UsingsHeader"/> once at the top of that file instead.</param>
    public static string Generate(string className, SheetModel structure, List<List<string?>>? sampleRows = null, bool includeUsings = true)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            className = SuggestClassName(structure.Name);
        }

        var sb = new StringBuilder();

        if (includeUsings)
        {
            sb.Append(UsingsHeader);
        }

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

            AppendProperty(sb, structure.Headers[i], i, usedNames, GetColumnSamples(sampleRows, i));
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void AppendProperty(StringBuilder sb, SheetCellModel header, int index, HashSet<string> usedNames, IReadOnlyList<string?>? columnSamples)
    {
        var propertyName = UniquePropertyName(header.Name, index, usedNames);
        var propertyType = ResolvePropertyType(header, columnSamples);
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

    private static string ResolvePropertyType(SheetCellModel header, IReadOnlyList<string?>? columnSamples)
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

        // RawFormatType is only null/empty when the live cell had no NumberFormat at all - Google
        // omits that object entirely for "Automatic" formatting nobody ever touched (see
        // SheetStructureHelper: "header.RawFormatType = numberFormat.Type" only runs when
        // numberFormat != null). Any other value - including "TEXT" - means a format was deliberately,
        // explicitly set, e.g. these Boardgames "Java Version"/"Bedrock Version" columns hold values
        // like "1.10" and "1.9" and are explicitly Plain Text, almost certainly on purpose (so Sheets
        // never silently collapses "1.10" to the numeric 1.1) - sampling must not override that.
        if (!string.IsNullOrEmpty(header.RawFormatType))
        {
            return header.RawFormatType switch
            {
                "NUMBER" or "CURRENCY" or "PERCENT" => DecimalType,
                _ => StringType
            };
        }

        // No format metadata at all to go on - true for a hand-built sheet nobody ever explicitly
        // formatted (plain cells with numbers just typed into them), which would otherwise always fall
        // through to "string". Sampling real values is the fallback for exactly that case only.
        return InferTypeFromSamples(columnSamples) ?? StringType;
    }

    /// <summary>Every non-blank sample must agree on a type for it to count - one stray "n/a" or "TBD"
    /// in an otherwise-numeric column should fall back to string rather than produce a class that
    /// won't parse that row at all.</summary>
    private static string? InferTypeFromSamples(IReadOnlyList<string?>? columnSamples)
    {
        if (columnSamples == null)
        {
            return null;
        }

        var nonBlank = columnSamples.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        if (nonBlank.Count == 0)
        {
            return null;
        }

        if (nonBlank.All(v => bool.TryParse(v, out _)))
        {
            return "bool";
        }

        if (nonBlank.All(v => decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out _)))
        {
            return DecimalType;
        }

        return null;
    }

    /// <summary>sampleRows[0] is assumed to be the header row (same assumption structure.Headers
    /// itself makes), so samples start from row 1. columnIndex lines up 1:1 with structure.Headers'
    /// own index - both are built by iterating spreadsheet columns left to right (see
    /// SheetStructureHelper's Column = SheetHelpers.GetColumnName(index)).</summary>
    private static List<string?>? GetColumnSamples(List<List<string?>>? sampleRows, int columnIndex)
    {
        if (sampleRows == null || sampleRows.Count <= 1)
        {
            return null;
        }

        return sampleRows.Skip(1).Select(row => columnIndex < row.Count ? row[columnIndex] : null).ToList();
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

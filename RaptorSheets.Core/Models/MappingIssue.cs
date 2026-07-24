using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Models;

/// <summary>
/// Diagnostic record of a cell that could not be carried onto its target property during row
/// mapping. This is purely informational and never affects the read: the row's entity is always
/// returned, with the property left at its type default (0, null, etc.) for the cell in question.
/// <para>
/// Reads never fail because of what a user typed into a cell - the sheet stays freely editable, and
/// column typing is an expectation, not a constraint the API enforces. A <see cref="MappingIssue"/>
/// exists so a caller can find out *why* a value came back as a default, not to gate whether the
/// value comes back at all.
/// </para>
/// </summary>
public class MappingIssue
{
    /// <summary>Sheet the row came from, if known to the caller that produced the issue.</summary>
    public required string SheetName { get; init; }

    /// <summary>The row's position as returned by the sheet (matches the entity's RowId, if it has one).</summary>
    public required int RowId { get; init; }

    /// <summary>Header name of the affected column.</summary>
    public required string Header { get; init; }

    /// <summary>Name of the entity property the column maps to.</summary>
    public required string PropertyName { get; init; }

    /// <summary>Why the value could not be carried onto the property.</summary>
    public required MappingIssueReason Reason { get; init; }

    /// <summary>
    /// The raw cell text that could not be parsed. Null unless the caller explicitly opted in via
    /// <c>includeRawCellValues</c> - sheet contents are frequently personal or financial data
    /// (earnings, application details, home finances) and must not flow into logs or messages by
    /// default.
    /// </summary>
    public string? RawValue { get; init; }
}

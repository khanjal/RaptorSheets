using RaptorSheets.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Main data container entity for holding all sheet data from a Google Sheets workbook.
/// This is a data transfer object that aggregates data from all sheets.
///
/// Row collections live under <see cref="SheetEntityBase{TSheets}.Sheets"/> (a
/// <see cref="GigSheets"/>) rather than flat on this object, so a domain sheet can never collide
/// with the reserved <c>Properties</c>/<c>Messages</c> members - see <see cref="GigSheets"/>.
/// Sheet order follows SheetsConfig.SheetNames.
/// </summary>
[ExcludeFromCodeCoverage]
public class SheetEntity : SheetEntityBase<GigSheets>
{
}

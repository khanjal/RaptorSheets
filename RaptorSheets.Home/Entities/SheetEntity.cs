using RaptorSheets.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

/// <summary>
/// Main data container entity for holding all sheet data from a Home Google Sheets workbook.
/// Row collections live under <see cref="SheetEntityBase{TSheets}.Sheets"/> (a <see cref="HomeSheets"/>).
/// </summary>
[ExcludeFromCodeCoverage]
public class SheetEntity : SheetEntityBase<HomeSheets>
{
}

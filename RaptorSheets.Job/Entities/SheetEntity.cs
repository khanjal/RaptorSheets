using RaptorSheets.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Main data container entity for holding all sheet data from a Job Google Sheets workbook.
/// Row collections live under <see cref="SheetEntityBase{TSheets}.Sheets"/> (a <see cref="JobSheets"/>).
/// </summary>
[ExcludeFromCodeCoverage]
public class SheetEntity : SheetEntityBase<JobSheets>
{
}

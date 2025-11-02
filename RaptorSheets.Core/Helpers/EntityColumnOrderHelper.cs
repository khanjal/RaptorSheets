using System.Reflection;
using System.Linq;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// [OBSOLETE] Use EntitySheetConfigHelper with ColumnAttribute instead.
/// ColumnAttribute provides comprehensive column configuration including ordering via the Order property.
/// This helper is kept for backward compatibility but will be removed in a future version.
/// </summary>
[Obsolete("Use EntitySheetConfigHelper with ColumnAttribute instead. ColumnAttribute provides comprehensive column configuration including the Order property for explicit ordering.", error: true)]
public static class EntityColumnOrderHelper
{
    // All methods marked obsolete - users should migrate to ColumnAttribute
}
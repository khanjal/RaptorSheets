using System;
using System.Collections.Generic;
using System.Linq;

namespace RaptorSheets.Gig.Helpers
{
    /// <summary>
    /// Small helper utilities used when creating sheets.
    /// Extracted for testability: ordering logic is deterministic and easy to unit test.
    /// </summary>
    public static class CreateSheetsHelpers
    {
        public static List<string> OrderSheetTitlesByIndex(Dictionary<string, int>? sheetsWithIndices)
        {
            if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
                return new List<string>();

            // Place negative indices (no preference) last; otherwise sort by index then by title for determinism
            return sheetsWithIndices
                .OrderBy(kv => kv.Value < 0 ? int.MaxValue : kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => kv.Key)
                .ToList();
        }
    }
}

using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Helpers
{
    /// <summary>
    /// Coordinator that implements a lazy-retry: attempt a batch-get, and if it fails,
    /// attempt to create missing sheets and retry once.
    /// </summary>
    public static class SheetFetchCoordinator
    {
        /// <summary>
        /// Try to get batch data for the requested sheets. If the initial attempt returns null,
        /// call <see cref="SheetInitializationHelper.EnsureMissingSheetsCreatedAsync"/> and retry once.
        /// Returns the response (may be null).
        /// </summary>
        public static async Task<BatchGetValuesByDataFilterResponse?> TryGetBatchDataWithCreateOnFailure(
            IGoogleSheetService sheetService,
            List<string> sheets,
            string? range = null,
            Func<IGoogleSheetService, List<string>, Dictionary<string,int>, Task<(List<string> Found, bool Created)>>? createMissingSheetsFunc = null)
        {
            if (sheetService == null) throw new ArgumentNullException(nameof(sheetService));
            if (sheets == null || sheets.Count == 0) return null;

            // First attempt: try to fetch directly
            var response = await sheetService.GetBatchData(sheets, range);
            if (response != null)
            {
                return response;
            }

            try
            {
                // Determine which sheets are missing. By default the core helper will NOT create
                // domain-specific sheets — callers in domain packages should create sheets
                // using their own configuration. If a domain-provided creator is supplied,
                // invoke it and retry once.
                var missingMap = await SheetInitializationHelper.GetMissingSheetsAsync(sheetService, sheets);

                if (missingMap == null || missingMap.Count == 0)
                {
                    // Nothing missing or unable to determine; nothing to create here.
                    return null;
                }

                var missing = missingMap.Keys.ToList();

                if (createMissingSheetsFunc != null)
                {
                    try
                    {
                        var (found, created) = await createMissingSheetsFunc(sheetService, missing, missingMap);
                        if (!created)
                        {
                            return null;
                        }

                        var retry = await sheetService.GetBatchData(sheets, range);
                        return retry;
                    }
                    catch (Exception exInner)
                    {
                        Console.WriteLine($"Error during domain create callback: {exInner.Message}");
                        return null;
                    }
                }

                // No domain creator supplied; do not create sheets from core.
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during lazy-retry create-on-failure: {ex.Message}");
                return null;
            }
        }
    }
}

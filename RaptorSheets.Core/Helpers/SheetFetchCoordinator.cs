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
            string? range = null)
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
                // Attempt to create missing sheets (helper is idempotent and safe)
                var (found, created) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(sheetService, sheets);

                // Retry only if the creation attempt returned a non-null response (created==true)
                if (!created)
                {
                    return null;
                }

                // Retry once
                var retry = await sheetService.GetBatchData(sheets, range);
                return retry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during lazy-retry create-on-failure: {ex.Message}");
                return null;
            }
        }
    }
}

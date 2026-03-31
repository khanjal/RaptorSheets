# RaptorSheets.Job Integration Test Setup

## Quick Start Guide

### 1. Create Test Spreadsheet

1. Go to [Google Sheets](https://sheets.google.com)
2. Create a new blank spreadsheet
3. Name it "RaptorSheets Job Test"
4. Copy the spreadsheet ID from the URL:
   ```
   https://docs.google.com/spreadsheets/d/SPREADSHEET_ID_HERE/edit
   ```

### 2. Share with Service Account

1. Click the "Share" button in your test spreadsheet
2. Add your service account email (found in your credentials JSON):
   ```
   your-service-account@your-project.iam.gserviceaccount.com
   ```
3. Grant "Editor" permissions
4. Click "Done"

### 3. Configure User Secrets

From the repository root:

```bash
cd RaptorSheets.Test

# Google Credentials (from your service account JSON file)
dotnet user-secrets set "google_credentials:type" "service_account"
dotnet user-secrets set "google_credentials:private_key_id" "YOUR_PRIVATE_KEY_ID"
dotnet user-secrets set "google_credentials:private_key" "YOUR_PRIVATE_KEY"
dotnet user-secrets set "google_credentials:client_email" "YOUR_CLIENT_EMAIL"
dotnet user-secrets set "google_credentials:client_id" "YOUR_CLIENT_ID"

# Test Spreadsheet ID
dotnet user-secrets set "spreadsheets:job" "YOUR_SPREADSHEET_ID"
```

**Alternative**: Set as one JSON string (easier for copy-paste):
```bash
dotnet user-secrets set GoogleCredential '{
  "type": "service_account",
  "private_key_id": "YOUR_PRIVATE_KEY_ID",
  "private_key": "YOUR_PRIVATE_KEY",
  "client_email": "YOUR_CLIENT_EMAIL",
  "client_id": "YOUR_CLIENT_ID"
}'

dotnet user-secrets set "spreadsheets:job" "YOUR_SPREADSHEET_ID"
```

### 4. Verify Setup

```bash
# Check secrets are configured
cd RaptorSheets.Test
dotnet user-secrets list

# Expected output:
# google_credentials:type = service_account
# google_credentials:private_key_id = abc123...
# google_credentials:private_key = -----BEGIN PRIVATE KEY-----...
# google_credentials:client_email = your-service@project.iam.gserviceaccount.com
# google_credentials:client_id = 123456789...
# spreadsheets:job = 1AbC123...XyZ
```

### 5. Run Tests

```bash
# From repository root
dotnet test RaptorSheets.Job.Tests --filter "Category=Integration"
```

### 6. View Results

Open your test spreadsheet in Google Sheets. You should see:

- **9 sheets created**:
  - Applications
  - Interviews
  - Companies
  - Positions
  - Sites
  - Decisions
  - Interview Types
  - Interview Outcomes
  - Schedules

- **Each sheet has**:
  - Proper headers in row 1
  - Correct formatting (colors, frozen rows)
  - Data validation on input columns
  - Formulas in calculated columns

## Test Categories

### Integration Tests

**What they test**:
- ? Sheet creation via Google Sheets API
- ? Header validation
- ? Sheet structure and layout
- ? Tab order and naming
- ? Demo data generation

**How to run**:
```bash
# All integration tests
dotnet test RaptorSheets.Job.Tests --filter "Category=Integration"

# Specific test
dotnet test RaptorSheets.Job.Tests --filter "FullyQualifiedName~Environment_ShouldHaveAllRequiredSheets"
```

### Unit Tests

**What they test**:
- ? Configuration constants
- ? Sheet utilities
- ? Entity structure
- ? No external dependencies

**How to run**:
```bash
# All unit tests (fast, no credentials needed)
dotnet test RaptorSheets.Job.Tests --filter "Category!=Integration"
```

## Troubleshooting

### Tests Skip with "User secrets not configured"

**Problem**: `FactCheckUserSecretsAttribute` is skipping tests

**Solution**:
```bash
# Verify secrets exist
cd RaptorSheets.Test
dotnet user-secrets list

# If empty, reconfigure secrets (see step 3 above)
```

### "Permission denied" or "404 Not Found"

**Problem**: Service account doesn't have access to spreadsheet

**Solutions**:
1. Verify spreadsheet ID is correct
2. Share spreadsheet with service account email
3. Grant "Editor" permissions (not just "Viewer")
4. Check service account email matches credentials

### "Authentication failed"

**Problem**: Invalid or expired credentials

**Solutions**:
1. Verify all credential fields are set correctly
2. Check for extra quotes or whitespace in private key
3. Ensure service account is enabled in Google Cloud Console
4. Generate new service account key if needed

### Tests Take Too Long

**Expected Behavior**: Integration tests make real API calls

**Typical Duration**:
- First run (with setup): 15-30 seconds
- Subsequent runs: 10-20 seconds

**If much slower**:
- Check internet connection
- Verify Google Sheets API quota isn't exceeded
- Consider running fewer tests at once

## Test Collection Behavior

The `IntegrationTestFixture` runs **once** before all integration tests:

1. **Deletes** all existing sheets in test spreadsheet
2. **Waits** 3 seconds for deletion to propagate
3. **Creates** all fresh sheets
4. **Waits** 2 seconds for creation to complete
5. **Runs** all integration tests

This ensures:
- ? Clean slate for every test run
- ? No leftover data from previous runs
- ? Consistent test results
- ? Proper sheet structure validation

## What You'll See in the Spreadsheet

After tests run successfully:

### Applications Sheet
```
Date       | Company      | Job Title          | Key                    | Interviews | Decision | Days Active | Pay - Low | Pay - High | Pay - Avg
2024-01-15 | TechCorp     | Software Engineer  | TechCorp-Software-0   | 0          | Pending  | 10         | 100000    | 150000     | 125000
```

### Interviews Sheet
```
Date       | Start Time | End Time | Company   | Job Title         | Key                    | Type          | Round | Outcome
2024-01-20 | 2:00 PM   | 3:00 PM  | TechCorp  | Software Engineer | TechCorp-Software-0   | Phone Screen  | 1     | Passed
```

### Companies Sheet (Auto-calculated)
```
Company      | Applications | Interviews
TechCorp     | 1           | 1
DataSystems  | 1           | 0
```

### Reference Sheets
Sites, Decisions, Interview Types, etc. with dropdown values ready for validation.

## Next Steps

Once integration tests are passing:

1. **Explore the spreadsheet** - See how formulas work
2. **Add more test data** - Extend demo data generation
3. **Test CRUD operations** - Add tests for create/update/delete
4. **Validate formulas** - Test cross-sheet references
5. **Test data validation** - Verify dropdown constraints

## Resources

- [Google Sheets API Documentation](https://developers.google.com/sheets/api)
- [Service Account Setup](https://cloud.google.com/iam/docs/creating-managing-service-accounts)
- [User Secrets in .NET](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)

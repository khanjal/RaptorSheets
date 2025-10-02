# Authentication Guide

## Overview

RaptorSheets libraries support multiple authentication methods for accessing Google Sheets API. This guide covers setup and configuration for all authentication types.

## Table of Contents
1. [Service Account (Recommended)](#service-account-recommended)
2. [OAuth2 Access Token](#oauth2-access-token)
3. [Google Cloud Setup](#google-cloud-setup)
4. [Testing Authentication](#testing-authentication)
5. [Troubleshooting](#troubleshooting)

## Service Account (Recommended)

Service accounts are ideal for server-side applications and automated processes. They don't require user interaction for authentication.

### 1. Create Service Account

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select or create a project
3. Navigate to **APIs & Services** > **Credentials**
4. Click **Create Credentials** > **Service Account**
5. Fill in service account details:
   - **Name**: `RaptorSheets Service Account`
   - **Description**: `Service account for RaptorSheets library`
6. Click **Create and Continue**
7. Skip role assignment (optional step)
8. Click **Done**

### 2. Generate Credentials

1. Click on your created service account
2. Go to the **Keys** tab
3. Click **Add Key** > **Create New Key**
4. Select **JSON** format
5. Click **Create** - the credentials file will download

### 3. Use Service Account Credentials

#### Method 1: Dictionary (Recommended)
```csharp
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["private_key_id"] = "your-private-key-id",
    ["private_key"] = "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
    ["client_email"] = "raptorsheets@your-project.iam.gserviceaccount.com",
    ["client_id"] = "your-client-id"
};

var manager = new GoogleSheetManager(credentials, spreadsheetId);
```

#### Method 2: JSON File
```csharp
// Load from JSON file
var json = await File.ReadAllTextAsync("path/to/service-account.json");
var credentialDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

var manager = new GoogleSheetManager(credentialDict, spreadsheetId);
```

#### Method 3: Environment Variables
```csharp
var credentials = new Dictionary<string, string>
{
    ["type"] = Environment.GetEnvironmentVariable("GOOGLE_TYPE"),
    ["private_key_id"] = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY_ID"),
    ["private_key"] = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY"),
    ["client_email"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_EMAIL"),
    ["client_id"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
};
```

### 4. Share Spreadsheet

**Important**: Share your Google Spreadsheet with the service account email:

1. Open your Google Spreadsheet
2. Click **Share** button
3. Add the service account email (found in the JSON file as `client_email`)
4. Set permissions to **Editor**
5. Uncheck **Notify people** (service accounts don't receive emails)
6. Click **Share**

## OAuth2 Access Token

Use OAuth2 for applications that act on behalf of users. Requires user consent flow.

### 1. Create OAuth2 Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Navigate to **APIs & Services** > **Credentials**
3. Click **Create Credentials** > **OAuth 2.0 Client IDs**
4. Select application type:
   - **Web application**: For web apps
   - **Desktop application**: For desktop/console apps
   - **Mobile**: For mobile applications
5. Configure authorized redirect URIs (for web applications)
6. Click **Create**

### 2. Obtain Access Token

#### Using Google's OAuth2 Library
```csharp
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

// Load client secrets
var clientSecrets = GoogleClientSecrets.FromFile("client_secret.json").Secrets;

// Create authorization flow
var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
{
    ClientSecrets = clientSecrets,
    Scopes = new[] { "https://www.googleapis.com/auth/spreadsheets" },
    DataStore = new FileDataStore("TokenStore")
});

// Authorize user
var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    clientSecrets,
    new[] { "https://www.googleapis.com/auth/spreadsheets" },
    "user",
    CancellationToken.None,
    new FileDataStore("TokenStore"));

// Get access token
var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

// Use with RaptorSheets
var manager = new GoogleSheetManager(accessToken, spreadsheetId);
```

#### Manual OAuth2 Flow
```csharp
// Redirect user to authorization URL
var authUrl = $"https://accounts.google.com/oauth/authorize" +
              $"?client_id={clientId}" +
              $"&redirect_uri={redirectUri}" +
              $"&scope=https://www.googleapis.com/auth/spreadsheets" +
              $"&response_type=code" +
              $"&access_type=offline";

// After user authorizes, exchange code for token
var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", 
    new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
        new KeyValuePair<string, string>("code", authorizationCode),
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("redirect_uri", redirectUri)
    }));

var tokenData = await tokenResponse.Content.ReadAsStringAsync();
var token = JsonSerializer.Deserialize<TokenResponse>(tokenData);

// Use access token
var manager = new GoogleSheetManager(token.AccessToken, spreadsheetId);
```

## Google Cloud Setup

### 1. Enable Required APIs

Enable these APIs in your Google Cloud project:

1. **Google Sheets API** (Required)
   - Go to [APIs & Services > Library](https://console.cloud.google.com/apis/library)
   - Search for "Google Sheets API"
   - Click and enable it

2. **Google Drive API** (Optional - for file operations)
   - Search for "Google Drive API" 
   - Click and enable it

### 2. Configure API Quotas

Monitor and configure quotas:

1. Go to **APIs & Services** > **Quotas**
2. Filter by "Sheets API"
3. Review current usage and limits:
   - **Requests per 100 seconds per user**: 100
   - **Requests per day**: 50,000

### 3. Set Up Billing (If Needed)

For production applications exceeding free quotas:

1. Go to **Billing** in Google Cloud Console
2. Link a billing account to your project
3. Consider requesting quota increases for high-volume applications

## Testing Authentication

### Test Service Account
```csharp
public async Task<bool> TestServiceAccountAuth(Dictionary<string, string> credentials, string spreadsheetId)
{
    try
    {
        var manager = new GoogleSheetManager(credentials, spreadsheetId);
        var properties = await manager.GetSheetProperties();
        
        Console.WriteLine($"? Authentication successful! Found {properties.Count} sheets.");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Authentication failed: {ex.Message}");
        return false;
    }
}
```

### Test OAuth2 Token
```csharp
public async Task<bool> TestOAuth2Auth(string accessToken, string spreadsheetId)
{
    try
    {
        var manager = new GoogleSheetManager(accessToken, spreadsheetId);
        var data = await manager.GetSheets();
        
        Console.WriteLine($"? Authentication successful! Retrieved data with {data.Messages.Count} messages.");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Authentication failed: {ex.Message}");
        return false;
    }
}
```

### Validate Credentials Format
```csharp
public bool ValidateServiceAccountCredentials(Dictionary<string, string> credentials)
{
    var requiredFields = new[] { "type", "private_key_id", "private_key", "client_email", "client_id" };
    
    foreach (var field in requiredFields)
    {
        if (!credentials.ContainsKey(field) || string.IsNullOrEmpty(credentials[field]))
        {
            Console.WriteLine($"? Missing or empty field: {field}");
            return false;
        }
    }
    
    if (credentials["type"] != "service_account")
    {
        Console.WriteLine($"? Invalid credential type: {credentials["type"]}. Expected: service_account");
        return false;
    }
    
    Console.WriteLine("? Service account credentials format is valid");
    return true;
}
```

## Configuration Examples

### ASP.NET Core Configuration
```csharp
// appsettings.json
{
  "GoogleCredentials": {
    "type": "service_account",
    "private_key_id": "your-key-id",
    "private_key": "your-private-key",
    "client_email": "service@project.iam.gserviceaccount.com",
    "client_id": "your-client-id"
  },
  "SpreadsheetId": "your-spreadsheet-id"
}

// Startup.cs or Program.cs
services.Configure<GoogleCredentials>(configuration.GetSection("GoogleCredentials"));
services.AddScoped<IGoogleSheetManager>(provider =>
{
    var credentials = provider.GetService<IOptions<GoogleCredentials>>().Value;
    var spreadsheetId = configuration["SpreadsheetId"];
    
    var credentialDict = new Dictionary<string, string>
    {
        ["type"] = credentials.Type,
        ["private_key_id"] = credentials.PrivateKeyId,
        ["private_key"] = credentials.PrivateKey,
        ["client_email"] = credentials.ClientEmail,
        ["client_id"] = credentials.ClientId
    };
    
    return new GoogleSheetManager(credentialDict, spreadsheetId);
});
```

### Console Application with User Secrets
```bash
# Initialize user secrets
dotnet user-secrets init

# Set credentials
dotnet user-secrets set "GoogleCredentials:type" "service_account"
dotnet user-secrets set "GoogleCredentials:private_key_id" "your-key-id"
dotnet user-secrets set "GoogleCredentials:private_key" "your-private-key"
dotnet user-secrets set "GoogleCredentials:client_email" "service@project.iam.gserviceaccount.com"
dotnet user-secrets set "GoogleCredentials:client_id" "your-client-id"
dotnet user-secrets set "SpreadsheetId" "your-spreadsheet-id"
```

```csharp
// Program.cs
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var credentials = new Dictionary<string, string>
{
    ["type"] = configuration["GoogleCredentials:type"],
    ["private_key_id"] = configuration["GoogleCredentials:private_key_id"],
    ["private_key"] = configuration["GoogleCredentials:private_key"],
    ["client_email"] = configuration["GoogleCredentials:client_email"],
    ["client_id"] = configuration["GoogleCredentials:client_id"]
};

var spreadsheetId = configuration["SpreadsheetId"];
var manager = new GoogleSheetManager(credentials, spreadsheetId);
```

## Troubleshooting

### Common Authentication Issues

#### 1. "The caller does not have permission"
**Solution**: Share the spreadsheet with the service account email
```
Error: The caller does not have permission
Fix: Add service account email to spreadsheet with Editor permissions
```

#### 2. "Invalid JWT Signature"
**Solutions**:
- Verify private key format (should include BEGIN/END markers)
- Ensure no extra whitespace in credentials
- Check that private_key_id matches the actual key

#### 3. "API not enabled"
**Solution**: Enable Google Sheets API
```
Error: Google Sheets API has not been used in project...
Fix: Go to Google Cloud Console > APIs & Services > Library > Enable Google Sheets API
```

#### 4. "Invalid scope"
**Solution**: Ensure correct OAuth2 scopes
```csharp
// Required scope for RaptorSheets
var scopes = new[] { "https://www.googleapis.com/auth/spreadsheets" };

// If using Drive API features, add:
var scopes = new[] { 
    "https://www.googleapis.com/auth/spreadsheets",
    "https://www.googleapis.com/auth/drive" 
};
```

#### 5. "Token has expired"
**Solution**: Refresh OAuth2 tokens
```csharp
// Implement token refresh logic
if (credential.Token.IsStale)
{
    await credential.RefreshTokenAsync(CancellationToken.None);
}
```

### Debug Authentication
```csharp
public async Task DebugAuthentication(Dictionary<string, string> credentials, string spreadsheetId)
{
    Console.WriteLine("=== Authentication Debug ===");
    
    // 1. Validate credential format
    ValidateServiceAccountCredentials(credentials);
    
    // 2. Test basic connection
    try
    {
        var manager = new GoogleSheetManager(credentials, spreadsheetId);
        Console.WriteLine("? Manager created successfully");
        
        // 3. Test API access
        var properties = await manager.GetSheetProperties();
        Console.WriteLine($"? API access successful: {properties.Count} sheets found");
        
        // 4. List sheet names
        foreach (var prop in properties)
        {
            Console.WriteLine($"   - {prop.Name}");
        }
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error: {ex.Message}");
        
        if (ex.Message.Contains("permission"))
        {
            Console.WriteLine("?? Hint: Share spreadsheet with service account email");
        }
        else if (ex.Message.Contains("API"))
        {
            Console.WriteLine("?? Hint: Enable Google Sheets API in Google Cloud Console");
        }
    }
}
```

### Environment-Specific Tips

#### Development
- Use user secrets for local development
- Create separate test spreadsheets
- Enable detailed logging/debugging

#### Production
- Store credentials securely (Azure Key Vault, AWS Secrets Manager, etc.)
- Monitor API usage and quotas
- Implement proper error handling and retries
- Use separate service accounts for different environments

#### CI/CD
- Store credentials as encrypted environment variables
- Use separate test spreadsheets for automated testing
- Implement authentication validation in pipeline

## Security Best Practices

### 1. Credential Storage
- **Never commit credentials to source control**
- Use secure storage (Key Vault, Secrets Manager)
- Rotate service account keys regularly
- Use different credentials for different environments

### 2. Access Control
- Grant minimal necessary spreadsheet permissions
- Use separate service accounts for different applications
- Regularly audit service account usage
- Monitor API access logs

### 3. Application Security
- Validate all inputs before API calls
- Implement rate limiting in your application
- Use HTTPS for all communications
- Log authentication failures for monitoring

## Getting Help

If you're still having authentication issues:

1. **Check Error Messages**: Most authentication errors provide specific guidance
2. **Review Google Cloud Console**: Check API enablement and quotas
3. **Verify Permissions**: Ensure spreadsheet is shared correctly
4. **Test Credentials**: Use the debug methods provided above
5. **Community Support**: Ask questions in [GitHub Discussions](https://github.com/khanjal/RaptorSheets/discussions)

For more specific implementation guidance, see the package-specific documentation:
- [Core Library](CORE.md) - For custom implementations
- [Gig Package](../RaptorSheets.Gig/README.md) - For gig work tracking
- [Stock Package](STOCK.md) - For portfolio management
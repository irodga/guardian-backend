using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

public class VaultIamAuthService
{
    private readonly HttpClient _http = new HttpClient();

    public async Task<string?> LoginAsync()
    {
        var vaultAddress = "https://api.secrets-guardian.online";
        var role = "backend";
        var region = "us-east-1";
        var endpoint = "https://sts.amazonaws.com";
        var service = "sts";
        var requestParams = "Action=GetCallerIdentity&Version=2011-06-15";

        var creds = FallbackCredentialsFactory.GetCredentials().GetCredentials();
        var accessKey = creds.AccessKey;
        var secretKey = creds.SecretKey;
        var token = creds.Token;

        var now = DateTime.UtcNow;
        var date = now.ToString("yyyyMMdd");
        var timestamp = now.ToString("yyyyMMddTHHmmssZ");

        var canonicalHeaders = $"content-type:application/x-www-form-urlencoded\nhost:sts.amazonaws.com\nx-amz-date:{timestamp}\nx-amz-security-token:{token}\n";
        var signedHeaders = "content-type;host;x-amz-date;x-amz-security-token";
        var payloadHash = HashSHA256(requestParams);

        var canonicalRequest = $"POST\n/\n\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
        var credentialScope = $"{date}/{region}/{service}/aws4_request";
        var stringToSign = $"AWS4-HMAC-SHA256\n{timestamp}\n{credentialScope}\n{HashSHA256(canonicalRequest)}";

        var signature = Sign(stringToSign, secretKey, date, region, service);

        var authHeader = $"AWS4-HMAC-SHA256 Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

        var iamHeaders = new Dictionary<string, string>
        {
            ["content-type"] = "application/x-www-form-urlencoded",
            ["host"] = "sts.amazonaws.com",
            ["x-amz-date"] = timestamp,
            ["x-amz-security-token"] = token,
            ["authorization"] = authHeader
        };

        var loginPayload = new
        {
            role,
            iam_http_request_method = "POST",
            iam_request_url = Convert.ToBase64String(Encoding.UTF8.GetBytes(endpoint)),
            iam_request_body = Convert.ToBase64String(Encoding.UTF8.GetBytes(requestParams)),
            iam_request_headers = iamHeaders
        };

        var json = JsonSerializer.Serialize(loginPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var res = await _http.PostAsync($"{vaultAddress}/v1/auth/aws/login", content);
        var response = await res.Content.ReadAsStringAsync();

        Console.WriteLine("Vault response:");
        Console.WriteLine(response);

        if (res.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(response);
            return doc.RootElement.GetProperty("auth").GetProperty("client_token").GetString();
        }

        return null;
    }

    private static string HashSHA256(string data)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLower();
    }

    private static byte[] HmacSHA256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string Sign(string stringToSign, string secretKey, string date, string region, string service)
    {
        var kSecret = Encoding.UTF8.GetBytes($"AWS4{secretKey}");
        var kDate = HmacSHA256(kSecret, date);
        var kRegion = HmacSHA256(kDate, region);
        var kService = HmacSHA256(kRegion, service);
        var kSigning = HmacSHA256(kService, "aws4_request");
        var signature = HmacSHA256(kSigning, stringToSign);
        return Convert.ToHexString(signature).ToLower();
    }
}

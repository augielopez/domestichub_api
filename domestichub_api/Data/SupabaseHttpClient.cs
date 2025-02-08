using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace domestichub_api.Data;

public class SupabaseHttpClient
{
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly HttpClient _httpClient;

    public SupabaseHttpClient(IConfiguration configuration)
    {
        // Read Supabase values from appsettings.json
        _supabaseUrl = configuration["Supabase:Url"];
        _supabaseKey = configuration["Supabase:Key"];

        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseKey))
        {
            throw new InvalidOperationException("Supabase URL or API Key is missing from configuration.");
        }

        // Initialize the HttpClient
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
    }

    public async Task<string> GetAsync(string tableName, string query = "")
    {
        var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/{tableName}?{query}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> PostAsync(string tableName, string jsonData)
    {
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/{tableName}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> DeleteAsync(string tableName, string query)
    {
        var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/{tableName}?{query}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

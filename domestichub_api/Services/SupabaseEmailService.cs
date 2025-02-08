using domestichub_api.Data;
using domestichub_api.Models;
using Newtonsoft.Json;

namespace domestichub_api.Services;

public class SupabaseEmailService
{
    
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly SupabaseHttpClient _supabaseClient;

    public SupabaseEmailService(IConfiguration configuration, IServiceProvider serviceProvider, SupabaseHttpClient supabaseClient)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _supabaseClient = supabaseClient;
    }
     
    public async Task<List<Email>?> GetEmailsAsync()
    {
        var response = await _supabaseClient.GetAsync("tb_emails");
        return JsonConvert.DeserializeObject<List<Email>>(response);
    }
        
    public async Task<Email?> GetEmailAsync(string uid)
    {
        var response = await _supabaseClient.GetAsync("tb_emails", $"pk=eq.{uid}");
        return JsonConvert.DeserializeObject<List<Email>>(response).FirstOrDefault();
    }
        
    public async Task DeleteEmailAsync(string uid)
    {
        await _supabaseClient.DeleteAsync("tb_emails", $"pk=eq.{uid}");
    }
        
    public async Task DeleteEmailsAsync(IEnumerable<string> uids)
    {
        var query = string.Join(",", uids.Select(uid => $"pk=eq.{uid}"));
        await _supabaseClient.DeleteAsync("tb_emails", query);
    }
        
    public async Task SaveEmailAsync(Email email)
    {
        var jsonData = JsonConvert.SerializeObject(email);
        await _supabaseClient.PostAsync("tb_emails", jsonData);
    }
        
    public async Task SaveEmailsAsync(IEnumerable<Email> emails)
    {
        var jsonData = JsonConvert.SerializeObject(emails);
        await _supabaseClient.PostAsync("tb_emails", jsonData);
    }
}
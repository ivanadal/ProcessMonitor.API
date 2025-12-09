using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {

        using var httpClient = new HttpClient();

        string apiUrl = "https://localhost:7023/processmonitor/analyze";

        var requestBody = new
        {
            action = "Closed ticket #48219 and sent confirmation email",
            guideline = "All closed tickets must include a confirmation email"
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine("API Response:");
            Console.WriteLine(responseJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error calling API: " + ex.Message);
        }
    }
}

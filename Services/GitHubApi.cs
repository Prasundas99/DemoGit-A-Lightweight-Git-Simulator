using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DemoGit.Services;

public class GitHubApi
{
    private readonly HttpClient _client;

    public GitHubApi(string token)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://api.github.com")
        };
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DemoGit", "1.0"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
    }

    public async Task<string> CreateRepository(string name, string description = null)
    {
        var payload = new
        {
            name,
            description,
            @private = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/user/repos", content);
        if(!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error creating repository: {response.StatusCode}, {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.GetProperty("clone_url").GetString();
    }

    public async Task PushToGitHub(string repoName, string localPath)
    {
        // Ensure the repository exists
        var remoteUrl = await CreateRepository(repoName);

        // Ensure the repository is initialized
        if(!Directory.Exists(Path.Combine(localPath, ".git")))
        {
            Console.WriteLine("Initializing git repository...");
            Directory.CreateDirectory(Path.Combine(localPath, ".git"));
        }

        Console.WriteLine("Preparing to push...");
        // Simulate pushing changes (tree, commits, refs)
        await PushContents(remoteUrl);
    }

    private async Task PushContents(string remoteUrl)
    {
        Console.WriteLine($"Pushing contents to {remoteUrl}...");
        // Simulate creating a commit
        var commitPayload = new
        {
            message = "Initial commit",
            tree = "BASE_TREE_SHA",
            parents = new string[] { },
            author = new
            {
                name = "DemoGit User",
                email = "demogit@example.com",
                date = DateTime.UtcNow.ToString("o")
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(commitPayload), System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/repos/owner/repo/git/commits", content);

        if(!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error pushing contents: {response.StatusCode}, {error}");
        }

        Console.WriteLine("Push successful!");
    }
}

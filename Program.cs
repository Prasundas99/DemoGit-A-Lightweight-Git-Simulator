using DemoGit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DemoGit;

internal class Program(DemoGit demoGit)
{
    private readonly DemoGit _demogit = demoGit;

    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<Program>();
                    })
                    .Build();

        var program = host.Services.GetRequiredService<Program>();
        await program.RunAsync();
    }


    public async Task RunAsync()
    {
        try
        {
            while(true)
            {
                var path = Directory.GetCurrentDirectory();
                SystemCommandHandler.PrintPrompt(path);

                var input = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Error: Input cannot be empty or whitespace.");
                    continue;
                }

                var (command, arguments) = SystemCommandHandler.ParseInput(input);

                if(command == "demogit")
                {
                    try
                    {
                        await _demogit.HandleDemoGitCommandAsync(arguments);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error in DemoGit command: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        SystemCommandHandler.RunSystemCommand(command, arguments);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error running system command '{command}': {ex.Message}");
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}. The application will terminate.");
        }
    }
   }

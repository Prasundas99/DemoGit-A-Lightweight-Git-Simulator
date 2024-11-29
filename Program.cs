using DemoGit.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DemoGit;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            while(true)
            {
                var path = Directory.GetCurrentDirectory();
                PrintPrompt(path);

                var input = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Error: Input cannot be empty or whitespace.");
                    continue;
                }

                var (command, arguments) = ParseInput(input);

                if(command == "demogit")
                {
                    try
                    {
                        HandleDemoGitCommand(arguments);
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
                        RunSystemCommand(command, arguments);
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

    static (string command, string arguments) ParseInput(string input)
    {
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLower();
        var arguments = parts.Length > 1 ? parts[1] : "";
        return (command, arguments);
    }

    static void HandleDemoGitCommand(string arguments)
    {
        if(string.IsNullOrWhiteSpace(arguments))
        {
            throw new ArgumentException("Error: No arguments provided for the 'demogit' command.");
        }

        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLower();
        var hash = parts.Length > 1 ? parts[1] : "";

        switch(command)
        {
            case "init":
                DemoGitCommands.GitInitialization();
                break;

            case "remove":
                DemoGitCommands.GitRemove();
                break;

            case "cat-file":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit cat-file <type|size|content> <hash>");
                }

                var subcommandParts = hash.Split(' ', 2);
                if(subcommandParts.Length < 2)
                {
                    throw new ArgumentException("Usage: demogit cat-file <type|size|content> <hash>");
                }

                var subcommand = subcommandParts[0].ToLower();
                var objectHash = subcommandParts[1];
                DemoGitCommands.GitCatFile(subcommand, objectHash);
                break;

            case "hash-object":
                DemoGitCommands.HandleHashObjectCommand(parts);
                break;

            case "ls-tree":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit ls-tree <tree-hash>");
                }

                DemoGitCommands.GitLsTree(hash);
                break;

            case "write-tree":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit write-tree -w <tree-hash>");
                }

                DemoGitCommands.GitWriteTree(hash);
                break;

            case "add":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit add <file-name|.>");
                }
                DemoGitCommands.AddToIndex(hash);
                break;

            case "status":
                Console.WriteLine("Status of your demoGit repository:");
                break;

            default:
                DemoGitCommands.DisplayDemoGitHelp();
                break;
        }
    }

    static void RunSystemCommand(string command, string arguments)
    {
        if(string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Error: System command cannot be empty.");
        }

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = IsWindows() ? "cmd.exe" : "/bin/bash",
                Arguments = IsWindows() ? $"/c {command} {arguments}" : $"-c \"{command} {arguments}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if(process == null)
            {
                throw new InvalidOperationException("Error: Failed to start the system process.");
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if(!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            if(!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine($"Error: {error}");
            }
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Failed to run system command: {ex.Message}", ex);
        }
    }

    static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    static void PrintPrompt(string path)
    {
        try
        {
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var os = IsWindows() ? "Windows" : "Linux";
            Console.WriteLine($"[{dateTime}] {os} : {path} > ");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error displaying prompt: {ex.Message}");
        }
    }
}

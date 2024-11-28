using DemoGit.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DemoGit;

internal class Program
{
    static void Main(string[] args)
    {
        while(true)
        {
            var path = Directory.GetCurrentDirectory();
            PrintPrompt(path);

            var input = Console.ReadLine();
            if(string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Input is not valid");
                continue;
            }

            var (command, arguments) = ParseInput(input);

            if(command == "demogit")
            {
                HandleDemoGitCommand(arguments);
            }
            else
            {
                RunSystemCommand(command, arguments);
            }
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
                    Console.WriteLine("Usage: demogit cat-file <type|size|content> <hash>");
                    break;
                }

                // We expect two arguments for cat-file: <type|size|content> <hash>
                var subcommandParts = hash.Split(' ', 2);
                if(subcommandParts.Length < 2)
                {
                    Console.WriteLine("Usage: demogit cat-file <type|size|content> <hash>");
                    break;
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
                    Console.WriteLine("Usage: demogit ls-tree <tree-hash>");
                    break;
                }

                DemoGitCommands.GitLsTree(hash);
                break;

            case "write-tree":
                if(string.IsNullOrEmpty(hash))
                {
                    Console.WriteLine("Usage: demogit write-tree -w <tree-hash>");
                    break;
                }

                DemoGitCommands.GitWriteTree(hash);
                break;

            case "status":
                Console.WriteLine("Status of your demoGit");
                break;

            default:
                DemoGitCommands.DisplayDemoGitHelp();
                break;
        }
    }


    static void RunSystemCommand(string command, string arguments)
    {
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
            var output = process?.StandardOutput.ReadToEnd();
            var error = process?.StandardError.ReadToEnd();
            process?.WaitForExit();

            if(!string.IsNullOrWhiteSpace(output)) Console.WriteLine(output);
            if(!string.IsNullOrWhiteSpace(error)) Console.WriteLine($"Error: {error}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Failed to run command '{command}': {ex.Message}");
        }
    }

    static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    static void PrintPrompt(string path)
    {
        var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var os = IsWindows() ? "Windows" : "Linux";
        Console.WriteLine($"[{dateTime}] {os} : {path} > ");
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DemoGit.Services;
public class SystemCommandHandler
{
    public static (string command, string arguments) ParseInput(string input)
    {
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLower();
        var arguments = parts.Length > 1 ? parts[1] : "";
        return (command, arguments);
    }

    public static void RunSystemCommand(string command, string arguments)
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

    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static void PrintPrompt(string path)
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

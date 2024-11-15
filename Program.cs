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
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Detect the OS
            var os = IsWindows() ? "Windows" : "Linux";
            Console.WriteLine($"[{dateTime}] {os} : {path} > ");

            var input = Console.ReadLine();

            Console.WriteLine("You entered: " + input);

            if(input == string.Empty || input == null)
            {
                Console.WriteLine("given input is not valid");
            }

            // Split the input into command and arguments
            var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();
            var arguments = parts.Length > 1 ? parts[1] : "";

            // Custom handling for 'demoGit'
            if(command == "demogit")
            {
                HandleDemoGitCommand(arguments);
            }
            else
            {
                // For other commands, attempt to run them
                RunSystemCommand(command, arguments);
            }
        }
    }

    static void HandleDemoGitCommand(string arguments)
    {
        // Example of handling different demoGit subcommands
        switch(arguments.ToLower())
        {
            case "init":
                GitInitialization();
                break;
            case "remove":
                GitRemove();
                break;
            case "status":
                Console.WriteLine("Status of your demoGit");
                break;
            // Add more demoGit subcommands as needed
            default:
                HelperDemoGitText(arguments);
                break;
        }
    }

    private static void HelperDemoGitText(string arguments)
    {
        if(arguments == null || arguments.Length == 0 || arguments == string.Empty)
        {
            Console.WriteLine("These are the existing demogit commands \n" +
                " 'demogit init' :-  This creates a .git folder / initialize git \n" +
                "'demogit remove' :- This removes existing .git folder and clears all git history and everything \n"
                );
        }
        else
        {
            Console.WriteLine(arguments + "is an unknown command; enter demogit for help");
        }
    }

    private static void GitInitialization()
    {
        Console.WriteLine("Initalizing your demoGit");
        Directory.CreateDirectory(".git");
        Directory.CreateDirectory(".git/objects");
        Directory.CreateDirectory(".git/refs");
        File.WriteAllText(".git/HEAD", "ref: refs/heads/main\n");
        Console.WriteLine("DemoGit initialized");
    }

    private static void GitRemove()
    {
        Console.WriteLine("Removing your demoGit..");
        File.Delete(".git/HEAD");
        Directory.Delete(".git/objects");
        Directory.Delete(".git/refs");
        Directory.Delete(".git");
        Console.WriteLine("DemoGit Removed..");
    }

    static void RunSystemCommand(string command, string arguments)
    {
        try
        {
            ProcessStartInfo processInfo;

            if(IsWindows())
            {
                // Windows requires commands to run in 'cmd.exe' if they are not standalone executables
                processInfo = new ProcessStartInfo("cmd.exe", $"/c {command} {arguments}");
            }
            else
            {
                // On Unix-like systems, commands should run in '/bin/bash'
                processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command} {arguments}\"");
            }

            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            // Capture and display output
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
                Console.WriteLine(output);
            if (!string.IsNullOrWhiteSpace(error))
                Console.WriteLine($"Error: {error}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Failed to run command '{command}': {ex.Message}");
        }
    }

    static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}

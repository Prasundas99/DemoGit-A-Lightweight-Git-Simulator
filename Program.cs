using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DemoGit;

internal class Program
{
    static void Main(string[] args)
    {

       

        while(true)
        {
            // Get the current path
            var path = Directory.GetCurrentDirectory();

            // Get the current date and time
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Detect the OS
            var os = IsWindows() ? "Windows" : "Linux";

            // Display the information in Git Bash style
            Console.WriteLine($"[{dateTime}] {os} : {path} > ");
            // Read the input from the user
            var input = Console.ReadLine();

            // Print the input back to the console
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
                Console.WriteLine($"Executing demoGit command: {arguments}");
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
                Console.WriteLine("Init your demoGit");
                break;
            case "status":
                Console.WriteLine("Status of your demoGit");
                break;
            // Add more demoGit subcommands as needed
            default:
                Console.WriteLine($"Unknown demoGit command: {arguments}");
                break;
        }
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

            using(var process = new Process { StartInfo = processInfo })
            {
                process.Start();

                // Capture and display output
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if(!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine(output);
                if(!string.IsNullOrWhiteSpace(error))
                    Console.WriteLine($"Error: {error}");
            }
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

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

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
                HandleDemoGitCommand(arguments, parts);
            }
            else
            {
                // For other commands, attempt to run them
                RunSystemCommand(command, arguments);
            }
        }
    }

    static void HandleDemoGitCommand(string arguments, string[] parts)
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
            case "cat-file":
                GitCatFile(parts);
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

    private static void GitCatFile(string[] parts)
    {
        if(parts.Length < 3)
        {
            Console.Error.WriteLine("Usage: demogit cat-file <type|size|content> <hash>");
            return;
        }

        var command = parts[1]; // Command type: "type", "size", or "content"
        var hash = parts[2];   // Git object hash

        if(string.IsNullOrEmpty(hash) || hash.Length < 3)
        {
            Console.Error.WriteLine("Error: Invalid hash.");
            return;
        }

        // Determine the object file path
        var folderName = hash.Substring(0, 2);
        var fileName = hash.Substring(2);
        var path = Path.Combine(".git", "objects", folderName, fileName);

        if(!File.Exists(path))
        {
            Console.Error.WriteLine($"Error: Object {hash} not found.");
            return;
        }

        // Read and decompress the object file
        var compressedData = File.ReadAllBytes(path);
        byte[] decompressedData;

        using(var compressedStream = new MemoryStream(compressedData))
        using(var decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress))
        using(var resultStream = new MemoryStream())
        {
            decompressionStream.CopyTo(resultStream);
            decompressedData = resultStream.ToArray();
        }

        // Parse the decompressed data
        var decompressedContent = Encoding.UTF8.GetString(decompressedData);
        var nullIndex = decompressedContent.IndexOf('\0');
        if(nullIndex == -1)
        {
            Console.Error.WriteLine("Error: Invalid Git object format.");
            return;
        }

        var header = decompressedContent.Substring(0, nullIndex); // e.g., "blob 13"
        var content = decompressedContent[(nullIndex + 1)..]; // Actual file content

        var headerParts = header.Split(' ');
        if(headerParts.Length != 2)
        {
            Console.Error.WriteLine("Error: Invalid Git object header.");
            return;
        }

        var type = headerParts[0]; // e.g., "blob"
        var size = headerParts[1]; // e.g., "13"

        // Process the command
        switch(command)
        {
            case "type":
                Console.WriteLine(type);
                break;

            case "size":
                Console.WriteLine(size);
                break;

            case "content":
                Console.WriteLine(content);
                break;

            default:
                Console.Error.WriteLine("Error: Unsupported command.");
                break;
        }
    }

    private static void HelperDemoGitText(string arguments)
    {
        if(arguments == null || arguments.Length == 0 || arguments == string.Empty)
        {
            Console.WriteLine("These are the existing demogit commands \n" +
                " 'demogit init' :-  This creates a .git folder / initialize git \n" +
                "'demogit remove' :- This removes existing .git folder and clears all git history and everything \n" +
                " 'demogit cat-file <type|size|content> <hash>' : It mimics how Git interacts with its object database to retrieve and display information about objects stored in the .git directory"
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

            if(!string.IsNullOrWhiteSpace(output))
                Console.WriteLine(output);
            if(!string.IsNullOrWhiteSpace(error))
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

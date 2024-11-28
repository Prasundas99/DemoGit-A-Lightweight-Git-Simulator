using System.IO.Compression;
using System.Linq;
using System.Text;

namespace DemoGit.Services;

public static class DemoGitHelper
{
    public static void DisplayDemoGitHelp()
    {
        Console.WriteLine("These are the existing demoGit commands: \n" +
            "'demogit init'      : Initializes demoGit (creates .git folder)\n" +
            "'demogit remove'    : Removes demoGit and clears all git history\n" +
            "'demogit cat-file'  : Mimics Git's object database (use with <type|size|content> <hash>)\n" +
            "'demogit status'    : Displays status of demoGit");
    }

    public static void GitInitialization()
    {
        Console.WriteLine("Initializing demoGit...");
        Directory.CreateDirectory(".git");
        Directory.CreateDirectory(".git/objects");
        Directory.CreateDirectory(".git/refs");
        File.WriteAllText(".git/HEAD", "ref: refs/heads/main\n");
        Console.WriteLine("demoGit initialized.");
    }

    public static void GitRemove()
    {
        Console.WriteLine("Removing demoGit...");
        if(Directory.Exists(".git"))
        {
            Directory.Delete(".git", true);
            Console.WriteLine("demoGit removed.");
        }
        else
        {
            Console.WriteLine("Error: .git directory does not exist.");
        }
    }

    public static void GitCatFile(string command, string hash)
    {
        if(string.IsNullOrEmpty(hash) || hash.Length < 3)
        {
            Console.Error.WriteLine("Error: Invalid hash.");
            return;
        }

        var path = Path.Combine(".git", "objects", hash.Substring(0, 2), hash.Substring(2));
        if(!File.Exists(path))
        {
            Console.Error.WriteLine($"Error: Object {hash} not found.");
            return;
        }

        try
        {
            var decompressedData = DecompressObjectFile(path);
            var (type, size, content) = ParseGitObject(decompressedData);

            switch(command.ToLower())
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
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    static (string type, string size, string content) ParseGitObject(byte[] data)
    {
        var decompressedContent = Encoding.UTF8.GetString(data);
        var nullIndex = decompressedContent.IndexOf('\0');
        if(nullIndex == -1) throw new InvalidDataException("Invalid Git object format.");

        var header = decompressedContent.Substring(0, nullIndex);
        var content = decompressedContent.Substring(nullIndex + 1);

        var headerParts = header.Split(' ');
        if(headerParts.Length != 2) throw new InvalidDataException("Invalid Git object header.");

        return (headerParts[0], headerParts[1], content);
    }

    static byte[] DecompressObjectFile(string path)
    {
        var compressedData = File.ReadAllBytes(path);
        using var compressedStream = new MemoryStream(compressedData);
        using var decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        decompressionStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
    public static void HandleHashObjectCommand(string[] parts)
    {
        var command = string.Join(" ", parts);
        var commands = command.Split(' ');
        if(commands.Length < 3 || commands[1] != "-w")
        {
            Console.WriteLine("Usage: demogit hash-object -w <file>");
            return;
        }

        var filePath = commands[2];
        if(!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: The file '{filePath}' does not exist.");
            return;
        }

        HashObject(filePath);
    }

    private static void HashObject(string filePath)
    {
        // Read the file content
        var fileContent = File.ReadAllBytes(filePath);

        // Create the object header
        var header = $"blob {fileContent.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);

        // Combine the header and file content
        var objectData = new byte[headerBytes.Length + fileContent.Length];
        Array.Copy(headerBytes, 0, objectData, 0, headerBytes.Length);
        Array.Copy(fileContent, 0, objectData, headerBytes.Length, fileContent.Length);

        // Compute the SHA-1 hash of the object data
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hashBytes = sha1.ComputeHash(objectData);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // Store the object in the .git/objects directory
        StoreGitObject(hash, objectData);

        Console.WriteLine($"Hash: {hash}");
    }

    private static void StoreGitObject(string hash, byte[] objectData)
    {
        // The first two characters of the hash are used to create the folder
        var folderName = hash.Substring(0, 2);
        var fileName = hash.Substring(2);

        // Create the directories if they don't exist
        var objectDirectory = Path.Combine(".git", "objects", folderName);
        if(!Directory.Exists(objectDirectory))
        {
            Directory.CreateDirectory(objectDirectory);
        }

        // Compress and save the object to a file
        var objectFilePath = Path.Combine(objectDirectory, fileName);
        using var fileStream = new FileStream(objectFilePath, FileMode.Create, FileAccess.Write);
        using var zlibStream = new ZLibStream(fileStream, CompressionMode.Compress);
        zlibStream.Write(objectData, 0, objectData.Length);
    }

}

using System.IO.Compression;
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

}


namespace DemoGit.Services;

public static class DemoGitCommands
{
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
        var path = DemoGitHelper.GetPathFromGitObjects(hash);

        try
        {
            var decompressedData = DemoGitHelper.DecompressObjectFile(path);
            var (type, size, content) = DemoGitHelper.ParseGitObject(decompressedData);

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
            throw new InvalidDataException($"Error: The file '{filePath}' does not exist.");
        }

        var hashObject = DemoGitHelper.CreateBlobObject(filePath);

        Console.WriteLine("Blob:" + hashObject);
    }

    public static void GitLsTree(string hash)
    {
        var folderName = hash.Substring(0, 2);
        var fileName = hash.Substring(2);
        var path = Path.Combine(".git", "objects", folderName, fileName);
        if(!File.Exists(path))
        {
            Console.Error.WriteLine($"Error: Tree object {hash} not found.");
            return;
        }

        var decompressedData = DemoGitHelper.DecompressObjectFile(path);
        var (type, c, d) = DemoGitHelper.ParseGitObject(decompressedData);

        if(type != "tree")
        {
            throw new InvalidDataException($"Expected 'tree' object but found '{type}'.");
        }

        var entries = DemoGitHelper.ParseTreeObject(decompressedData);

        foreach(var entry in entries)
        {
            Console.WriteLine($"{entry.Mode} {entry.Hash}\t{entry.Name}");
        }
    }

    public static void GitWriteTree(string hash)
    {
        var path = DemoGitHelper.GetPathFromGitObjects(hash);

        var treeHash = DemoGitHelper.CreateTreeObject(path);
        Console.WriteLine($"Tree written successfully: {treeHash}");
    }


    public static void DisplayDemoGitHelp()
    {
        Console.WriteLine("These are the existing demoGit commands: \n" +
            "'demogit init'      : Initializes demoGit (creates .git folder)\n" +
            "'demogit remove'    : Removes demoGit and clears all git history\n" +
            "'demogit cat-file'  : Mimics Git's object database (use with <type|size|content> <hash>)\n" +
            "'demogit status'    : Displays status of demoGit");
    }
}

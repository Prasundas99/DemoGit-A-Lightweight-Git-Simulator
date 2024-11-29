namespace DemoGit.Services;

public static class DemoGitCommands
{
    public static void GitInitialization()
    {
        try
        {
            Console.WriteLine("Initializing demoGit...");
            Directory.CreateDirectory(".git");
            Directory.CreateDirectory(".git/objects");
            Directory.CreateDirectory(".git/refs");
            File.Create("index").Dispose();
            File.WriteAllText(".git/HEAD", "ref: refs/heads/main\n");
            Console.WriteLine("demoGit initialized.");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error during initialization: {ex.Message}", ex);
        }
    }

    public static void GitRemove()
    {
        try
        {
            if(!Directory.Exists(".git"))
            {
                throw new InvalidOperationException(".git directory does not exist.");
            }

            Directory.Delete(".git", true);
            Console.WriteLine("demoGit removed.");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error during removal: {ex.Message}", ex);
        }
    }

    public static void GitCatFile(string command, string hash)
    {
        if(string.IsNullOrEmpty(hash) || hash.Length < 3)
        {
            throw new ArgumentException("Invalid hash.");
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
                    throw new ArgumentException("Unsupported command.");
            }
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in cat-file: {ex.Message}", ex);
        }
    }

    public static void HandleHashObjectCommand(string[] parts)
    {
        try
        {
            var command = string.Join(" ", parts);
            var commands = command.Split(' ');

            if(commands.Length < 3 || commands[1] != "-w")
            {
                throw new ArgumentException("Usage: demogit hash-object -w <file>");
            }

            var filePath = commands[2];
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' does not exist.");
            }

            var hashObject = DemoGitHelper.CreateBlobObject(filePath);
            Console.WriteLine("Blob: " + hashObject);
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in hash-object command: {ex.Message}", ex);
        }
    }

    public static void GitLsTree(string hash)
    {
        try
        {
            if(string.IsNullOrEmpty(hash) || hash.Length < 3)
            {
                throw new ArgumentException("Invalid hash.");
            }

            var folderName = hash.Substring(0, 2);
            var fileName = hash.Substring(2);
            var path = Path.Combine(".git", "objects", folderName, fileName);

            if(!File.Exists(path))
            {
                throw new FileNotFoundException($"Tree object {hash} not found.");
            }

            var decompressedData = DemoGitHelper.DecompressObjectFile(path);
            var (type, _, _) = DemoGitHelper.ParseGitObject(decompressedData);

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
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in ls-tree: {ex.Message}", ex);
        }
    }

    public static void GitWriteTree(string hash)
    {
        try
        {
            var path = DemoGitHelper.GetPathFromGitObjects(hash);
            var treeHash = DemoGitHelper.CreateTreeObject(path);
            Console.WriteLine($"Tree written successfully: {treeHash}");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in write-tree: {ex.Message}", ex);
        }
    }

    public static void AddToIndex(string hash)
    {
        try
        {
            if(!Directory.Exists(".git"))
            {
                throw new InvalidOperationException("Not a demoGit repository (or any of the parent directories).");
            }

            // Implementation for adding to index (Placeholder for now)
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in AddToIndex: {ex.Message}", ex);
        }
    }

    public static void DisplayDemoGitHelp()
    {
        try
        {
            Console.WriteLine("These are the existing demoGit commands: \n" +
                "'demogit init'      : Initializes demoGit (creates .git folder)\n" +
                "'demogit remove'    : Removes demoGit and clears all git history\n" +
                "'demogit cat-file'  : Mimics Git's object database (use with <type|size|content> <hash>)\n" +
                "'demogit status'    : Displays status of demoGit");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in DisplayDemoGitHelp: {ex.Message}", ex);
        }
    }
}

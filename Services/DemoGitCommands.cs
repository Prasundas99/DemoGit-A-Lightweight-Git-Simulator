using System.IO;

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

    public static void AddToIndex(string path)
    {
        try
        {
            if(!Directory.Exists(".git"))
            {
                throw new InvalidOperationException("Not a demoGit repository (or any of the parent directories).");
            }

            var indexPath = Path.Combine(".git", "index");

            if(!File.Exists(indexPath)) 
            { 
                File.Create(indexPath).Dispose();
            }

            // Determine if it's a single file or directory
            var filesToAdd = path == "."
                ? Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                : [path];


            var indexEntries = File.ReadAllLines(indexPath).ToList();

            foreach(var file in filesToAdd)
            {
                if(!File.Exists(file))
                {
                    Console.Error.WriteLine($"Warning: File '{file}' does not exist.");
                    continue;
                }

                var hash = DemoGitHelper.CreateBlobObject(file);
                // Add to the index
                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);

                var entry = $"{hash} {relativePath}";
                if(!indexEntries.Contains(entry))
                {
                    indexEntries.Add(entry);
                }
            }

            // Save the updated index
            File.WriteAllLines(indexPath, indexEntries);

            Console.WriteLine($"Successfully added {path} to the index.");
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

    public static void DisplayStatus()
    {
        try
        {
            // Check if the .git directory exists
            if(!Directory.Exists(".git"))
            {
                Console.WriteLine("Error: This is not a DemoGit repository.");
                return;
            }

            var indexPath = Path.Combine(".git", "index");

            // Check if the index file exists
            if(!File.Exists(indexPath))
            {
                Console.WriteLine("No files are currently staged.");
                return;
            }

            // Read the index file to get the list of staged files
            var stagedFiles = File.ReadAllLines(indexPath).ToList();

            // List files in the current directory (working tree)
            var workingDirectoryFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(Directory.GetCurrentDirectory(), f)).ToList();

            // Display staged files
            Console.WriteLine("Staged files:");
            foreach(var stagedFile in stagedFiles)
            {
                Console.WriteLine($"  {stagedFile}");
            }

            // Display unstaged files
            Console.WriteLine("\nUnstaged files:");
            foreach(var file in workingDirectoryFiles)
            {
                if(!stagedFiles.Contains(file))
                {
                    Console.WriteLine($"  {file}");
                }
            }

            // Handle untracked files (files in working directory not added to index)
            Console.WriteLine("\nUntracked files:");
            foreach(var file in workingDirectoryFiles)
            {
                if(!stagedFiles.Contains(file) && !File.Exists(Path.Combine(".git", "objects", file)))
                {
                    Console.WriteLine($"  {file}");
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error displaying status: {ex.Message}");
        }
    }
}

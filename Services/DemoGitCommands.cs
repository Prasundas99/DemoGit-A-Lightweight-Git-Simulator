using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;

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

            // Load .gitignore
            var gitignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");
            var ignorePatterns = DemoGitHelper.LoadGitignorePatterns(gitignorePath);

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

                // Skip files that match .gitignore patterns
                if(DemoGitHelper.ShouldIgnoreFile(file, ignorePatterns))
                {
                    Console.WriteLine($"Ignoring file: {file}");
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

            // Load .gitignore patterns
            var gitignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");
            var ignorePatterns = DemoGitHelper.LoadGitignorePatterns(gitignorePath);

            // Read the index file to get the list of staged files
            var stagedFiles = File.ReadAllLines(indexPath).ToList();

            // List files in the current directory (working tree)
            var workingDirectoryFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(Directory.GetCurrentDirectory(), f)).ToList();


            // Display staged files (ignoring files in .gitignore)
            Console.WriteLine("Staged files:");
            foreach(var stagedFile in stagedFiles)
            {
                if(!DemoGitHelper.IsIgnored(stagedFile, ignorePatterns))
                {
                    Console.WriteLine($"  {stagedFile}");
                }
            }

            // Display unstaged files (ignoring files in .gitignore)
            Console.WriteLine("\nUnstaged files:");
            foreach(var file in workingDirectoryFiles)
            {
                if(!stagedFiles.Contains(file) && !DemoGitHelper.IsIgnored(file, ignorePatterns))
                {
                    Console.WriteLine($"  {file}");
                }
            }

            // Handle untracked files (files in working directory not added to index or ignored)
            Console.WriteLine("\nUntracked files:");
            foreach(var file in workingDirectoryFiles)
            {
                if(!stagedFiles.Contains(file) && !File.Exists(Path.Combine(".git", "objects", file)) && !DemoGitHelper.IsIgnored(file, ignorePatterns))
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

    public static void UnstageAll()
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

            // Read the index file to get the staged files
            var stagedFiles = File.ReadAllLines(indexPath).ToList();

            // If no files are staged, inform the user
            if(stagedFiles.Count == 0)
            {
                Console.WriteLine("No files are staged.");
                return;
            }

            // Clear all files from the staged list (unstage everything)
            stagedFiles.Clear();

            // Save the updated (empty) index
            File.WriteAllLines(indexPath, stagedFiles);

            Console.WriteLine("Successfully unstaged all files.");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error unstaging all files: {ex.Message}");
        }
    }


    public static void DisplayDemoGitHelp()
    {
        try
        {
            Console.WriteLine("DemoGit - Available Commands:\n");

            Console.WriteLine("Repository Initialization:");
            Console.WriteLine("  init                    : Initialize a new DemoGit repository");
            Console.WriteLine("  remove                  : Remove DemoGit repository and clear history");
            Console.WriteLine("  clone <token> <url>     : Clone a repository from GitHub\n");

            Console.WriteLine("Working with Changes:");
            Console.WriteLine("  add <file|.>            : Add file(s) to the staging area");
            Console.WriteLine("  status                  : Show working tree status");
            Console.WriteLine("  unstage-all             : Remove all files from staging area");
            Console.WriteLine("  commit <message>        : Record changes to the repository");
            Console.WriteLine("  push <token> <repo>     : Push to GitHub repository\n");

            Console.WriteLine("Examining Repository:");
            Console.WriteLine("  cat-file <type|size|content> <hash> : Display object information");
            Console.WriteLine("  hash-object -w <file>   : Hash a file and store it");
            Console.WriteLine("  ls-tree <tree-hash>     : List contents of a tree object");
            Console.WriteLine("  write-tree -w <hash>    : Create a tree object from current index\n");

            Console.WriteLine("Note: <token> refers to your GitHub Personal Access Token");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error in DisplayDemoGitHelp: {ex.Message}", ex);
        }
    }

    public static void GitCommit(string message)
    {
        if(string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Commit message cannot be empty");
        }

        // Create tree from current index
        var treeHash = DemoGitHelper.CreateTreeFromIndex();

        // Get parent commit hash (if any)
        var parentHash = DemoGitHelper.GetLastCommitHash();

        // Create commit object
        var commitHash = DemoGitHelper.CreateCommitObject(message, treeHash, parentHash);

        // Update current branch reference
        var branch = DemoGitHelper.GetCurrentBranch();
        DemoGitHelper.UpdateRef(branch, commitHash);

        Console.WriteLine($"[{branch} {commitHash}] {message}");
    }

    public static async Task GitPush(string token, string repoName)
    {
        try
        {
            if(!Directory.Exists(".git"))
            {
                throw new InvalidOperationException("Not a git repository");
            }

            var api = new GitHubApi(token);
            await api.PushToGitHub(repoName, Directory.GetCurrentDirectory());
            Console.WriteLine($"Successfully pushed to GitHub repository: {repoName}");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Push failed: {ex.Message}", ex);
        }
    }

    public static async Task GitClone(string token, string repoUrl, string localPath)
    {
        try
        {
            // Extract repo name from URL
            var repoName = Path.GetFileNameWithoutExtension(repoUrl);

            // Create local directory
            Directory.CreateDirectory(localPath);

            var api = new GitHubApi(token);

            // Initialize git repository
            var gitPath = Path.Combine(localPath, ".git");
            Directory.CreateDirectory(gitPath);
            Directory.CreateDirectory(Path.Combine(gitPath, "objects"));
            Directory.CreateDirectory(Path.Combine(gitPath, "refs", "heads"));
            File.WriteAllText(Path.Combine(gitPath, "HEAD"), "ref: refs/heads/main\n");

            // Download repository content
            await DownloadRepository(token, repoUrl, localPath);

            Console.WriteLine($"Successfully cloned repository to {localPath}");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Clone failed: {ex.Message}", ex);
        }
    }

    private static async Task DownloadRepository(string token, string repoUrl, string localPath)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DemoGit", "1.0"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

        // Convert github.com URL to api.github.com
        var apiUrl = repoUrl.Replace("https://github.com", "https://api.github.com/repos")
                           .TrimEnd(".git".ToCharArray());

        // Get repository content
        var response = await client.GetStringAsync($"{apiUrl}/git/trees/main?recursive=1");
        using var document = JsonDocument.Parse(response);
        var tree = document.RootElement.GetProperty("tree");

        foreach(var item in tree.EnumerateArray())
        {
            var path = item.GetProperty("path").GetString();
            var type = item.GetProperty("type").GetString();
            var sha = item.GetProperty("sha").GetString();

            if(type == "blob")
            {
                // Download and save file
                var content = await client.GetStringAsync($"{apiUrl}/git/blobs/{sha}");
                using var blob = JsonDocument.Parse(content);
                var data = blob.RootElement.GetProperty("content").GetString();
                var encoding = blob.RootElement.GetProperty("encoding").GetString();

                var filePath = Path.Combine(localPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                if(encoding == "base64")
                {
                    await File.WriteAllBytesAsync(filePath, Convert.FromBase64String(data));
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, data);
                }
            }
        }
    }
}

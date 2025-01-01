using DemoGit.DTO;
using System.IO.Compression;
using System.Text;

namespace DemoGit.Services;

public static class DemoGitHelper
{
    public static (string type, string size, string content) ParseGitObject(byte[] data)
    {
        try
        {
            var decompressedContent = Encoding.UTF8.GetString(data);
            var nullIndex = decompressedContent.IndexOf('\0');
            if(nullIndex == -1) throw new InvalidDataException("Invalid Git object format.");

            var header = decompressedContent.Substring(0, nullIndex);
            var content = decompressedContent.Substring(nullIndex + 1);

            var headerParts = header.Split(' ');
            if(headerParts.Length != 2)
            {
                throw new InvalidDataException("Invalid Git object header.");
            }

            return (headerParts[0], headerParts[1], content);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in ParseGitObject: {ex.Message}");
            throw;
        }
    }

    public static byte[] DecompressObjectFile(string path)
    {
        try
        {
            var compressedData = File.ReadAllBytes(path);
            using var compressedStream = new MemoryStream(compressedData);
            using var decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            decompressionStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in DecompressObjectFile: {ex.Message}");
            throw;
        }
    }

    public static string CreateBlobObject(string filePath)
    {
        try
        {
            var fileContent = File.ReadAllBytes(filePath);

            var header = $"blob {fileContent.Length}\0";
            var headerBytes = Encoding.UTF8.GetBytes(header);

            var objectData = new byte[headerBytes.Length + fileContent.Length];
            Array.Copy(headerBytes, 0, objectData, 0, headerBytes.Length);
            Array.Copy(fileContent, 0, objectData, headerBytes.Length, fileContent.Length);

            return HashObject(objectData);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in CreateBlobObject: {ex.Message}");
            throw;
        }
    }

    private static string HashObject(byte[] objectData)
    {
        try
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var hashBytes = sha1.ComputeHash(objectData);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            StoreGitObject(hash, objectData);
            return hash;
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in HashObject: {ex.Message}");
            throw;
        }
    }

    public static void StoreGitObject(string hash, byte[] objectData)
    {
        try
        {
            var folderName = hash.Substring(0, 2);
            var fileName = hash.Substring(2);

            var objectDirectory = Path.Combine(".git", "objects", folderName);
            if(!Directory.Exists(objectDirectory))
            {
                Directory.CreateDirectory(objectDirectory);
            }

            var objectFilePath = Path.Combine(objectDirectory, fileName);
            using var fileStream = new FileStream(objectFilePath, FileMode.Create, FileAccess.Write);
            using var zlibStream = new ZLibStream(fileStream, CompressionMode.Compress);
            zlibStream.Write(objectData, 0, objectData.Length);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in StoreGitObject: {ex.Message}");
            throw;
        }
    }

    public static List<TreeEntry> ParseTreeObject(byte[] decompressedData)
    {
        try
        {
            var entries = new List<TreeEntry>();
            var index = 0;

            while(index < decompressedData.Length)
            {
                var modeEnd = Array.IndexOf(decompressedData, (byte)' ', index);
                if(modeEnd == -1)
                {
                    throw new InvalidDataException("Invalid tree object format: Unable to find mode.");
                }
                var mode = Encoding.ASCII.GetString(decompressedData, index, modeEnd - index);
                index = modeEnd + 1;

                var nameEnd = Array.IndexOf(decompressedData, (byte)'\0', index);
                if(nameEnd == -1)
                {
                    throw new InvalidDataException("Invalid tree object format: Unable to find name.");
                }
                var name = Encoding.ASCII.GetString(decompressedData, index, nameEnd - index);
                index = nameEnd + 1;

                if(index + 20 > decompressedData.Length)
                {
                    throw new InvalidDataException("Invalid tree object format: Unexpected end of data.");
                }
                var hashBytes = decompressedData.Skip(index).Take(20).ToArray();
                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                index += 20;

                entries.Add(new TreeEntry
                {
                    Mode = mode,
                    Name = name,
                    Hash = hash
                });
            }
            return entries;
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in ParseTreeObject: {ex.Message}");
            throw;
        }
    }

    public static string CreateTreeObject(string directoryPath, int depth = 0)
    {
        try
        {
            if(depth > 25)
            {
                throw new InvalidOperationException("Maximum directory depth (25) exceeded");
            }

            var entries = Directory.GetFileSystemEntries(directoryPath);

            var treeEntries = entries
                .Select(entry =>
                {
                    var fileName = Path.GetFileName(entry);
                    var isDirectory = Directory.Exists(entry);
                    var hash = isDirectory ? CreateTreeObject(entry, depth + 1) : CreateBlobObject(entry);
                    return (isDirectory ? "040000" : "100644") + " " + (isDirectory ? "tree" : "blob") + " " + hash + " " + fileName;
                })
                .ToList();

            var treeContent = string.Join("\0", treeEntries);
            var treeObjectData = Encoding.UTF8.GetBytes($"tree {treeContent.Length}\0{treeContent}");

            return HashObject(treeObjectData);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in CreateTreeObject: {ex.Message}");
            throw;
        }
    }

    public static string GetPathFromGitObjects(string hash)
    {
        try
        {
            var folderName = hash.Substring(0, 2);
            var fileName = hash.Substring(2);
            var path = Path.Combine(".git", "objects", folderName, fileName);
            if(!File.Exists(path))
            {
                throw new FileNotFoundException("Invalid Path");
            }
            return path;
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error in GetPathFromGitObjects: {ex.Message}");
            throw;
        }
    }


    //for ignoring files
    public static List<string> LoadGitignorePatterns(string gitignorePath)
    {
        var patterns = new List<string>();
        if(File.Exists(gitignorePath))
        {
            // Read lines and clean up for patterns (e.g., ignore comments and empty lines)
            patterns = File.ReadAllLines(gitignorePath)
                           .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                           .Select(line => line.Trim())
                           .ToList();
        }
        return patterns;
    }

    public static bool ShouldIgnoreFile(string file, List<string> ignorePatterns)
    {
        // Ignore files starting with a dot (.) by default
        if(Path.GetFileName(file).StartsWith("."))
        {
            return true;
        }

        foreach(var pattern in ignorePatterns)
        {
            if(MatchPattern(file, pattern))
            {
                return true;
            }
        }
        return false;
    }

    public static bool MatchPattern(string file, string pattern)
    {
        return file.Contains(pattern);
    }

    public static bool IsIgnored(string file, List<string> ignorePatterns)
    {
        return ignorePatterns.Any(pattern => DemoGitHelper.MatchPattern(file, pattern));
    }

    public static string CreateCommitObject(string message, string treeHash, string parentHash = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var author = "Demo User <demo@example.com>";

        var commitContent = new StringBuilder();
        commitContent.AppendLine($"tree {treeHash}");

        if(!string.IsNullOrEmpty(parentHash))
        {
            commitContent.AppendLine($"parent {parentHash}");
        }

        commitContent.AppendLine($"author {author} {timestamp} +0000");
        commitContent.AppendLine($"committer {author} {timestamp} +0000");
        commitContent.AppendLine();
        commitContent.AppendLine(message);

        var content = commitContent.ToString();
        var header = $"commit {content.Length}\0";
        var commitData = Encoding.UTF8.GetBytes(header + content);

        return HashObject(commitData);
    }

    public static string GetCurrentBranch()
    {
        var headContent = File.ReadAllText(Path.Combine(".git", "HEAD")).Trim();
        if(headContent.StartsWith("ref: "))
        {
            return headContent.Substring(5);
        }
        return headContent;
    }

    public static string GetLastCommitHash()
    {
        var branch = GetCurrentBranch();
        var branchPath = Path.Combine(".git", branch);

        if(File.Exists(branchPath))
        {
            return File.ReadAllText(branchPath).Trim();
        }
        return null;
    }

    public static void UpdateRef(string refPath, string hash)
    {
        var fullPath = Path.Combine(".git", refPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, hash + "\n");
    }

    public static string CreateTreeFromIndex()
    {
        var indexPath = Path.Combine(".git", "index");
        if(!File.Exists(indexPath))
        {
            throw new InvalidOperationException("Nothing to commit (index is empty)");
        }

        var entries = new List<(string mode, string type, string hash, string name)>();
        var indexLines = File.ReadAllLines(indexPath);

        foreach(var line in indexLines)
        {
            var parts = line.Split(' ', 2);
            if(parts.Length != 2) continue;

            var hash = parts[0];
            var path = parts[1];

            entries.Add(("100644", "blob", hash, path));
        }

        // Sort entries by name
        entries.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        // Create tree content
        var treeContent = new StringBuilder();
        foreach(var (mode, type, hash, name) in entries)
        {
            treeContent.Append($"{mode} {type} {hash}\t{name}\0");
        }

        var content = treeContent.ToString();
        var header = $"tree {content.Length}\0";
        var treeData = Encoding.UTF8.GetBytes(header + content);

        return HashObject(treeData);
    }
}

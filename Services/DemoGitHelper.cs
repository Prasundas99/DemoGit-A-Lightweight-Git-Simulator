using DemoGit.DTO;
using System.IO.Compression;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DemoGit.Services;

public static class DemoGitHelper
{
    public static (string type, string size, string content) ParseGitObject(byte[] data)
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

    public static byte[] DecompressObjectFile(string path)
    {
        var compressedData = File.ReadAllBytes(path);
        using var compressedStream = new MemoryStream(compressedData);
        using var decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        decompressionStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    public static string CreateBlobObject(string filePath)
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

        return HashObject(objectData);
    }

    private static string HashObject(byte[] objectData)
    {
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hashBytes = sha1.ComputeHash(objectData);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // Store the object in the .git/objects directory
        StoreGitObject(hash, objectData);
        return hash;
    }


    public static void StoreGitObject(string hash, byte[] objectData)
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

    public static List<TreeEntry> ParseTreeObject(byte[] decompressedData)
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

            // Parse the name (file or directory name) as a null-terminated string
            var nameEnd = Array.IndexOf(decompressedData, (byte)'\0', index);
            if(nameEnd == -1)
            {
                throw new InvalidDataException("Invalid tree object format: Unable to find name.");
            }
            var name = Encoding.ASCII.GetString(decompressedData, index, nameEnd - index);
            index = nameEnd + 1;

            // Parse the hash (20 bytes of binary SHA-1 data)
            if(index + 20 > decompressedData.Length)
            {
                throw new InvalidDataException("Invalid tree object format: Unexpected end of data.");
            }
            var hashBytes = decompressedData.Skip(index).Take(20).ToArray();
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            index += 20;

            // Add the parsed entry to the list
            entries.Add(new TreeEntry
            {
                Mode = mode,
                Name = name,
                Hash = hash
            });
        }
        return entries;
    }

    public static string CreateTreeObject(string directoryPath)
    {
        var entries = Directory.GetFileSystemEntries(directoryPath);

        var treeEntries = entries
            .Select(entry =>
            {
                var fileName = Path.GetFileName(entry);
                var isDirectory = Directory.Exists(entry);
                var hash = isDirectory ? CreateTreeObject(entry) : CreateBlobObject(entry);
                return (isDirectory ? "040000" : "100644") + " " + (isDirectory ? "tree" : "blob") + " " + hash + " " + fileName;
            })
            .ToList();

        var treeContent = string.Join("\0", treeEntries);
        var treeObjectData = Encoding.UTF8.GetBytes($"tree {treeContent.Length}\0{treeContent}");

        return HashObject(treeObjectData);
    }

    public static string GetPathFromGitObjects(string hash)
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
}
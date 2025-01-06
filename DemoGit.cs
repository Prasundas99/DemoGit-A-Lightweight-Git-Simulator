using DemoGit.Services;

namespace DemoGit;

internal class DemoGit(DemoGitCommands demoGitCommands)
{
    private readonly DemoGitCommands _demoGitCommands = demoGitCommands;

    public async Task HandleDemoGitCommandAsync(string arguments)
    {
        if(string.IsNullOrWhiteSpace(arguments))
        {
            throw new ArgumentException("Error: No arguments provided for the 'demogit' command.");
        }

        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLower();
        var hash = parts.Length > 1 ? parts[1] : "";

        switch(command)
        {
            case "init":
                DemoGitCommands.GitInitialization();
                break;

            case "remove":
                DemoGitCommands.GitRemove();
                break;

            case "cat-file":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit cat-file <type|size|content> <hash>");
                }

                var subcommandParts = hash.Split(' ', 2);
                if(subcommandParts.Length < 2)
                {
                    throw new ArgumentException("Usage: demogit cat-file <type|size|content> <hash>");
                }

                var subcommand = subcommandParts[0].ToLower();
                var objectHash = subcommandParts[1];

                DemoGitCommands.GitCatFile(subcommand, objectHash);
                break;

            case "hash-object":
                DemoGitCommands.HandleHashObjectCommand(parts);
                break;

            case "ls-tree":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit ls-tree <tree-hash>");
                }

                DemoGitCommands.GitLsTree(hash);
                break;

            case "write-tree":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit write-tree -w <tree-hash>");
                }

                DemoGitCommands.GitWriteTree(hash);
                break;

            case "add":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit add <file-name|.>");
                }
                DemoGitCommands.AddToIndex(hash);
                break;

            case "unstage-all":
                DemoGitCommands.UnstageAll();
                break;

            case "status":
                DemoGitCommands.DisplayStatus();
                break;

            case "commit":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit commit <message>");
                }
                DemoGitCommands.GitCommit(hash);
                break;

            case "push":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit push <token> <repository-name>");
                }
                var pushArgs = hash.Split(' ', 2);
                if(pushArgs.Length != 2)
                {
                    throw new ArgumentException("Usage: demogit push <token> <repository-name>");
                }
                await _demoGitCommands.GitPush(pushArgs[0], pushArgs[1]);
                break;

            case "clone":
                if(string.IsNullOrEmpty(hash))
                {
                    throw new ArgumentException("Usage: demogit clone <token> <repository-url>");
                }
                var cloneArgs = hash.Split(' ', 2);
                if(cloneArgs.Length != 2)
                {
                    throw new ArgumentException("Usage: demogit clone <token> <repository-url>");
                }
                await DemoGitCommands.GitClone(cloneArgs[0], cloneArgs[1], Directory.GetCurrentDirectory());
                break;

            default:
                DemoGitCommands.DisplayDemoGitHelp();
                break;
        }
    }

}

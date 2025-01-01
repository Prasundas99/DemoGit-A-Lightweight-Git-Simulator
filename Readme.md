# DemoGit

A lightweight implementation of Git fundamentals in C#, designed for educational purposes. This project demonstrates the core concepts of Git's internal workings while providing basic GitHub integration.

## Features

- **Basic Git Operations**
  - Repository initialization and removal
  - File staging and unstaging
  - Commit creation
  - Status checking
  - GitHub integration (push and clone)

- **Low-Level Git Operations**
  - Object hashing
  - Tree manipulation
  - Object content inspection
  - Index management

## Prerequisites

- .NET 6.0 or higher
- GitHub account (for remote operations)
- GitHub Personal Access Token (for push/clone operations)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/demogit.git
```

2. Build the project:
```bash
cd demogit
dotnet build
```

## Usage

### Basic Commands

#### Initialize a Repository
```bash
demogit init
```

#### Add Files to Staging
```bash
demogit add <filename>    # Add specific file
demogit add .            # Add all files
```

#### Check Repository Status
```bash
demogit status
```

#### Create a Commit
```bash
demogit commit "Your commit message"
```

#### Push to GitHub
```bash
demogit push <github-token> <repository-name>
```

#### Clone from GitHub
```bash
demogit clone <github-token> <repository-url>
```

### Advanced Commands

#### Examine Git Objects
```bash
demogit cat-file <type|size|content> <hash>
```

#### Create Object Hash
```bash
demogit hash-object -w <file>
```

#### List Tree Contents
```bash
demogit ls-tree <tree-hash>
```

#### Create Tree Object
```bash
demogit write-tree -w <hash>
```

## GitHub Integration

To use GitHub features (push/clone), you'll need a Personal Access Token:

1. Go to GitHub Settings → Developer Settings → Personal Access Tokens
2. Generate a new token with 'repo' scope
3. Save the token securely
4. Use the token in push/clone commands

## Project Structure

```
DemoGit/
├── Program.cs                 # Main program entry
├── DemoGitCommands.cs        # Command implementations
├── DemoGitHelper.cs          # Utility functions
└── Models/
    └── TreeEntry.cs          # Tree entry model
```

## Implementation Details

- Uses SHA-1 for object hashing
- Implements basic Git object types (blob, tree, commit)
- Includes .gitignore support
- Handles binary and text files
- Supports basic GitHub API integration

## Limitations

- Single branch support (main only)
- No merge functionality
- Basic GitHub integration
- No SSH support
- Limited error recovery
- No diffing functionality

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Educational Purpose

This project is designed for learning Git's internal mechanisms and should not be used as a replacement for Git in production environments.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by Git's internal design
- Built for educational purposes
- Thanks to the Git community for documentation

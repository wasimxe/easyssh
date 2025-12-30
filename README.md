# EasySSH - SSH Login Tool for Windows

A fast and reliable SSH client for Windows that properly handles Ctrl+C signals. Built to replace problematic SSH password tools that disconnect when you press Ctrl+C.

## 🎯 Problem Solved

Traditional tools like `sshpass.exe` have a critical flaw: when you press **Ctrl+C** during an SSH session, they terminate the entire connection instead of sending the interrupt signal to the remote command. This tool fixes that.

## ✨ Features

- **Proper Ctrl+C Handling** - Ctrl+C interrupts remote commands without disconnecting
- **Single Command Login** - Connect to SSH servers with one command
- **Password Authentication** - Pass password directly via command line
- **Custom Port Support** - Connect to non-standard SSH ports
- **Command Execution Mode** - Execute single commands and get output
- **Connection Timeout** - Avoid hanging on unreachable hosts
- **Full Terminal Support** - Works with vim, nano, htop, and other interactive tools
- **Single Executable** - No installation required, no dependencies
- **PowerShell & CMD Compatible** - Works identically in both environments

## 📦 Executable Location

The ready-to-use executable is located at:

```
D:\workspace\windows\sshpass\bin\Release\net8.0\win-x64\publish\easyssh.exe
```

**File Size:** ~66 MB (includes all dependencies, no .NET Runtime installation required)

## 🚀 Quick Start

### Basic SSH Login (Interactive Shell)

```powershell
easyssh.exe -h 192.168.1.100 -u admin -p mypassword
```

### SSH with Custom Port

```powershell
easyssh.exe -h server.example.com -u root -p secret -P 2222
```

### Execute Single Command

```powershell
easyssh.exe -h 10.0.0.5 -u deploy -p password -c "systemctl status nginx"
```

### With Connection Timeout

```powershell
easyssh.exe -h 192.168.1.50 -u user -p pass -t 10
```

## 📖 Command-Line Options

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `-h, --host` | ✅ Yes | - | SSH server hostname or IP address |
| `-u, --user` | ✅ Yes | - | SSH username |
| `-p, --password` | ✅ Yes | - | SSH password |
| `-P, --port` | ❌ No | 22 | SSH port number |
| `-c, --command` | ❌ No | - | Command to execute (omit for interactive shell) |
| `-t, --timeout` | ❌ No | 30 | Connection timeout in seconds |
| `--help` | ❌ No | - | Show help and usage information |

## 💡 Usage Examples

### Interactive Shell Mode

Connect to a server and get an interactive shell:

```powershell
# Basic connection
easyssh.exe -h 192.168.1.100 -u admin -p mypassword

# After connection, you can:
# - Run commands normally
# - Press Ctrl+C to interrupt long-running commands (stays connected!)
# - Use vim, nano, htop, etc.
# - Type 'exit' or press Ctrl+D to disconnect
```

### Command Execution Mode

Run a single command and return to local shell:

```powershell
# Check system status
easyssh.exe -h server.com -u admin -p pass -c "uptime"

# List files
easyssh.exe -h 10.0.0.5 -u user -p pass -c "ls -la /var/log"

# Check service status
easyssh.exe -h server.com -u root -p pass -c "systemctl status nginx"

# Multiple commands with semicolon
easyssh.exe -h server.com -u user -p pass -c "cd /app && git pull && npm install"
```

### Real-World Scenarios

```powershell
# Deploy application
easyssh.exe -h production.server.com -u deploy -p deploypass -c "cd /app && git pull && pm2 restart app"

# Check disk space on multiple servers
easyssh.exe -h server1.com -u admin -p pass -c "df -h"
easyssh.exe -h server2.com -u admin -p pass -c "df -h"

# Tail logs interactively
easyssh.exe -h server.com -u admin -p pass -c "tail -f /var/log/nginx/access.log"
# Press Ctrl+C to stop tailing (won't disconnect!)

# Run interactive tools
easyssh.exe -h server.com -u admin -p pass
# Then inside SSH: vim, nano, htop, etc. all work perfectly
```

## 🔧 Installation & Deployment

### For End Users (Recommended)

1. Copy `easyssh.exe` to your desired location
2. Run directly - no installation needed!

### Add to Windows PATH (Optional)

To use `easyssh` from anywhere:

**Option 1: Add current location to PATH (PowerShell as Admin)**
```powershell
$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
$newPath = "$currentPath;D:\workspace\windows\sshpass\bin\Release\net8.0\win-x64\publish"
[Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
```

**Option 2: Copy to System32**
```powershell
Copy-Item "D:\workspace\windows\sshpass\bin\Release\net8.0\win-x64\publish\easyssh.exe" -Destination "C:\Windows\System32\"
```

**Option 3: Copy to a custom tools folder**
```powershell
# Create a tools folder
New-Item -ItemType Directory -Path "C:\Tools" -Force
Copy-Item "D:\workspace\windows\sshpass\bin\Release\net8.0\win-x64\publish\easyssh.exe" -Destination "C:\Tools\"

# Add to PATH
$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
$newPath = "$currentPath;C:\Tools"
[Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
```

After adding to PATH, restart your terminal and use:
```powershell
easyssh -h server.com -u admin -p password
```

## 🎮 Ctrl+C Behavior Comparison

### With EasySSH ✅
```
C:\> easyssh -h server.com -u admin -p pass
admin@server:~$ ping google.com
PING google.com (142.250.185.46): 56 data bytes
64 bytes from 142.250.185.46: icmp_seq=0 ttl=117 time=12.4 ms
64 bytes from 142.250.185.46: icmp_seq=1 ttl=117 time=11.8 ms
^C                          ← Press Ctrl+C
admin@server:~$             ← Still connected! Ready for next command
```

### With Old sshpass.exe ❌
```
C:\> sshpass.exe -h server.com -u admin -p pass
admin@server:~$ ping google.com
PING google.com (142.250.185.46): 56 data bytes
64 bytes from 142.250.185.46: icmp_seq=0 ttl=117 time=12.4 ms
^C                          ← Press Ctrl+C
C:\>                        ← Disconnected! SSH session killed
```

## 🔒 Security Notes

**⚠️ Warning:** Passing passwords via command-line arguments can expose them in:
- Process lists (Task Manager, `ps` commands)
- Command history (PowerShell history, CMD history)
- Scripts stored in plain text

**For Production Use, Consider:**
- Using SSH key authentication instead
- Storing passwords in secure vaults (e.g., Azure Key Vault, HashiCorp Vault)
- Using environment variables instead of command-line arguments
- Implementing proper secret management

**This tool is best suited for:**
- Development environments
- Automation scripts with proper security controls
- Testing scenarios
- Temporary access needs

## 🛠️ Technical Details

### Architecture
- **Language:** C# .NET 8
- **SSH Library:** SSH.NET (Renci.SshNet)
- **CLI Framework:** System.CommandLine
- **Build Type:** Self-contained single-file executable

### How Ctrl+C Works

EasySSH uses three layers of Ctrl+C protection:

1. **Win32 SetConsoleCtrlHandler** - Intercepts Windows console control events
2. **Console.CancelKeyPress** - Catches .NET's cancel event and prevents termination
3. **Raw Console Mode** - Reads keyboard input byte-by-byte and detects Ctrl+C as 0x03

When Ctrl+C is pressed, the tool sends byte `0x03` through the SSH stream to the remote server, which interprets it as a standard Unix interrupt signal - exactly like pressing Ctrl+C directly on the remote machine.

### Terminal Compatibility

- Emulates **xterm-256color** terminal
- Supports ANSI escape sequences
- Handles arrow keys, Home, End, Page Up/Down, Delete
- Works with interactive tools: vim, nano, htop, less, more
- Proper terminal resizing support

## 📁 Project Structure

```
D:\workspace\windows\sshpass\
├── bin\
│   └── Release\
│       └── net8.0\
│           └── win-x64\
│               └── publish\
│                   └── easyssh.exe          ← DEPLOYABLE EXECUTABLE (66 MB)
├── Program.cs                               ← Entry point & CLI parsing
├── SshConnection.cs                         ← SSH connection management
├── InteractiveShell.cs                      ← Interactive mode with Ctrl+C handling
├── CommandExecutor.cs                       ← Single command execution
├── ConsoleInterop.cs                        ← Win32 API for console control
├── TerminalHelper.cs                        ← Terminal utilities
├── sshpass.csproj                          ← .NET project file
└── README.md                                ← This file
```

## 🔨 Building from Source

### Prerequisites
- .NET 8 SDK installed
- Windows 10/11

### Build Commands

**Development build:**
```powershell
cd D:\workspace\windows\sshpass
dotnet build
```

**Self-contained single executable (for distribution):**
```powershell
cd D:\workspace\windows\sshpass
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin\Release\net8.0\win-x64\publish\easyssh.exe`

## 🐛 Troubleshooting

### Connection timeout error
```
Error: Connection to 192.168.1.100:22 timed out after 30 seconds
```
**Solution:** Increase timeout with `-t 60` or check if the server is reachable

### Authentication failed error
```
Error: Authentication failed for user 'admin'
```
**Solution:** Verify username and password are correct

### Host unreachable error
```
Error: Could not connect to server.com:22
```
**Solution:** Check hostname/IP, port, and network connectivity

### Terminal not displaying correctly
**Solution:** Ensure your Windows Terminal or PowerShell supports ANSI escape sequences. Use Windows Terminal (recommended) instead of legacy CMD.

## 📝 Examples for PowerShell Scripts

### Automated Deployment Script
```powershell
# deploy.ps1
$server = "production.server.com"
$user = "deploy"
$password = "deploypass123"

Write-Host "Deploying application..."
easyssh.exe -h $server -u $user -p $password -c "cd /app && git pull && npm install && pm2 restart app"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment successful!" -ForegroundColor Green
} else {
    Write-Host "Deployment failed!" -ForegroundColor Red
}
```

### Check Multiple Servers
```powershell
# check-servers.ps1
$servers = @("server1.com", "server2.com", "server3.com")
$user = "admin"
$password = "adminpass"

foreach ($server in $servers) {
    Write-Host "`nChecking $server..." -ForegroundColor Cyan
    easyssh.exe -h $server -u $user -p $password -c "uptime && df -h /"
}
```

### Batch Command Execution
```powershell
# batch-update.ps1
$servers = @("web1.com", "web2.com", "web3.com")
$user = "root"
$password = "rootpass"
$command = "apt update && apt upgrade -y"

foreach ($server in $servers) {
    Write-Host "Updating $server..." -ForegroundColor Yellow
    easyssh.exe -h $server -u $user -p $password -c $command -t 300
}
```

## 📄 License

This project is provided as-is for educational and practical use.

## 🤝 Contributing

This tool was built to solve a specific problem: proper Ctrl+C handling in SSH sessions on Windows. If you find bugs or have suggestions, feel free to modify the source code.

## 📞 Support

For issues, questions, or feature requests, refer to the source code in the project directory.

---

**Made with ❤️ for Windows SSH users who are tired of broken Ctrl+C handling**

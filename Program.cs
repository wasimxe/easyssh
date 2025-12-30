using System.CommandLine;

namespace SshPass;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var hostOption = new Option<string>(
            aliases: new[] { "-h", "--host" },
            description: "SSH server hostname or IP address")
        {
            IsRequired = true
        };

        var userOption = new Option<string>(
            aliases: new[] { "-u", "--user" },
            description: "SSH username")
        {
            IsRequired = true
        };

        var passwordOption = new Option<string>(
            aliases: new[] { "-p", "--password" },
            description: "SSH password")
        {
            IsRequired = true
        };

        var portOption = new Option<int>(
            aliases: new[] { "-P", "--port" },
            getDefaultValue: () => 22,
            description: "SSH port (default: 22)");

        var commandOption = new Option<string?>(
            aliases: new[] { "-c", "--command" },
            getDefaultValue: () => null,
            description: "Command to execute (if omitted, starts interactive shell)");

        var timeoutOption = new Option<int>(
            aliases: new[] { "-t", "--timeout" },
            getDefaultValue: () => 30,
            description: "Connection timeout in seconds (default: 30)");

        var rootCommand = new RootCommand("SSH login tool with proper Ctrl+C handling")
        {
            hostOption,
            userOption,
            passwordOption,
            portOption,
            commandOption,
            timeoutOption
        };

        rootCommand.SetHandler(
            (host, user, password, port, command, timeout) =>
            {
                Environment.ExitCode = Execute(host, user, password, port, command, timeout);
            },
            hostOption, userOption, passwordOption, portOption, commandOption, timeoutOption);

        return await rootCommand.InvokeAsync(args);
    }

    static int Execute(string host, string user, string password, int port, string? command, int timeout)
    {
        try
        {
            using var connection = new SshConnection(host, port, user, password, timeout);
            var client = connection.Connect();

            if (!string.IsNullOrEmpty(command))
            {
                var executor = new CommandExecutor(client);
                return executor.Execute(command);
            }
            else
            {
                var shell = new InteractiveShell(client);
                return shell.Run();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}

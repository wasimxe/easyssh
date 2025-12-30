using Renci.SshNet;

namespace SshPass;

internal class CommandExecutor
{
    private readonly SshClient _client;

    public CommandExecutor(SshClient client)
    {
        _client = client;
    }

    public int Execute(string command)
    {
        try
        {
            using var cmd = _client.CreateCommand(command);

            var asyncResult = cmd.BeginExecute();

            var outputReader = new StreamReader(cmd.OutputStream);
            var errorReader = new StreamReader(cmd.ExtendedOutputStream);

            while (!asyncResult.IsCompleted)
            {
                var output = outputReader.ReadToEnd();
                if (!string.IsNullOrEmpty(output))
                {
                    Console.Write(output);
                }

                var error = errorReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    Console.Error.Write(error);
                }

                Thread.Sleep(10);
            }

            cmd.EndExecute(asyncResult);

            var remainingOutput = outputReader.ReadToEnd();
            if (!string.IsNullOrEmpty(remainingOutput))
            {
                Console.Write(remainingOutput);
            }

            var remainingError = errorReader.ReadToEnd();
            if (!string.IsNullOrEmpty(remainingError))
            {
                Console.Error.Write(remainingError);
            }

            return cmd.ExitStatus ?? 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Command execution error: {ex.Message}");
            return 1;
        }
    }
}

namespace SshPass;

internal static class TerminalHelper
{
    public static (uint width, uint height) GetTerminalSize()
    {
        try
        {
            return ((uint)Console.WindowWidth, (uint)Console.WindowHeight);
        }
        catch
        {
            return (80, 24);
        }
    }

    public static string GetTerminalType()
    {
        var term = Environment.GetEnvironmentVariable("TERM");
        return string.IsNullOrEmpty(term) ? "xterm-256color" : term;
    }
}

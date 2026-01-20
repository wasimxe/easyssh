using Renci.SshNet;
using System.Text;

namespace SshPass;

internal class InteractiveShell
{
    private readonly SshClient _client;
    private ShellStream? _shell;
    private ConsoleInterop.ConsoleMode _originalInputMode;
    private ConsoleInterop.ConsoleMode _originalOutputMode;
    private volatile bool _isRunning;
    private ConsoleInterop.ConsoleCtrlHandlerRoutine? _ctrlHandler;
    private readonly ManualResetEventSlim _exitEvent = new(false);

    public InteractiveShell(SshClient client)
    {
        _client = client;
    }

    public int Run()
    {
        try
        {
            SaveConsoleState();
            SetupCtrlCHandler();
            StartShell();
            RunShellLoop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Shell error: {ex.Message}");
            return 1;
        }
        finally
        {
            Cleanup();
        }
    }

    private void SaveConsoleState()
    {
        _originalInputMode = ConsoleInterop.GetInputMode();
        _originalOutputMode = ConsoleInterop.GetOutputMode();
    }

    private void SetupCtrlCHandler()
    {
        _ctrlHandler = (ctrlType) =>
        {
            if (ctrlType == ConsoleInterop.CtrlTypes.CTRL_C_EVENT)
            {
                try
                {
                    _shell?.Write(new byte[] { 0x03 }, 0, 1);
                    _shell?.Flush();
                }
                catch
                {
                }
                return true;
            }
            return false;
        };

        ConsoleInterop.SetConsoleCtrlHandler(_ctrlHandler, true);

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            try
            {
                _shell?.Write(new byte[] { 0x03 }, 0, 1);
                _shell?.Flush();
            }
            catch
            {
            }
        };
    }

    private void StartShell()
    {
        var (width, height) = TerminalHelper.GetTerminalSize();
        var termType = TerminalHelper.GetTerminalType();

        _shell = _client.CreateShellStream(
            termType,
            width,
            height,
            width,
            height,
            4096,
            new Dictionary<Renci.SshNet.Common.TerminalModes, uint>()
        );

        // Use event-based reading - no polling, no timeout needed
        _shell.DataReceived += OnDataReceived;
        _shell.Closed += OnShellClosed;

        ConsoleInterop.EnableRawMode();
        _isRunning = true;
    }

    private void OnDataReceived(object? sender, Renci.SshNet.Common.ShellDataEventArgs e)
    {
        if (e.Data.Length > 0)
        {
            var output = Encoding.UTF8.GetString(e.Data);
            Console.Write(output);
        }
    }

    private void OnShellClosed(object? sender, EventArgs e)
    {
        _isRunning = false;
        _exitEvent.Set();
    }

    private void RunShellLoop()
    {
        WriteToShell();
    }

    private void WriteToShell()
    {
        try
        {
            while (_isRunning && _shell != null)
            {
                // Check if shell closed while waiting
                if (_exitEvent.Wait(0))
                    break;

                // Console.ReadKey blocks until input - CPU efficient
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var key = Console.ReadKey(true);

                if (!_isRunning || _shell == null) break;

                if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                {
                    _shell.Write(new byte[] { 0x03 }, 0, 1);
                    _shell.Flush();
                }
                else if (key.Key == ConsoleKey.D && key.Modifiers == ConsoleModifiers.Control)
                {
                    _shell.Write(new byte[] { 0x04 }, 0, 1);
                    _shell.Flush();
                    _isRunning = false;
                    break;
                }
                else
                {
                    var bytes = GetKeyBytes(key);
                    if (bytes != null && bytes.Length > 0)
                    {
                        _shell.Write(bytes, 0, bytes.Length);
                        _shell.Flush();
                    }
                }
            }
        }
        catch
        {
        }
    }

    private byte[] GetKeyBytes(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter)
        {
            return new byte[] { 0x0D };
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            return new byte[] { 0x7F };
        }
        else if (key.Key == ConsoleKey.Tab)
        {
            return new byte[] { 0x09 };
        }
        else if (key.Key == ConsoleKey.Escape)
        {
            return new byte[] { 0x1B };
        }
        else if (key.Key == ConsoleKey.UpArrow)
        {
            return new byte[] { 0x1B, 0x5B, 0x41 };
        }
        else if (key.Key == ConsoleKey.DownArrow)
        {
            return new byte[] { 0x1B, 0x5B, 0x42 };
        }
        else if (key.Key == ConsoleKey.RightArrow)
        {
            return new byte[] { 0x1B, 0x5B, 0x43 };
        }
        else if (key.Key == ConsoleKey.LeftArrow)
        {
            return new byte[] { 0x1B, 0x5B, 0x44 };
        }
        else if (key.Key == ConsoleKey.Home)
        {
            return new byte[] { 0x1B, 0x5B, 0x48 };
        }
        else if (key.Key == ConsoleKey.End)
        {
            return new byte[] { 0x1B, 0x5B, 0x46 };
        }
        else if (key.Key == ConsoleKey.Delete)
        {
            return new byte[] { 0x1B, 0x5B, 0x33, 0x7E };
        }
        else if (key.Key == ConsoleKey.Insert)
        {
            return new byte[] { 0x1B, 0x5B, 0x32, 0x7E };
        }
        else if (key.Key == ConsoleKey.PageUp)
        {
            return new byte[] { 0x1B, 0x5B, 0x35, 0x7E };
        }
        else if (key.Key == ConsoleKey.PageDown)
        {
            return new byte[] { 0x1B, 0x5B, 0x36, 0x7E };
        }
        else if (key.KeyChar != '\0')
        {
            return Encoding.UTF8.GetBytes(new[] { key.KeyChar });
        }

        return Array.Empty<byte>();
    }

    private void Cleanup()
    {
        _isRunning = false;

        try
        {
            if (_shell != null)
            {
                _shell.DataReceived -= OnDataReceived;
                _shell.Closed -= OnShellClosed;
            }
        }
        catch
        {
        }

        try
        {
            ConsoleInterop.RestoreMode(_originalInputMode, _originalOutputMode);
        }
        catch
        {
        }

        try
        {
            if (_ctrlHandler != null)
            {
                ConsoleInterop.SetConsoleCtrlHandler(_ctrlHandler, false);
            }
        }
        catch
        {
        }

        try
        {
            _shell?.Dispose();
        }
        catch
        {
        }

        try
        {
            _exitEvent.Dispose();
        }
        catch
        {
        }
    }
}

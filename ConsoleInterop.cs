using System.Runtime.InteropServices;

namespace SshPass;

internal static class ConsoleInterop
{
    public delegate bool ConsoleCtrlHandlerRoutine(CtrlTypes ctrlType);

    public enum CtrlTypes : uint
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    [Flags]
    public enum ConsoleMode : uint
    {
        ENABLE_PROCESSED_INPUT = 0x0001,
        ENABLE_LINE_INPUT = 0x0002,
        ENABLE_ECHO_INPUT = 0x0004,
        ENABLE_WINDOW_INPUT = 0x0008,
        ENABLE_MOUSE_INPUT = 0x0010,
        ENABLE_INSERT_MODE = 0x0020,
        ENABLE_QUICK_EDIT_MODE = 0x0040,
        ENABLE_EXTENDED_FLAGS = 0x0080,
        ENABLE_AUTO_POSITION = 0x0100,
        ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,

        ENABLE_PROCESSED_OUTPUT = 0x0001,
        ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
        ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
        DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
        ENABLE_LVB_GRID_WORLDWIDE = 0x0010
    }

    private const int STD_INPUT_HANDLE = -10;
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_ERROR_HANDLE = -12;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine? handlerRoutine, bool add);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleMode lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleMode dwMode);

    public static IntPtr GetStdInputHandle() => GetStdHandle(STD_INPUT_HANDLE);
    public static IntPtr GetStdOutputHandle() => GetStdHandle(STD_OUTPUT_HANDLE);
    public static IntPtr GetStdErrorHandle() => GetStdHandle(STD_ERROR_HANDLE);

    public static ConsoleMode GetInputMode()
    {
        var handle = GetStdInputHandle();
        GetConsoleMode(handle, out var mode);
        return mode;
    }

    public static ConsoleMode GetOutputMode()
    {
        var handle = GetStdOutputHandle();
        GetConsoleMode(handle, out var mode);
        return mode;
    }

    public static bool SetInputMode(ConsoleMode mode)
    {
        var handle = GetStdInputHandle();
        return SetConsoleMode(handle, mode);
    }

    public static bool SetOutputMode(ConsoleMode mode)
    {
        var handle = GetStdOutputHandle();
        return SetConsoleMode(handle, mode);
    }

    public static void EnableRawMode()
    {
        var inputMode = GetInputMode();
        inputMode &= ~(ConsoleMode.ENABLE_LINE_INPUT | ConsoleMode.ENABLE_ECHO_INPUT | ConsoleMode.ENABLE_PROCESSED_INPUT);
        inputMode |= ConsoleMode.ENABLE_VIRTUAL_TERMINAL_INPUT;
        SetInputMode(inputMode);

        var outputMode = GetOutputMode();
        outputMode |= ConsoleMode.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        SetOutputMode(outputMode);
    }

    public static void RestoreMode(ConsoleMode inputMode, ConsoleMode outputMode)
    {
        SetInputMode(inputMode);
        SetOutputMode(outputMode);
    }
}

using Renci.SshNet;
using Renci.SshNet.Common;

namespace SshPass;

internal class SshConnection : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly int _timeoutSeconds;
    private SshClient? _client;

    public SshConnection(string host, int port, string username, string password, int timeoutSeconds = 30)
    {
        _host = host;
        _port = port;
        _username = username;
        _password = password;
        _timeoutSeconds = timeoutSeconds;
    }

    public SshClient Connect()
    {
        try
        {
            var connectionInfo = new ConnectionInfo(
                _host,
                _port,
                _username,
                new PasswordAuthenticationMethod(_username, _password))
            {
                Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
            };

            _client = new SshClient(connectionInfo);

            var connectTask = Task.Run(() => _client.Connect());
            if (!connectTask.Wait(TimeSpan.FromSeconds(_timeoutSeconds)))
            {
                throw new TimeoutException($"Connection to {_host}:{_port} timed out after {_timeoutSeconds} seconds");
            }

            if (!_client.IsConnected)
            {
                throw new Exception($"Failed to connect to {_host}:{_port}");
            }

            return _client;
        }
        catch (SshAuthenticationException ex)
        {
            throw new Exception($"Authentication failed for user '{_username}': {ex.Message}", ex);
        }
        catch (SshConnectionException ex)
        {
            throw new Exception($"Could not connect to {_host}:{_port}: {ex.Message}", ex);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"SSH connection error: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

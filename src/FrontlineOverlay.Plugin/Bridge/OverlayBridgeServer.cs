using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using FrontlineOverlay.Plugin.Core.Events;
using FrontlineOverlay.Plugin.Core.Serialization;

namespace FrontlineOverlay.Plugin.Bridge;

public sealed class OverlayBridgeServer : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource cancellation = new();
    private readonly ConcurrentDictionary<Guid, WebSocket> clients = new();

    private volatile bool isRunning;
    private Task? acceptLoop;

    public OverlayBridgeServer(int port)
    {
        Endpoint = new Uri($"ws://127.0.0.1:{port}/frontline-overlay-jp/");
        listener = new TcpListener(IPAddress.Loopback, port);
    }

    public Uri Endpoint { get; }

    public bool IsRunning => isRunning;

    public Task StartAsync()
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        listener.Start();
        isRunning = true;
        acceptLoop = Task.Run(() => AcceptLoopAsync(cancellation.Token));
        return Task.CompletedTask;
    }

    public async Task BroadcastAsync(OverlayEventEnvelope payload, CancellationToken token = default)
    {
        var json = OverlayEventJsonSerializer.Serialize(payload);
        var buffer = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(buffer);

        foreach (var (id, socket) in clients)
        {
            if (socket.State != WebSocketState.Open)
            {
                clients.TryRemove(id, out _);
                continue;
            }

            try
            {
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, token).ConfigureAwait(false);
            }
            catch (WebSocketException)
            {
                clients.TryRemove(id, out _);
            }
            catch (ObjectDisposedException)
            {
                clients.TryRemove(id, out _);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        cancellation.Cancel();

        isRunning = false;
        listener.Stop();

        foreach (var (id, socket) in clients)
        {
            clients.TryRemove(id, out _);
            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Plugin stopped", CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (WebSocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }

            socket.Dispose();
        }

        cancellation.Dispose();

        if (acceptLoop is not null)
        {
            try
            {
                await acceptLoop.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient tcpClient;
            try
            {
                tcpClient = await listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException) when (token.IsCancellationRequested)
            {
                break;
            }

            _ = Task.Run(() => HandleClientAsync(tcpClient, token), token);
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken token)
    {
        using (tcpClient)
        {
            var stream = tcpClient.GetStream();
            var request = await ReadHttpHeaderAsync(stream, token).ConfigureAwait(false);

            if (!TryGetWebSocketKey(request, out var webSocketKey))
            {
                await WriteTextResponseAsync(stream, "Frontline Overlay JP Plugin bridge", token).ConfigureAwait(false);
                return;
            }

            var acceptKey = Convert.ToBase64String(
                SHA1.HashData(Encoding.ASCII.GetBytes(webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

            var response = string.Join(
                "\r\n",
                "HTTP/1.1 101 Switching Protocols",
                "Upgrade: websocket",
                "Connection: Upgrade",
                $"Sec-WebSocket-Accept: {acceptKey}",
                "\r\n");

            await stream.WriteAsync(Encoding.ASCII.GetBytes(response), token).ConfigureAwait(false);

            using var socket = WebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(30));
            var id = Guid.NewGuid();
            clients[id] = socket;
            await DrainClientAsync(id, socket, token).ConfigureAwait(false);
        }
    }

    private static async Task<string> ReadHttpHeaderAsync(NetworkStream stream, CancellationToken token)
    {
        var buffer = new byte[1024];
        var builder = new StringBuilder();

        while (builder.Length < 8192)
        {
            var read = await stream.ReadAsync(buffer, token).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            builder.Append(Encoding.ASCII.GetString(buffer, 0, read));
            if (builder.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static bool TryGetWebSocketKey(string request, out string webSocketKey)
    {
        webSocketKey = string.Empty;
        var lines = request.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0 || !lines[0].StartsWith("GET /frontline-overlay-jp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (var line in lines)
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = line[..separatorIndex].Trim();
            if (!name.Equals("Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            webSocketKey = line[(separatorIndex + 1)..].Trim();
            return webSocketKey.Length > 0;
        }

        return false;
    }

    private static async Task WriteTextResponseAsync(NetworkStream stream, string text, CancellationToken token)
    {
        var body = Encoding.UTF8.GetBytes(text);
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");

        await stream.WriteAsync(header, token).ConfigureAwait(false);
        await stream.WriteAsync(body, token).ConfigureAwait(false);
    }

    private async Task DrainClientAsync(Guid id, WebSocket socket, CancellationToken token)
    {
        var buffer = new byte[1024];

        try
        {
            while (!token.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, token).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        finally
        {
            clients.TryRemove(id, out _);
            socket.Dispose();
        }
    }
}

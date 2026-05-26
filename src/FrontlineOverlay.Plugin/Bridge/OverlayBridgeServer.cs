using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using FrontlineOverlay.Plugin.Core.Events;
using FrontlineOverlay.Plugin.Core.Serialization;

namespace FrontlineOverlay.Plugin.Bridge;

public sealed class OverlayBridgeServer : IAsyncDisposable
{
    private readonly HttpListener listener = new();
    private readonly CancellationTokenSource cancellation = new();
    private readonly ConcurrentDictionary<Guid, WebSocket> clients = new();

    private Task? acceptLoop;

    public OverlayBridgeServer(int port)
    {
        Endpoint = new Uri($"http://127.0.0.1:{port}/frontline-overlay-jp/");
        listener.Prefixes.Add(Endpoint.ToString());
    }

    public Uri Endpoint { get; }

    public bool IsRunning => listener.IsListening;

    public Task StartAsync()
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        listener.Start();
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

        if (listener.IsListening)
        {
            listener.Stop();
        }

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

        listener.Close();
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
        while (!token.IsCancellationRequested && listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            if (!context.Request.IsWebSocketRequest)
            {
                await WriteHealthResponseAsync(context).ConfigureAwait(false);
                continue;
            }

            var socketContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
            var id = Guid.NewGuid();
            clients[id] = socketContext.WebSocket;
            _ = Task.Run(() => DrainClientAsync(id, socketContext.WebSocket, token), token);
        }
    }

    private static async Task WriteHealthResponseAsync(HttpListenerContext context)
    {
        const string body = "Frontline Overlay JP Plugin bridge";
        var buffer = Encoding.UTF8.GetBytes(body);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer).ConfigureAwait(false);
        context.Response.Close();
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

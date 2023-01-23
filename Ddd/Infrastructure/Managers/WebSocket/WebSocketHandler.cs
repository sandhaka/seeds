using System.Net.WebSockets;
using System.Text;

namespace Ddd.Infrastructure.Managers.WebSocket;

public abstract class WebSocketHandler
{
    protected WebSocketConnectionManager WebSocketConnectionManager { get; }

    public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
    {
        WebSocketConnectionManager = webSocketConnectionManager ??
                                     throw new ArgumentNullException(nameof(webSocketConnectionManager));
    }

    public virtual void OnConnected(System.Net.WebSockets.WebSocket socket)
    {
        WebSocketConnectionManager.AddSocket(socket);
    }

    public virtual async Task OnDisconnected(System.Net.WebSockets.WebSocket socket)
    {
        await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
    }

    public async Task SendMessageAsync(System.Net.WebSockets.WebSocket socket, string message)
    {
        if (socket.State != WebSocketState.Open)
        {
            return;
        }

        await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                offset: 0,
                count: message.Length),
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None);
    }

    public async Task SendMessageAsync(string socketId, string message)
    {
        await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
    }

    public async Task SendMessageToAllAsync(string message)
    {
        foreach(var (_, webSocket) in WebSocketConnectionManager.GetAll())
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await SendMessageAsync(webSocket, message);
            }
        }
    }

    public abstract Task ReceiveAsync(System.Net.WebSockets.WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
}
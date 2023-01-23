using System.Net.WebSockets;
using Ddd.Infrastructure.Managers.WebSocket;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ddd.Infrastructure.Middlewares;

public class WebSocketMiddleware : IMiddleware
{
    private readonly WebSocketHandler _webSocketHandler;
    private readonly ILogger _logger;

    public WebSocketMiddleware(WebSocketHandler webSocketHandler, ILogger logger)
    {
        _webSocketHandler = webSocketHandler ?? throw new ArgumentNullException(nameof(webSocketHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();

            _webSocketHandler.OnConnected(socket);

            await Receive(socket, async(result, buffer) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                        return;
                    case WebSocketMessageType.Close:
                    {
                        await _webSocketHandler.OnDisconnected(socket);
                        _logger.LogInformation($"Socket {socket} was closed");
                        return;
                    }
                    case WebSocketMessageType.Binary:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }
        else
        {
            await next(context);
        }
    }

    private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
    {
        var buffer = new byte[1024 * 4];

        while(socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                cancellationToken: CancellationToken.None);

            handleMessage(result, buffer);
        }
    }
}
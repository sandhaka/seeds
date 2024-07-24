using System.Net.WebSockets;
using Ddd.Infrastructure.Managers.WebSocket;
using Moq;

namespace Tests.Ddd.Infrastructure.WebSocketsService;

public class WebSocketConnectionManagerTests
{
    private readonly WebSocketConnectionManager _sut;
    private readonly Mock<WebSocket> _webSocket;

    public WebSocketConnectionManagerTests()
    {
        _sut = new WebSocketConnectionManager();
        _webSocket = new Mock<WebSocket>();
    }

    [Fact]
    public void ShouldAdd()
    {
        // Act
        _sut.AddSocket(_webSocket.Object);

        // Verify
        Assert.True(_sut.GetAll().Count == 1);
        Assert.NotEmpty(_sut.GetId(_webSocket.Object));
    }

    [Fact]
    public void ShouldRemove()
    {
        // Setup
        _sut.AddSocket(_webSocket.Object);
        _webSocket.Setup(s =>
            s.CloseAsync( It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

        var id = _sut.GetId(_webSocket.Object);

        // Act
        _sut.RemoveSocket(id);
        _webSocket.Verify(s =>
            s.CloseAsync(
                It.Is<WebSocketCloseStatus>(a => a.Equals(WebSocketCloseStatus.NormalClosure)),
                It.IsAny<string>(), It.Is<CancellationToken>(a => a.Equals(CancellationToken.None)
                )), Times.Exactly(1));
    }
}
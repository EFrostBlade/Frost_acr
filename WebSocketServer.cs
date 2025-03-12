using AEAssist.Helper;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public sealed class WebSocketServer
{
    private static readonly Lazy<WebSocketServer> _instance = new(() => new WebSocketServer());

    public static WebSocketServer Instance => _instance.Value;

    private HttpListener? _httpListener;
    private WebSocket? _webSocket;

    // 私有构造函数，防止外部实例化
    private WebSocketServer() { }

    // 启动 WebSocket 服务端
    public async Task StartAsync(string prefix)
    {
        if (_httpListener != null && _httpListener.IsListening)
        {
            LogHelper.Print("WebSocket 服务已在运行中");
            return;
        }

        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(prefix);
        _httpListener.Start();

        LogHelper.Print($"WebSocket 服务已启动，监听地址：{prefix}");

        _ = AcceptClientsAsync();
    }

    // 接受客户端连接
    private async Task AcceptClientsAsync()
    {
        while (_httpListener != null && _httpListener.IsListening)
        {
            var httpContext = await _httpListener.GetContextAsync();

            if (httpContext.Request.IsWebSocketRequest)
            {
                var wsContext = await httpContext.AcceptWebSocketAsync(null);
                _webSocket = wsContext.WebSocket;

                LogHelper.Print("客户端已连接");

                // 开始接收消息
                _ = ReceiveAsync(_webSocket);
            }
            else
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.Close();
            }
        }
    }

    // 接收客户端消息
    private async Task ReceiveAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, "关闭连接", CancellationToken.None);

                LogHelper.Print("客户端已断开连接");
            }
            else
            {
                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                LogHelper.Print($"收到消息：{message}");

                // 根据需要处理接收到的消息
            }
        }
    }

    // 向客户端发送消息的方法
    public async Task SendMessageAsync(string message)
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            LogHelper.Print($"发送消息：{message}");
        }
        else
        {
            LogHelper.Print("没有可用的客户端连接");
        }
    }

    // 停止 WebSocket 服务
    public void Stop()
    {
        _httpListener?.Stop();
        _httpListener = null;

        _webSocket?.Dispose();
        _webSocket = null;

        LogHelper.Print("WebSocket 服务已停止");
    }
}

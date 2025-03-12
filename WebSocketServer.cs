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

    // ˽�й��캯������ֹ�ⲿʵ����
    private WebSocketServer() { }

    // ���� WebSocket �����
    public async Task StartAsync(string prefix)
    {
        if (_httpListener != null && _httpListener.IsListening)
        {
            LogHelper.Print("WebSocket ��������������");
            return;
        }

        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(prefix);
        _httpListener.Start();

        LogHelper.Print($"WebSocket ������������������ַ��{prefix}");

        _ = AcceptClientsAsync();
    }

    // ���ܿͻ�������
    private async Task AcceptClientsAsync()
    {
        while (_httpListener != null && _httpListener.IsListening)
        {
            var httpContext = await _httpListener.GetContextAsync();

            if (httpContext.Request.IsWebSocketRequest)
            {
                var wsContext = await httpContext.AcceptWebSocketAsync(null);
                _webSocket = wsContext.WebSocket;

                LogHelper.Print("�ͻ���������");

                // ��ʼ������Ϣ
                _ = ReceiveAsync(_webSocket);
            }
            else
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.Close();
            }
        }
    }

    // ���տͻ�����Ϣ
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
                    WebSocketCloseStatus.NormalClosure, "�ر�����", CancellationToken.None);

                LogHelper.Print("�ͻ����ѶϿ�����");
            }
            else
            {
                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                LogHelper.Print($"�յ���Ϣ��{message}");

                // ������Ҫ������յ�����Ϣ
            }
        }
    }

    // ��ͻ��˷�����Ϣ�ķ���
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

            LogHelper.Print($"������Ϣ��{message}");
        }
        else
        {
            LogHelper.Print("û�п��õĿͻ�������");
        }
    }

    // ֹͣ WebSocket ����
    public void Stop()
    {
        _httpListener?.Stop();
        _httpListener = null;

        _webSocket?.Dispose();
        _webSocket = null;

        LogHelper.Print("WebSocket ������ֹͣ");
    }
}

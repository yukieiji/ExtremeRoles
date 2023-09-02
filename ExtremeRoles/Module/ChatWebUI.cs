using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.ApiHandler;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class ChatWebUI :
	NullableSingleton<ChatWebUI>
{
	public static string Path => $"{PostChat.Path}ui/";
	public static string SocketUrl => $"{ApiServer.Url}{Path}ws/";

	public readonly record struct WebUIChat(
		string PlayerName, string Chat, bool isRight);

	private WebSocket? socket = null;
	private HttpListener listener;

	public ChatWebUI()
	{
		this.listener = new HttpListener();
		this.listener.Prefixes.Add(SocketUrl);
		this.listener.Start();
	}

	public async Task ConnectWs()
	{
		var hc = await this.listener.GetContextAsync();

		//クライアントからのリクエストがWebSocketでない場合は処理を中断
		if (!hc.Request.IsWebSocketRequest)
		{
			//クライアント側にエラー(400)を返却し接続を閉じる
			hc.Response.StatusCode = 400;
			hc.Response.Close();
			return;
		}

		//WebSocketでレスポンスを返却
		var wsc = await hc.AcceptWebSocketAsync(null);
		this.socket = wsc.WebSocket;

		this.AddChatToWebUI(
			new WebUIChat(
				Translation.GetString("SystemMessage"),
				Translation.GetString("SuccessAmongUsConnect"),
			false));
	}

	public void ResetChat()
	{
		this.sendData(Array.Empty<byte>());
	}

	public void AddChatToWebUI(ChatBubble bubble)
	{
		var chat = new WebUIChat(
			bubble.NameText.text,
			bubble.TextArea.text,
			bubble.Player.cosmetics.FlipX);
		AddChatToWebUI(chat);
	}

	public void AddChatToWebUI(WebUIChat chat)
	{
		string jsonText = JsonSerializer.Serialize(chat);

		byte[] buffer = Encoding.UTF8.GetBytes(jsonText);
		this.sendData(buffer);
	}

	private void sendData(byte[] data)
	{
		var segment = new ArraySegment<byte>(data);

		this.socket?.SendAsync(
			segment,
			WebSocketMessageType.Text,
			true, CancellationToken.None);
	}
}

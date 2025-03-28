using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.NetworkInformation;

using ExtremeRoles.Module.Interface;
using System.Linq;

namespace ExtremeRoles.Module;

#nullable enable

public class ApiServer : IDisposable
{
	public static string Url => $"http://localhost:{port}";

	public static ApiServer Instance
	{
		get
		{
			if (instance == null)
			{
				throw new InvalidOperationException("Register URLs");
			}
			return instance;
		}
	}

	private readonly HttpListener listener;
	private readonly Thread listenerThread;

	private const int port = 57700;

	public record RequestKey(string Url, string Method);

	private readonly IReadOnlyDictionary<RequestKey, IRequestHandler> registedHandler = handler;

	private static ApiServer? instance;
	private static Dictionary<RequestKey, IRequestHandler> handler = new Dictionary<RequestKey, IRequestHandler>();

	private ApiServer()
	{
		this.listener = new HttpListener();
		foreach (RequestKey key in this.registedHandler.Keys)
		{
			this.listener.Prefixes.Add($"{Url}{key.Url}");
		}
		this.listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		try
		{
			this.listener.Start();
		}
		catch
		{
			this.listener.Close();
			throw;
		}

		this.listenerThread = new Thread(startListener);
		this.listenerThread.Start();
	}

	public static void Create()
	{
		if (instance is not null)
		{
			return;
		}

		if (handler.Count == 0)
		{
			ExtremeRolesPlugin.Logger.LogWarning($"ExR ApiServer: Disable, Register URL is ZERO");
			return;
		}

		var ips = IPGlobalProperties.GetIPGlobalProperties();
		IPEndPoint[] tcpConnInfoArray = ips.GetActiveTcpListeners();
		if (tcpConnInfoArray.Any(x => x.Port == port))
		{
			ExtremeRolesPlugin.Logger.LogError($"ExR ApiServer: Can't boot. port:{port} used already!!");
			return;
		}
		try
		{
			instance = new ApiServer();
		}
		catch (Exception ex)
		{
			ExtremeRolesPlugin.Logger.LogError($"ExR ApiServer: Can't boot: {ex}");
			instance = null;
		}
	}

	public static void Register(string url, HttpMethod method, IRequestHandler handle)
	{
		if (!url.EndsWith('/'))
		{
			url = $"{url}/";
		}
		if (!url.StartsWith('/'))
		{
			url = $"/{url}";
		}
		handler.Add(new(url, method.Method), handle);
	}

	public static void Stop()
	{
		instance?.Dispose();
	}

	public void Dispose()
	{
		this.listenerThread.Join();
		this.listener.Stop();
		this.listener.Close();
	}

	private void startListener()
	{
		while (this.listener.IsListening)
		{
			var result = this.listener.BeginGetContext(this.listenerCallback, this.listener);
			result.AsyncWaitHandle.WaitOne();
		}
	}

	// REST APIを呼び出す際、パスは「\」ではなく「/」を使い、ヘッダーの「Contents-Type」は常につける
	private void listenerCallback(IAsyncResult result)
	{
		if (!this.listener.IsListening) { return; }
		HttpListenerContext context = this.listener.EndGetContext(result);

		string? url = context.Request.RawUrl;

		if (string.IsNullOrEmpty(url))
		{
			return;
		}

		url = url.Split('?')[0];

		RequestKey key = new RequestKey(url, context.Request.HttpMethod);

		if (this.registedHandler.TryGetValue(key, out var handle) ||
			handle != null)
		{
			UnityMainThreadDispatcher.Instance.Enqueue(
				() =>
				{
					try
					{
						handle.Request.Invoke(context);
					}
					catch (Exception e)
					{
						ExtremeRolesPlugin.Logger.LogError(e);
						returnInternalError(context.Response, e);
					}
				});
		}
		else
		{
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			context.Response.Abort();
		}
	}

	private static void returnInternalError(HttpListenerResponse response, Exception cause)
	{
		response.StatusCode = (int)HttpStatusCode.InternalServerError;
		response.ContentType = "text/plain";

		using var writer = new StreamWriter(response.OutputStream, Encoding.UTF8);
		writer.Write(cause.ToString());

		response.Close();
	}
}

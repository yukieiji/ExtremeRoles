using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module;

#nullable enable

public class ApiServer : IDisposable
{
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

	private HttpListener listener;
	private Thread listenerThread;

	private const string url = "http://localhost:57700";

	public record RequestKey(string Url, string Method);

	private readonly IReadOnlyDictionary<RequestKey, IRequestHandler> registedHandler = handler;

	private static ApiServer? instance;
	private static Dictionary<RequestKey, IRequestHandler> handler = new Dictionary<RequestKey, IRequestHandler>();

	private ApiServer()
	{
		this.listener = new HttpListener();
		foreach (RequestKey key in this.registedHandler.Keys)
		{
			this.listener.Prefixes.Add($"{url}{key.Url}");
		}
		this.listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		this.listener.Start();

		this.listenerThread = new Thread(startListener);
		this.listenerThread.Start();
	}

	public static void Create()
	{
		if (handler.Count == 0)
		{
			return;
		}
		instance = new ApiServer();
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

	public void Dispose()
	{
		this.listener.Stop();
		this.listenerThread.Join();
	}

	private void startListener()
	{
		while (this.listener.IsListening)
		{
			var result = this.listener.BeginGetContext(this.listenerCallback, this.listener);
			result.AsyncWaitHandle.WaitOne();
		}
	}

	private void listenerCallback(IAsyncResult result)
	{
		if (!this.listener.IsListening) { return; }
		HttpListenerContext context = listener.EndGetContext(result);

		string? url = context.Request.RawUrl;

		if (string.IsNullOrEmpty(url))
		{
			return;
		}

		RequestKey key = new RequestKey(url, context.Request.HttpMethod);

		try
		{
			if (this.registedHandler.TryGetValue(key, out var handle) ||
				handle != null)
			{
				UnityMainThreadDispatcher.Instance.Enqueue(
					() => handle.Request.Invoke(context));
			}
			else
			{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.Abort();
			}
		}
		catch (Exception e)
		{
			returnInternalError(context.Response, e);
		}
	}

	private void returnInternalError(HttpListenerResponse response, Exception cause)
	{
		response.StatusCode = (int)HttpStatusCode.InternalServerError;
		response.ContentType = "text/plain";
		try
		{
			using (var writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
				writer.Write(cause.ToString());
			response.Close();
		}
		catch
		{
			response.Abort();
		}
	}
}

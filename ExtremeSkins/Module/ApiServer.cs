using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeSkins.Module.ApiHandler;

namespace ExtremeSkins.Module;


public class ApiServer : NullableSingleton<ApiServer>, IDisposable
{
	private HttpListener listener;
	private Thread listenerThread;

	private const string url = "http://localhost:57700";

	public enum RequestType
	{
		Get,
		Post,
	}
	private readonly IReadOnlyDictionary<string, IRequestHandler> handler = new Dictionary<string, IRequestHandler>()
	{
		{
			$"/status/", new GetStatusHandler()
		},
	};

	public ApiServer()
	{
		this.listener = new HttpListener();
		foreach (string rawUrl in this.handler.Keys)
		{
			this.listener.Prefixes.Add($"{url}{rawUrl}");
		}
		this.listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		this.listener.Start();

		this.listenerThread = new Thread(startListener);
		this.listenerThread.Start();
	}

	public void Dispose()
	{
		this.listener.Stop();
		this.listenerThread.Join();
		this.Destroy();
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

		try
		{
			if (this.handler.TryGetValue(url, out var handle) ||
				handle != null)
			{
				switch (handle.Type)
				{
					case RequestType.Get:
						processGetRequest(context, handle);
						break;
					case RequestType.Post:
						processPostRequest(context, handle);
						break;
					default:
						return;
				}
			}
			else
			{
				context.Response.Abort();
			}
		}
		catch (Exception e)
		{
			returnInternalError(context.Response, e);
		}
	}

	private bool isHttpMethod(HttpMethod expected, string requested) =>
		string.Equals(expected.Method, requested, StringComparison.CurrentCultureIgnoreCase);

	private void processGetRequest(HttpListenerContext context, IRequestHandler handle)
	{
		HttpListenerResponse response = context.Response;

		if (!isHttpMethod(HttpMethod.Get, context.Request.HttpMethod) || context.Request.IsWebSocketRequest)
		{
			response.StatusCode = (int)HttpStatusCode.BadRequest;
			response.Abort();
		}

		//メインスレッドでGetリクエストイベントを呼び出し
		UnityMainThreadDispatcher.Instance().Enqueue(() => handle.Request.Invoke(context));
	}

	private void processPostRequest(HttpListenerContext context, IRequestHandler handle)
	{
		HttpListenerResponse response = context.Response;
		HttpListenerRequest request = context.Request;

		if (!isHttpMethod(HttpMethod.Post, request.HttpMethod) ||
			request.IsWebSocketRequest ||
			request.ContentType != IRequestHandler.JsonContent)
		{
			response.StatusCode = (int)HttpStatusCode.BadRequest;
			response.Abort();
			return;
		}

		//メインスレッドでGetリクエストイベントを呼び出し
		UnityMainThreadDispatcher.Instance().Enqueue(() => handle.Request.Invoke(context));
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

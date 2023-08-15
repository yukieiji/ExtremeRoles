using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ExtremeRoles.Module.Interface;

public interface IRequestHandler
{
	public const string JsonContent = "application/json";

	public Action<HttpListenerContext> Request { get; }

	protected static string GetJsonString(HttpListenerRequest request)
	{
		string jsonStr;
		using (var reader = new StreamReader(
			request.InputStream, request.ContentEncoding))
		{
			jsonStr = reader.ReadToEnd();
		}
		return jsonStr;
	}

	protected static void SetStatusOK(HttpListenerResponse response)
	{
		response.StatusCode = (int)HttpStatusCode.OK;
	}

	protected static void SetContentsType(HttpListenerResponse response)
	{
		response.ContentType = JsonContent;
		response.ContentEncoding = Encoding.UTF8;
	}

	protected static void Write<T>(HttpListenerResponse response, T writeObject)
	{
		using var stream = new MemoryStream();
		JsonSerializer.Serialize(stream, writeObject);
		stream.GetBuffer();

		response.Close(stream.ToArray(), false);
	}
}

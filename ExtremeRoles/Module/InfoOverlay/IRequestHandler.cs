using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExtremeRoles.Module.Interface;

public interface IRequestHandler
{
	public const string JsonContent = "application/json";

	public Action<HttpListenerContext> Request { get; }

	protected static T DeserializeJson<T>(HttpListenerRequest request)
	{
		string jsonStr;
		using (var reader = new StreamReader(
			request.InputStream, request.ContentEncoding))
		{
			jsonStr = reader.ReadToEnd();
		}

		var options = new JsonSerializerOptions()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			// Pythonで作ったREST APIの呼び出しが成功しない[不具合](https://github.com/dotnet/runtime/issues/76130)があるらしいのでこれを付ける必要がある
			PropertyNameCaseInsensitive = true
		};
		T newHat = JsonSerializer.Deserialize<T>(jsonStr, options);

		return newHat;
	}

	protected static void SetStatusOK(HttpListenerResponse response)
	{
		response.StatusCode = (int)HttpStatusCode.OK;
	}

	protected static void SetStatusNG(HttpListenerResponse response)
	{
		response.StatusCode = (int)HttpStatusCode.BadRequest;
	}

	protected static void SetContentsType(HttpListenerResponse response)
	{
		response.ContentType = JsonContent;
		response.ContentEncoding = Encoding.UTF8;
	}

	protected static void Write<T>(HttpListenerResponse response, T writeObject)
	{
		using var stream = new MemoryStream();
		JsonSerializer.Serialize(
			stream, writeObject,
			new JsonSerializerOptions()
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = true
			});
		stream.GetBuffer();

		response.Close(stream.ToArray(), false);
	}
}

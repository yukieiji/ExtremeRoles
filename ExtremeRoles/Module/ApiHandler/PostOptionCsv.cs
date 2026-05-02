using System;

using System.IO;
using System.Text;
using System.Net;

using ExtremeRoles.Module.Interface;


namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct PostCsvRequest(string CsvBody);

public sealed class PostOptionCsv : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (LobbyBehaviour.Instance == null ||
			GameManager.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.currentGameOptions == null ||
			GameOptionsManager.Instance.gameOptionsFactory == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var data = IRequestHandler.DeserializeJson<PostCsvRequest>(context.Request);
		byte[] arr = Encoding.UTF8.GetBytes(data.CsvBody);

		using var stream = new MemoryStream(arr);
		using var reader = new StreamReader(stream, encoding: Encoding.UTF8);
		if (!CustomOptionCsvProcessor.TryImportToStream(reader))
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}

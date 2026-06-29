using System;

using System.IO;
using System.Text;
using System.Net;

using ExtremeRoles.Module.Interface;


namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct GetCsvResult(DateTime ExportAt, string Version, string CsvBody);

public sealed class GetOptionCsv : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (LobbyBehaviour.Instance == null ||
			GameManager.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.currentGameOptions == null ||
			GameOptionsManager.Instance.gameOptionsFactory == null ||
			!AmongUsClient.Instance.AmHost)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		using var stream = new MemoryStream();
		using var writer = new StreamWriter(stream, encoding: Encoding.UTF8);
		CustomOptionCsvProcessor.ExportToStream(writer);
		writer.Flush();
		string csvStr = Encoding.UTF8.GetString(stream.ToArray());

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(
			response,
			new GetCsvResult(
				DateTime.UtcNow,
				this.GetType()?.Assembly?.GetName()?.Version?.ToString() ?? "",
				csvStr));
	}
}

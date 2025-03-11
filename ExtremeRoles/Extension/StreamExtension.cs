using System;
using System.IO;

using System.Threading;

using SystemTask = System.Threading.Tasks.Task;

namespace ExtremeRoles.Extension.System.IO;

// .NET 6には未実装のためこいう感じに実装する

public static class StreamExtension
{
	public static void ReadExactly(this Stream stream, in byte[] byteArray, int offset, int length)
	{
		int size = stream.Read(byteArray, 0, length);
		if (size != length)
		{
			ExtremeRolesPlugin.Logger.LogError($"Can't read {length}, except {size}");
		}
	}

	public static void ReadExactly(this Stream stream, in Span<byte> buffer)
	{
		int length = buffer.Length;
		int size = stream.Read(buffer);
		if (size != length)
		{
			ExtremeRolesPlugin.Logger.LogError($"Can't read {length}, except {size}");
		}
	}

	public static async SystemTask ReadExactlyAsync(
		this Stream stream, byte[] byteArray, int offset, int length, CancellationToken cancellationToken)
	{
		int size = await stream.ReadAsync(byteArray, offset, length, cancellationToken);
		if (size != length)
		{
			ExtremeRolesPlugin.Logger.LogError($"Can't read {length}, except {size}");
		}
	}
}

using System;

namespace ExtremeRoles.Helper;

public static class ModId
{
	public static void Register()
	{
		if (string.IsNullOrEmpty(CurrentModRegistration.ModRegistrationGuidString))
		{
			CurrentModRegistration.ModRegistrationGuidString = ExtremeRolesPlugin.ModId;
		}
		else
		{
			Combine(ExtremeRolesPlugin.ModId); // 何らかのModが登録されている場合は、ModIdを組み合わせる
		}
	}

	public static void Combine(string modId)
	{
		if (!Guid.TryParse(modId, out Guid add))
		{
			ExtremeRolesPlugin.Logger.LogError($"Invalid modId: {modId}");
			return;
		}
		if (!Guid.TryParse(CurrentModRegistration.ModRegistrationGuidString, out Guid guid))
		{
			return;
		}

		byte[] guid1Byte = guid.ToByteArray();
		byte[] guid2Byte = add.ToByteArray();

		if (guid1Byte.Length != guid2Byte.Length)
		{
			ExtremeRolesPlugin.Logger.LogError($"Invalid modId: {modId}");
			return;
		}


		byte[] destByte = new byte[guid1Byte.Length];

		for (int i = 0; i < guid1Byte.Length; i++)
		{
			destByte[i] = (byte)(guid1Byte[i] ^ guid2Byte[i]);
		}
		var newId = new Guid(destByte);
		CurrentModRegistration.ModRegistrationGuidString = newId.ToString();
	}
}

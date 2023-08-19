using ExtremeRoles.Module.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hazel;

namespace ExtremeRoles.Module;

public sealed class ExtremeSystemTypeManager :
	NullableSingleton<ExtremeSystemTypeManager>,
	IAmongUs.ISystemType
{

	public static SystemTypes Type => (SystemTypes)60;

	public bool IsDirty => true;

	public void Deserialize(MessageReader reader, bool initialState)
	{
	}

	public void Detoriorate(float deltaTime)
	{
	}

	public void Reset()
	{

	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
	}

	public void RpcUpdateSystem()
	{
		MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
		ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Ventilation, messageWriter);
		messageWriter.Recycle();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{

	}
}

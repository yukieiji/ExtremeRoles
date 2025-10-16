using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using System.Collections;
using System.Collections.Generic;


#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeRoleAssignee(
	IRoleAssignDataBuilder builder) : IRoleAssignee
{
	private readonly IRoleAssignDataBuilder builder = builder;

	public IEnumerator CoRpcAssign()
	{
		var data = builder.Build();

		yield return null;

		rpcAssignToExRole(data);
	}

	private static void rpcAssignToExRole(IReadOnlyList<IPlayerToExRoleAssignData> assignData)
	{
		using (var caller = RPCOperator.CreateCaller(
			PlayerControl.LocalPlayer.NetId,
			RPCOperator.Command.SetRoleToAllPlayer))
		{
			caller.WritePackedInt(assignData.Count); // 何個あるか

			foreach (IPlayerToExRoleAssignData data in assignData)
			{
				caller.WriteByte(data.PlayerId); // PlayerId
				caller.WriteByte(data.RoleType); // RoleType : single or comb
				caller.WritePackedInt(data.RoleId); // RoleId
				caller.WritePackedInt(data.ControlId); // int GameContId

				if (data.RoleType == (byte)IPlayerToExRoleAssignData.ExRoleType.Comb)
				{
					var combData = (PlayerToCombRoleAssignData)data;
					caller.WriteByte(combData.CombTypeId); // combTypeId
					caller.WriteByte(combData.AmongUsRoleId); // byted AmongUsVanillaRoleId
				}
			}
		}
		RPCOperator.SetRoleToAllPlayer(assignData);

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
	}
}

using System.Collections;
using System.Collections.Generic;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeRoleAssignee(
	IRoleAssignDataPreparer preparer,
	IRoleAssignDataBuilder builder) : IRoleAssignee
{
	public PreparationData PreparationData { get; } = preparer.Prepare();
	private readonly IRoleAssignDataBuilder builder = builder;

	public IEnumerator CoRpcAssign()
	{
		var data = builder.Build(this.PreparationData);

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
		RoleAssignState.Instance.SwitchRoleAssignToEnd();
	}


}

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MastermindNoticeSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.MastermindNotice;

	public static MastermindNoticeSystem CreateOrGet()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<MastermindNoticeSystem>(Type);

	public void UpdateSystem(PlayerControl player, Hazel.MessageReader msgReader) { }

	public void Reset(ResetTiming timing, PlayerControl? player)
	{
		if (timing == ResetTiming.MeetingStart)
		{
			int aliveMastermindCount = 0;
			foreach (var p in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (p != null && !p.IsDead && !p.Disconnected)
				{
					if (ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) && role.Core.Id == ExtremeRoleId.Mastermind)
					{
						aliveMastermindCount++;
					}
				}
			}

			if (aliveMastermindCount > 0 && HudManager.Instance != null && HudManager.Instance.Chat != null && PlayerControl.LocalPlayer != null)
			{
				string message = Tr.GetString("mastermindAliveChat", aliveMastermindCount);
				HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, message);
			}
		}
	}
}

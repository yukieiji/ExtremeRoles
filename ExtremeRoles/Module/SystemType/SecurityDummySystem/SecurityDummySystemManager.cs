using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.SystemType.SecurityDummySystem;

public interface ISecurityDummySystem
{
	public void Add(params byte[] players);

	public void Remove(params byte[] players);

	public void Clear();

	public void Begin();

	public void Close();
}

public sealed class SecurityDummySystemManager : IExtremeSystemType
{
	public bool IsActive { get; set; } = false;

	public enum DummyMode
	{
		Normal,
		No
	}

	public enum Option
	{
		Add,
		Remove,
	}

	public static SecurityDummySystemManager Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<SecurityDummySystemManager>(ExtremeSystemType.SecurityDummySystem);

	public static bool TryGet([NotNullWhen(true)] out SecurityDummySystemManager? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.SecurityDummySystem, out system);

	public DummyMode Mode { get; set; } = DummyMode.Normal;
	public bool IsLog => Map.Id == 2;

	private ISecurityDummySystem mainSystem = Map.Id == 2 ? new SecurityLogDummySystem() : new SurveillanceDummySystem();

	public void PostfixBegin()
		=> this.mainSystem.Begin();

	public void PostfixClose()
		=> this.mainSystem.Close();

	public void Add(params byte[] players)
		=> this.mainSystem.Add(players);

	public void Remove(params byte[] players)
		=> this.mainSystem.Remove(players);

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.OnPlayer)
		{
			this.mainSystem.Clear();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var opt = (Option)msgReader.ReadByte();
		byte playerId = msgReader.ReadByte();

		switch (opt)
		{
			case Option.Add:
				this.Add(playerId);
				break;
			case Option.Remove:
				this.Remove(playerId);
				break;
			default:
				break;
		}
	}
}

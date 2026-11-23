using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

/// <summary>
/// ホセとホセ候補の登録を一元管理する共有レジストラ
/// </summary>
public sealed class FemmeFataleStatusModel : IStatusModel
{
    private readonly HashSet<byte> josePlayerId = [];
    private readonly HashSet<byte> joseCandidatePlayerId = [];

    public IReadOnlyCollection<byte> JosePlayerIds => josePlayerId;
    public IReadOnlyCollection<byte> JoseCandidatePlayerIds => joseCandidatePlayerId;

    public void AddJose(byte playerId)
    {
        josePlayerId.Add(playerId);
    }

    public void RemoveJose(byte playerId)
    {
        josePlayerId.Remove(playerId);
    }

    public void AddCandidate(byte playerId)
    {
        joseCandidatePlayerId.Add(playerId);
    }

    public void RemoveCandidate(byte playerId)
    {
        joseCandidatePlayerId.Remove(playerId);
    }

    public void ClearAll()
    {
        josePlayerId.Clear();
        joseCandidatePlayerId.Clear();
    }

    public bool IsJoseOrCandidate(byte playerId)
    {
        return josePlayerId.Contains(playerId) ||
            joseCandidatePlayerId.Contains(playerId);
    }

    public IReadOnlyList<byte> GetLivingJoses()
    {
        return getLiving(josePlayerId).ToList();
    }

    public IReadOnlyList<byte> GetLivingJosesAndCandidates()
    {
		var living = getLiving(josePlayerId).Concat(getLiving(joseCandidatePlayerId));
        return living.ToList();
    }

    private static IEnumerable<byte> getLiving(HashSet<byte> playerIds)
    {
        foreach (byte playerId in playerIds)
        {
            if (Player.TryGetPlayerControl(playerId, out var player) && player.IsAlive())
            {
                yield return playerId;
			}
        }
    }
}

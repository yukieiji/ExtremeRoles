using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions; // PlayerControl のため
using ExtremeRoles.Roles;    // ExtremeRoleId, ExtremeRoleManager のため
using ExtremeRoles.Roles.API; // SingleRoleBase のため
using ExtremeRoles.Module.Interface; // IPlayerToExRoleAssignData のため (仮。実際の場所に合わせて修正)

// PlayerRoleAssignData, PreparationData, ISpawnLimiter, PlayerToSingleRoleAssignData, PlayerToCombRoleAssignData のため
// これらの型が ExtremeRoles.Module.RoleAssign 名前空間にあると想定

namespace ExtremeRoles.Module.RoleAssign;

public class RoleDependencyRule
{
    public int RoleA_Id { get; }
    public int RoleB_Id { get; }
    public Func<SingleRoleBase, bool> SettingCChecker { get; }

    public RoleDependencyRule(ExtremeRoleId roleA, ExtremeRoleId roleB, Func<SingleRoleBase, bool> settingCChecker)
    {
        RoleA_Id = (int)roleA;
        RoleB_Id = (int)roleB;
        SettingCChecker = settingCChecker;
    }
}

public class RoleAssignValidator : IRoleAssignValidator
{
    private readonly IEnumerable<RoleDependencyRule> dependencyRules;

    public RoleAssignValidator(IEnumerable<RoleDependencyRule> dependencyRules)
    {
        this.dependencyRules = dependencyRules ?? new List<RoleDependencyRule>();
    }

    public bool IsReBuild(PreparationData prepareData)
    {
        bool dataWasActuallyRemoved = false;
        var currentAssignments = prepareData.Assign.Data; // IReadOnlyList であり、ループ中に変更されない参照

        // ループ中に prepareData.Assign.RemoveAssignment が呼ばれると currentAssignments の実体が変更されるため、
        // ToList() でコピーを作成してイテレーションする方が安全だが、
        // RemoveAssignment の中で currentAssignments が直接変更されることを期待している。
        // ただし、RoleIdとPlayerIdのペアで削除するので、複数の同一役職持ちプレイヤーがいるケースを考慮し、
        // 削除が発生したらループをやり直すか、慎重なイテレーションが必要。
        // 今回は、一度のIsReBuild呼び出しで複数のルールやプレイヤーにまたがる削除を許容するため、
        // 削除が発生してもループは継続する。ただし、currentAssignments の状態変更には注意。
        // 最も安全なのは、対象をリストアップし、その後まとめて削除・追加処理を行うことだが、
        // 複雑になるため、今回は逐次処理とする。

        foreach (var rule in this.dependencyRules)
        {
            // RoleAが割り当てられているアサインメント情報を一時リストに保持
            // (元のリストを変更しながらループすると問題が起きるため)
            var assignmentsOfRoleA = currentAssignments
                .Where(a => GetRoleIdFromAssignment(a) == rule.RoleA_Id)
                .Select(a => new { PlayerId = GetPlayerIdFromAssignment(a), RoleId = GetRoleIdFromAssignment(a) }) // Assignmentオブジェクトそのものは不要
                .ToList();

            foreach (var itemAInfo in assignmentsOfRoleA)
            {
                // itemAInfo.Assignment はもう使わないので、PlayerId と RoleId だけあれば良い
                SingleRoleBase? roleA_Instance = null;
                if (ExtremeRoleManager.TryGetRole(itemAInfo.PlayerId, out var gameRoleInstance) && gameRoleInstance.Id == (ExtremeRoleId)rule.RoleA_Id)
                {
                    roleA_Instance = gameRoleInstance;
                }

                if (roleA_Instance == null) continue;

                bool settingC_IsValid = rule.SettingCChecker(roleA_Instance);

                // currentAssignments は prepareData.Assign.Data を参照しており、RemoveAssignment によって変更される可能性がある。
                // そのため、roleB_ExistsInAnyPlayer のチェックは、rule.RoleA_Id の削除前に毎回行う必要がある。
                bool roleB_ExistsInAnyPlayer = prepareData.Assign.Data.Any(b => GetRoleIdFromAssignment(b) == rule.RoleB_Id);

                if (!roleB_ExistsInAnyPlayer && settingC_IsValid)
                {
                    bool removedThisTime = prepareData.Assign.RemoveAssignment(itemAInfo.PlayerId, rule.RoleA_Id);

                    if (removedThisTime)
                    {
                        dataWasActuallyRemoved = true; // IsReBuild の結果として true を返すフラグ

                        PlayerControl? playerControlToRequeue = PlayerControl.AllPlayerControls.FirstOrDefault(pc => pc.PlayerId == itemAInfo.PlayerId);
                        if (playerControlToRequeue != null)
                        {
                            prepareData.Assign.AddPlayerToReassign(playerControlToRequeue);
                        }

                        if (ExtremeRoleManager.NormalRole.TryGetValue(rule.RoleA_Id, out var roleADefinition))
                        {
                            prepareData.Limit.Reduce(roleADefinition.Team);
                        }
                        // コンビネーション役職の考慮は現状省略
                    }
                }
            }
        }
        return dataWasActuallyRemoved;
    }

    private int GetRoleIdFromAssignment(IPlayerToExRoleAssignData assignment)
    {
        if (assignment is PlayerToSingleRoleAssignData single) return single.RoleId;
        if (assignment is PlayerToCombRoleAssignData comb) return comb.RoleId;
        return -1;
    }

    private byte GetPlayerIdFromAssignment(IPlayerToExRoleAssignData assignment)
    {
        if (assignment is PlayerToSingleRoleAssignData single) return single.PlayerId;
        if (assignment is PlayerToCombRoleAssignData comb) return comb.PlayerId;
        return 0;
    }
}

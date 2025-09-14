using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Bomber : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
    public enum BomberOption
    {
        ExplosionRange,
        ExplosionKillChance,
        TimerMaxTime,
        TimerMinTime,
        TellExplosion
    }

    private float timer = 0f;
    private float timerMinTime = 0f;
    private float timerMaxTime = 0f;
    private int explosionKillChance;
    private float explosionRange;
    private bool tellExplosion;
    private PlayerControl setTargetPlayer;
    private PlayerControl bombSettingPlayer;

    private Queue<byte> bombPlayerId;
    private TMPro.TextMeshPro tellText;

    public ExtremeAbilityButton Button
    {
        get => this.bombButton;
        set
        {
            this.bombButton = value;
        }
    }
    private ExtremeAbilityButton bombButton;


    public Bomber() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Bomber),
        true, false, true, true)
    { }

    public void CreateAbility()
    {

        this.CreateActivatingAbilityCountButton(
            "setBomb",
			UnityObjectLoader.LoadFromResources<Sprite>(ObjectPath.Bomb),
			CheckAbility, CleanUp, ForceCleanUp);
    }

    public bool IsAbilityUse()
    {
        this.setTargetPlayer = Player.GetClosestPlayerInKillRange();
        return IRoleAbility.IsCommonUse() && this.setTargetPlayer != null;
    }

    public void ForceCleanUp()
    {
        this.bombSettingPlayer = null;
    }

    public void CleanUp()
    {
        if (this.bombSettingPlayer != null)
        {
            this.bombPlayerId.Enqueue(this.bombSettingPlayer.PlayerId);
            this.bombSettingPlayer = null;
        }
    }

    public bool CheckAbility()
		=> GameSystem.TryGetKillDistance(out var range) &&
			Player.IsPlayerInRangeAndDrawOutLine(
				PlayerControl.LocalPlayer,
				this.bombSettingPlayer, this,
				range[this.KillRange]);

	public bool UseAbility()
    {
        this.bombSettingPlayer = this.setTargetPlayer;
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 5, 2.5f);
        factory.CreateIntOption(
            BomberOption.ExplosionRange,
            2, 1, 5, 1);
        factory.CreateIntOption(
            BomberOption.ExplosionKillChance,
            50, 25, 75, 1,
            format: OptionUnit.Percentage);
        factory.CreateFloatOption(
            BomberOption.TimerMinTime,
            15f, 5.0f, 30f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateFloatOption(
            BomberOption.TimerMaxTime,
            60f, 45f, 75f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateBoolOption(
            BomberOption.TellExplosion,
            true);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

        this.timerMinTime = cate.GetValue<BomberOption, float>(
            BomberOption.TimerMinTime);
        this.timerMaxTime = cate.GetValue<BomberOption, float>(
            BomberOption.TimerMaxTime);
        this.explosionKillChance = cate.GetValue<BomberOption, int>(
            BomberOption.ExplosionKillChance);
        this.explosionRange = cate.GetValue<BomberOption, int>(
		BomberOption.ExplosionRange);
        this.tellExplosion = cate.GetValue<BomberOption, bool>(
            BomberOption.TellExplosion);

        this.bombPlayerId = new Queue<byte>();
        resetTimer();
    }

    public void ResetOnMeetingStart()
    {
        if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected) { return; }
        if (this.bombPlayerId.Count == 0) { return; }

        if (MeetingHud.Instance != null ||
            ShipStatus.Instance == null ||
            GameData.Instance == null) { return; }
        if (!ShipStatus.Instance.enabled ||
			OnemanMeetingSystemManager.IsActive) { return; }

        this.timer -= Time.deltaTime;
        if (this.timer > 0) { return; }

        resetTimer();

        byte bombTargetPlayerId = this.bombPlayerId.Dequeue();
        PlayerControl bombPlayer = Player.GetPlayerControlById(bombTargetPlayerId);

        if (bombPlayer == null) { return; }
        if (bombPlayer.Data.IsDead || bombPlayer.Data.Disconnected) { return; }

        HashSet<PlayerControl> target = getAllPlayerInExplosion(
            rolePlayer, bombPlayer);
        foreach (PlayerControl player in target)
        {
            if (explosionKillChance > Random.RandomRange(0, 100))
            {
                explosionKill(bombPlayer, player);
            }
        }
        explosionKill(bombPlayer, bombPlayer);
        if (this.tellExplosion)
        {
            rolePlayer.StartCoroutine(showText());
        }
    }

    private void resetTimer()
    {
        this.timer = Random.RandomRange(
            this.timerMinTime, this.timerMaxTime);
    }

    private HashSet<PlayerControl> getAllPlayerInExplosion(
        PlayerControl rolePlayer,
        PlayerControl sourcePlayer)
    {
        HashSet<PlayerControl> result = new HashSet<PlayerControl>();

        Vector2 truePosition = sourcePlayer.GetTruePosition();

        foreach (NetworkedPlayerInfo playerInfo in
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {

            if (!playerInfo.Disconnected &&
                !playerInfo.IsDead &&
                (playerInfo.PlayerId != sourcePlayer.PlayerId) &&
                (!playerInfo.Object.inVent || ExtremeGameModeManager.Instance.ShipOption.Vent.CanKillVentInPlayer) &&
                (!ExtremeRoleManager.GameRole[playerInfo.PlayerId].IsImpostor() ||
                 playerInfo.PlayerId == rolePlayer.PlayerId))
            {
                PlayerControl @object = playerInfo.Object;
                if (@object)
                {
                    Vector2 vector = @object.GetTruePosition() - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= this.explosionRange &&
                        !PhysicsHelpers.AnyNonTriggersBetween(
                            truePosition, vector.normalized,
                            magnitude, Constants.ShipAndObjectsMask))
                    {
                        result.Add(@object);
                    }
                }
            }
        }

        return result;

    }

    private IEnumerator showText()
    {
        if (this.tellText == null)
        {
            this.tellText = Object.Instantiate(
                Prefab.Text, Camera.main.transform, false);
            this.tellText.transform.localPosition = new Vector3(-3.75f, -2.5f, -250.0f);
            this.tellText.enableWordWrapping = true;
            this.tellText.GetComponent<RectTransform>().sizeDelta = new Vector2(3.0f, 0.75f);
            this.tellText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
            this.tellText.gameObject.layer = 5;
            this.tellText.text = Tr.GetString("explosionText");
        }
        this.tellText.gameObject.SetActive(true);

        yield return new WaitForSeconds(90f);

        this.tellText.gameObject.SetActive(false);

    }

    private static void explosionKill(
        PlayerControl bombPlayer,
        PlayerControl target)
    {

        if (Crewmate.BodyGuard.TryRpcKillGuardedBodyGuard(
                bombPlayer.PlayerId, target.PlayerId))
        {
            return;
        }

        Player.RpcUncheckMurderPlayer(
            bombPlayer.PlayerId,
            target.PlayerId,
            byte.MaxValue);

        ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
            target.PlayerId, ExtremeShipStatus.PlayerStatus.Explosion);
    }
}

﻿using System.Collections;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;



using BepInEx.Unity.IL2CPP.Utils;
using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class TimeMaster : SingleRoleBase, IRoleAutoBuildAbility, IKilledFrom
{
    public enum TimeMasterOption
    {
        RewindTime
    }

    public enum TimeMasterOps : byte
    {
        ShieldOff,
        ShieldOn,
        RewindTime,
        ResetMeeting,
    }

    public ExtremeAbilityButton Button
    {
        get => this.timeShieldButton;
        set
        {
            this.timeShieldButton = value;
        }
    }
    private ExtremeAbilityButton timeShieldButton;

    private bool isRewindTime = false;
    private bool isShieldOn = false;
    private SpriteRenderer rewindScreen;

    private static TimeMasterHistory history;

    public TimeMaster() : base(
        ExtremeRoleId.TimeMaster,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.TimeMaster.ToString(),
        ColorPalette.TimeMasterBlue,
        false, true, false, false)
    { }

    public static void Ability(ref MessageReader reader)
    {
        byte tmPlayerId = reader.ReadByte();
        TimeMasterOps ops = (TimeMasterOps)reader.ReadByte();
        switch (ops)
        {
            case TimeMasterOps.ShieldOff:
                shieldOff(tmPlayerId);
                break;
            case TimeMasterOps.ShieldOn:
                shieldOn(tmPlayerId);
                break;
            case TimeMasterOps.RewindTime:
                startRewind(tmPlayerId);
                break;
            case TimeMasterOps.ResetMeeting:
                resetMeeting(tmPlayerId);
                break;
            default:
                break;
        }
    }

    public static void ResetHistory()
    {
        history = null;
    }

    private static void startRewind(byte playerId)
    {
        if (history.BlockAddHistory) { return; }

        history.StartCoroutine(coRewind(playerId, PlayerControl.LocalPlayer));
    }

    private static IEnumerator coRewind(
        byte rolePlayerId, PlayerControl localPlayer)
    {

        // Enable rewind
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(rolePlayerId);
        if (timeMaster == null) { yield break; }
        timeMaster.isRewindTime = true;

        history.BlockAddHistory = true;

        // Screen Initialize
        if (timeMaster.rewindScreen == null)
        {
            timeMaster.rewindScreen = Object.Instantiate(
                 HudManager.Instance.FullScreen,
                 HudManager.Instance.transform);
            timeMaster.rewindScreen.transform.localPosition = new Vector3(0f, 0f, 20f);
            timeMaster.rewindScreen.gameObject.SetActive(true);
            timeMaster.rewindScreen.enabled = false;
            timeMaster.rewindScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
        }
        // Screen On
        timeMaster.rewindScreen.enabled = true;

        // SetUp
        if (MapBehaviour.Instance)
        {
            MapBehaviour.Instance.Close();
        }
        if (Minigame.Instance)
        {
            Minigame.Instance.ForceClose();
        }

        float time = Time.fixedDeltaTime;

        // 梯子とか使っている最中に巻き戻すと色々とおかしくなる
        // => その処理が終わるまで待機、巻き戻しはその後
        //    ただし、処理が終わるまでの間の時間巻き戻し時間は短くなる
        int skipFrame = 0;
        if (!localPlayer.inVent && !localPlayer.moveable)
        {
            do
            {
                yield return new WaitForSeconds(time);
                ++skipFrame;
            }
            while (!localPlayer.moveable);
        }

        int rewindFrame = history.Size - skipFrame;

        Logging.Debug($"History Size:{history.Size}   SkipFrame:{skipFrame}");

        Vector3 prevPos = localPlayer.transform.position;
        Vector3 sefePos = prevPos;
        bool isNotSafePos = false;
        int frameCount = 0;

        // Rewind Main Process
        foreach (var hist in history.GetAllHistory())
        {
            if (rewindFrame == frameCount) { break; }

            yield return new WaitForSeconds(time);

            if (localPlayer.PlayerId == rolePlayerId) { continue; }

            ++frameCount;

            localPlayer.moveable = false;

			Vector3 newPos = hist.Pos;

			if (localPlayer.Data.IsDead)
            {
                localPlayer.transform.position = newPos;
            }
            else
            {
                if (localPlayer.inVent)
                {
                    foreach (Vent vent in ShipStatus.Instance.AllVents)
                    {
                        bool canUse;
                        bool couldUse;
                        vent.CanUse(
                            localPlayer.Data,
                            out canUse, out couldUse);
                        if (canUse)
                        {
                            localPlayer.MyPhysics.RpcExitVent(vent.Id);
                            vent.SetButtons(false);
                        }
                    }
                }

                Vector2 offset = localPlayer.Collider.offset;
                Vector3 newTruePos = new Vector3(
                    newPos.x + offset.x,
                    newPos.y + offset.y,
                    newPos.z);
                Vector3 prevTruePos = new Vector3(
                    prevPos.x + offset.x,
                    prevPos.y + offset.y,
                    newPos.z);

                bool isAnythingBetween = PhysicsHelpers.AnythingBetween(
                    prevTruePos, newTruePos,
                    Constants.ShipAndAllObjectsMask, false);


                // (間に何もない and 動ける) or ベント内だったの座標だった場合
                // => 巻き戻しかつ、安全な座標を更新
                if ((!isAnythingBetween && hist.CanMove) || hist.InVent)
                {
                    localPlayer.transform.position = newPos;
                    prevPos = newPos;
                    sefePos = newPos;
                    isNotSafePos = false;
                }
                // 何か使っている時の座標(梯子、移動床等)
                // => 巻き戻すが、安全ではない(壁抜けする)座標として記録
                else if (hist.IsUsed)
                {
                    localPlayer.transform.position = newPos;
                    prevPos = newPos;
                    isNotSafePos = true;
                }
                else
                {
                    localPlayer.transform.position = prevPos;
                }
            }
        }

        // 最後の巻き戻しが壁抜けする座標だった場合、壁抜けしない安全な場所に飛ばす
        if (isNotSafePos)
        {
            localPlayer.transform.position = sefePos;
        }

        localPlayer.moveable = true;
        timeMaster.isRewindTime = false;
        timeMaster.rewindScreen.enabled = false;

        history.ResetAfterRewind();
    }

    private static void shieldOn(byte playerId)
    {
        TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);

        if (timeMaster != null)
        {
            timeMaster.isShieldOn = true;
        }
    }

    private static void shieldOff(byte playerId)
    {
        TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);

        if (timeMaster != null)
        {
            timeMaster.isShieldOn = false;
        }
    }
    private static void resetMeeting(byte playerId)
    {
        TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);

        if (timeMaster == null) { return; }

        // ヒストリーのコルーチン処理を止める
        history.StopAllCoroutines();

        timeMaster.isShieldOn = false;
        timeMaster.isRewindTime = false;
        if (timeMaster.rewindScreen != null)
        {
            timeMaster.rewindScreen.enabled = false;
        }

        // ヒストリーブロック解除
        history.BlockAddHistory = false;

		if (MeetingHud.Instance != null)
		{
			// 会議開始後リウィンドのコルーチンが止まるまでポジションがバグるので
			// ここでポジションを上書きする => TMが発動してなくても通るが問題なし
			// それ以外でコードを追加してもいいが最も被害が少ない変更がここ
			ShipStatus.Instance.SpawnPlayer(
				PlayerControl.LocalPlayer,
				GameData.Instance.PlayerCount, false);
		}

        PlayerControl.LocalPlayer.moveable = true;
    }

    public void CleanUp()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ShieldOff);
        }
        shieldOff(localPlayer.PlayerId);
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "timeShield",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.TimeMasterTimeShield),
            abilityOff: this.CleanUp);
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ShieldOn);
        }
        shieldOn(localPlayer.PlayerId);

        return true;
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingStart()
    {

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ResetMeeting);
        }
        resetMeeting(localPlayer.PlayerId);
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public bool TryKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        if (this.isRewindTime) { return false; }

        if (this.isShieldOn)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.TimeMasterAbility))
            {
                caller.WriteByte(rolePlayer.PlayerId);
                caller.WriteByte((byte)TimeMasterOps.RewindTime);
            }
            startRewind(rolePlayer.PlayerId);

            return false;
        }

        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory, 3.0f);

        factory.CreateFloatOption(
            TimeMasterOption.RewindTime,
            5.0f, 1.0f, 60.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        if (history != null || PlayerControl.LocalPlayer == null) { return; }

        history = PlayerControl.LocalPlayer.gameObject.AddComponent<
            TimeMasterHistory>();
        history.Initialize(
            this.Loader.GetValue<TimeMasterOption, float>(
                TimeMasterOption.RewindTime));
    }
}

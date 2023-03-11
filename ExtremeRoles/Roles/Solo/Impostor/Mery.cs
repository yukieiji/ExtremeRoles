using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Hazel;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Extension.Ship;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Mery : SingleRoleBase, IRoleAbility
{
    public sealed class Camp : IUpdatableObject
    {
        private HashSet<byte> player = new HashSet<byte>();

        private GameObject body;
        private SpriteRenderer img;

        private float activePlayerNum;
        private float activeRange;
        private bool isActivate;

        public Camp(
            int activeNum,
            float activateRange,
            bool canSee,
            Vector2 pos)
        {
            this.body = new GameObject("MaryCamp");
            this.img = this.body.AddComponent<SpriteRenderer>();
            this.img.sprite = Loader.CreateSpriteFromResources(
               Path.MeryNoneActiveVent, 125f);

            this.body.gameObject.SetActive(canSee);
            this.body.transform.position = new Vector3(
                pos.x, pos.y, (pos.y / 1000f));

            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                    this.body.gameObject,
                    Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
            }

            this.activePlayerNum = activeNum;
            this.activeRange = activateRange;
            this.isActivate = false;
        }

        public void Update(int index)
        {

            if (this.isActivate) { return; }

            Vector2 pos = new Vector2(
                this.body.transform.position.x,
                this.body.transform.position.y);

            foreach (GameData.PlayerInfo playerInfo in 
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (playerInfo == null) { continue; }

                if (!playerInfo.Disconnected &&
                    !ExtremeRoleManager.GameRole[playerInfo.PlayerId].IsImpostor() &&
                    !playerInfo.IsDead &&
                    playerInfo.Object != null &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - pos;
                        float magnitude = vector.magnitude;
                        if (magnitude <= activeRange &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                pos, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            this.player.Add(@object.PlayerId);
                        }
                    }
                }
            }

            if (this.player.Count >= this.activePlayerNum)
            {
                this.isActivate = true;

                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.MeryAbility))
                {
                    caller.WriteByte((byte)MeryAbility.ActiveCamp);
                    caller.WriteInt(index);
                }
                activateVent(index);
            }
        }

        public Vent GetConvertedVent()
        {
            var referenceVent = Object.FindObjectOfType<Vent>();
            var vent = Object.Instantiate<Vent>(referenceVent);
            vent.transform.position = this.body.transform.position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.EnterVentAnim = null;
            vent.ExitVentAnim = null;
            vent.Offset = new Vector3(0f, 0.25f, 0f);

            var anim = vent.GetComponent<PowerTools.SpriteAnim>();
            if (anim)
            {
                anim.Stop();
            }
            vent.Id = CachedShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1;

                var console = vent.GetComponent<Console>();
                if (console is not null)
                {
                    Object.Destroy(console);
                }
                var ventRenderer = vent.GetComponent<SpriteRenderer>();
                ventRenderer.sprite = Loader.CreateSpriteFromResources(
                   string.Format(Path.MeryCustomVentAnime, "0"), 125f);
                vent.myRend = ventRenderer;           
                vent.name = "MaryVent_" + vent.Id;
                vent.gameObject.SetActive(this.body.gameObject.active);

            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                    vent.gameObject, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
            }

            return vent;
        }

        public void Clear()
        {
            Object.Destroy(this.img);
            Object.Destroy(this.body);
        }
    }


    public enum MeryOption
    {
        ActiveNum,
        ActiveRange
    }

    public enum MeryAbility : byte
    {
        SetCamp,
        ActiveCamp
    }

    public ExtremeAbilityButton Button
    {
        get => this.bombButton;
        set
        {
            this.bombButton = value;
        }
    }

    public int ActiveNum;
    public float ActiveRange;

    private ExtremeAbilityButton bombButton;

    public Mery() : base(
        ExtremeRoleId.Mery,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Mery.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public static void Ability(ref MessageReader reader)
    {
        MeryAbility abilityType = (MeryAbility)reader.ReadByte();

        switch (abilityType)
        {
            case MeryAbility.SetCamp:
                byte meryPlayerId = reader.ReadByte();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                setCamp(meryPlayerId, new Vector2(x, y));
                break;
            case MeryAbility.ActiveCamp:
                int activateVentIndex = reader.ReadInt32();
                activateVent(activateVentIndex);
                break;
            default:
                break;
        }
    }

    private static void setCamp(byte callerId, Vector2 setPos)
    {
        var mery = ExtremeRoleManager.GetSafeCastedRole<Mery>(callerId);
        if (mery == null) { return; }
        var localPlayerRole = ExtremeRoleManager.GetLocalPlayerRole();

        bool isMarlin = localPlayerRole.Id == ExtremeRoleId.Marlin;

        ExtremeRolesPlugin.ShipState.AddUpdateObject(
            new Camp(
                mery.ActiveNum,
                mery.ActiveRange,
                localPlayerRole.IsImpostor() || isMarlin,
                setPos));
    }

    private static void activateVent(
        int activateVentIndex)
    {
        Camp camp = (Camp)ExtremeRolesPlugin.ShipState.GetUpdateObject(
            activateVentIndex);
        ExtremeRolesPlugin.ShipState.RemoveUpdateObjectAt(activateVentIndex);

        Vent newVent = camp.GetConvertedVent();
        var meryVent = CachedShipStatus.Instance.GetCustomVent(
            VentExtension.CustomVentType.MeryVent);

        int ventNum = meryVent.Count;

        if (ventNum > 0)
        {
            var prevAddVent = meryVent[ventNum - 1];
            newVent.Left = prevAddVent;
            prevAddVent.Right = newVent;

            if (ventNum > 1)
            {
                meryVent[0].Left = newVent;
                newVent.Right = meryVent[0];
            }
            else
            {
                newVent.Right = null;
            }
        }
        else
        {
            newVent.Left = null;
            newVent.Right = null;
        }

        newVent.Center = null;

        CachedShipStatus.Instance.AddVent(
            newVent, VentExtension.CustomVentType.MeryVent);

        camp.Clear();
    }

    public void CreateAbility()
    {

        this.CreateAbilityCountButton(
            "setCamp",
            Loader.CreateSpriteFromResources(
                string.Format(Path.MeryCustomVentAnime, "0")));
    }

    public bool IsAbilityUse()
    {
        return this.IsCommonUse();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
        Vector2 setPos = localPlayer.GetTruePosition();

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.MeryAbility))
        {
            caller.WriteByte((byte)MeryAbility.SetCamp);
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteFloat(setPos.x);
            caller.WriteFloat(setPos.y);
        }
        setCamp(localPlayer.PlayerId, setPos);

        return true;
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        this.CreateAbilityCountOption(
            parentOps, 3, 5);

        CreateIntOption(
            MeryOption.ActiveNum,
            3, 1, 5, 1, parentOps);
        CreateFloatOption(
            MeryOption.ActiveRange,
            2.0f, 0.1f, 3.0f, 0.1f, parentOps);
    }

    protected override void RoleSpecificInit()
    {

        this.RoleAbilityInit();

        var allOption = OptionHolder.AllOption;

        this.ActiveNum = allOption[
            GetRoleOptionId(MeryOption.ActiveNum)].GetValue();
        this.ActiveRange = allOption[
            GetRoleOptionId(MeryOption.ActiveRange)].GetValue();

    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }
}

using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Cracker : SingleRoleBase, IRoleAbility
    {
        public sealed class CrackTrace : IMeetingResetObject
        {
            private SpriteRenderer image;
            private GameObject body;

            public CrackTrace(Vector3 pos)
            {
                this.body = new GameObject("CrackTrace");
                this.image = this.body.AddComponent<SpriteRenderer>();
                this.image.sprite = Loader.CreateSpriteFromResources(
                   Path.CrackerCrackTrace, 300f);

                this.body.transform.position = pos;

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                        this.body.gameObject,
                        Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
                }
            }

            public void Clear()
            {
                Object.Destroy(this.image);
                Object.Destroy(this.body);
            }
        }
        public enum CrackerOption
        {
            RemoveDeadBody,
            CanCrackDistance,
        }

        public bool IsRemoveDeadBody;
        private float crackDistance;
        private byte targetDeadBodyId;

        public RoleAbilityButtonBase Button
        {
            get => this.crackButton;
            set
            {
                this.crackButton = value;
            }
        }

        private RoleAbilityButtonBase crackButton;

        public Cracker() : base(
            ExtremeRoleId.Cracker,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Cracker.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public static void CrackDeadBody(
            byte rolePlayerId, byte targetPlayerId)
        {
            var role = ExtremeRoleManager.GetSafeCastedRole<Cracker>(rolePlayerId);
            if (role == null) { return; }

            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
                {

                    if (role.IsRemoveDeadBody)
                    {
                        ExtremeRolesPlugin.GameDataStore.AddMeetingResetObject(
                            new CrackTrace(array[i].gameObject.transform.position));
                        Object.Destroy(array[i].gameObject);
                    }
                    else
                    {
                        array[i].GetComponentInChildren<BoxCollider2D>().enabled = false;
                    }
                    break;
                }
            }
        }
        
        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("crack"),
                Loader.CreateSpriteFromResources(
                   Path.CrackerCrack));
        }

        public bool IsAbilityUse()
        {
            this.targetDeadBodyId = byte.MaxValue;
            GameData.PlayerInfo info = Player.GetDeadBodyInfo(
                this.crackDistance);
            
            if (info != null)
            {
                this.targetDeadBodyId = info.PlayerId;
            }

            return this.IsCommonUse() && this.targetDeadBodyId != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.CrackerCrackDeadBody,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    this.targetDeadBodyId
                });

            CrackDeadBody(
                CachedPlayerControl.LocalPlayer.PlayerId,
                this.targetDeadBodyId);
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 2, 5);

            CreateFloatOption(
                CrackerOption.CanCrackDistance,
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);

            CreateBoolOption(
                CrackerOption.RemoveDeadBody,
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.crackDistance = OptionHolder.AllOption[
                GetRoleOptionId(CrackerOption.CanCrackDistance)].GetValue();
            this.IsRemoveDeadBody = OptionHolder.AllOption[
                GetRoleOptionId(CrackerOption.RemoveDeadBody)].GetValue();
            this.RoleAbilityInit();
        }
    }
}

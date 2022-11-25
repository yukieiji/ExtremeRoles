using System;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.Combination;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister(
       new Type[]
       {
            typeof(IUsable)
       })]
    public sealed class TorchBehavior : MonoBehaviour
    {
        public ImageNames UseIcon
        {
            get
            {
                return ImageNames.UseButton;
            }
        }

        public float UsableDistance
        {
            get
            {
                return this.distance;
            }
        }

        public float PercentCool
        {
            get
            {
                return 0.1f;
            }
        }

        private float distance = 1.3f;

        private CircleCollider2D collider;
        private SpriteRenderer img;

        public void Awake()
        {
            this.collider = base.gameObject.AddComponent<CircleCollider2D>();
            this.img = base.gameObject.AddComponent<SpriteRenderer>();
            this.img.sprite = Loader.CreateSpriteFromResources(
                Path.WispTorch);

            this.collider.radius = 0.01f;
        }

        public float CanUse(
            GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
        {
            float num = Vector2.Distance(
                pc.Object.GetTruePosition(),
                base.transform.position);
            couldUse = !pc.IsDead && !Wisp.HasTorch(pc.PlayerId);
            canUse = (couldUse && num <= this.UsableDistance);
            return num;
        }

        public void SetOutline(bool on, bool mainTarget)
        { }

        public void Use()
        {
            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.KidsAbility))
            {
                caller.WriteByte((byte)Kids.AbilityType.PickUpTorch);
                caller.WriteByte(playerId);
            }
            Wisp.PickUpTorch(playerId);
        }
        public void SetRange(float range)
        {
            this.distance = range;
        }
    }
}

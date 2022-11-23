using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles;
using System;
using UnityEngine;
using ExtremeRoles.Resources;

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
                return 0f;
            }
        }

        private float maxTime = 100f;
        private float timer = 100f;
        private float distance = 1.3f;
        private bool isAddOffsets = false;

        private CircleCollider2D collider;
        private SpriteRenderer img;

        public void Awake()
        {
            this.collider = base.gameObject.AddComponent<CircleCollider2D>();
            this.img = base.gameObject.AddComponent<SpriteRenderer>();
            this.img.sprite = Loader.CreateSpriteFromResources(
                Path.TestButton);

            this.collider.radius = 0.001f;
        }

        public void OnDestroy()
        {

        }

        public float CanUse(
            GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
        {
            float num = Vector2.Distance(
                pc.Object.GetTruePosition(),
                base.transform.position);
            couldUse = pc.IsDead ? false : true;
            canUse = (couldUse && num <= this.UsableDistance);
            return num;
        }

        public void SetOutline(bool on, bool mainTarget)
        { }

        public void Use()
        {
              
        }

        public void FixedUpdate()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            float distance = Vector2.Distance(
                localPlayer.GetTruePosition(),
                base.transform.position);
        }
    }
}

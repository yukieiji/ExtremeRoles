using UnityEngine;

namespace ExtremeRoles.Module
{
    public class FakeDeadBody
    {
        private SpriteRenderer body;
        public FakeDeadBody(
            PlayerControl rolePlayer,
            PlayerControl targetPlayer)
        {
            var killAnimation = PlayerControl.LocalPlayer.KillAnimations[0];
            this.body = Object.Instantiate(
                killAnimation.bodyPrefab.bodyRenderer);
            targetPlayer.SetPlayerMaterialColors(this.body);

            Vector3 vector = rolePlayer.transform.position + killAnimation.BodyOffset;
            vector.z = vector.y / 1000f;
            this.body.transform.position = vector;
            this.body.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        }

        public void Clear()
        {
            Object.Destroy(this.body);
        }

    }
}

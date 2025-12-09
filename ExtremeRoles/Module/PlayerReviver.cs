using System;
using UnityEngine;
using TMPro;

namespace ExtremeRoles.Module
{
    public sealed class PlayerReviver
    {
        private InnerToken? token;
        private TextMeshPro? resurrectText;

        public bool IsReviving => token != null;

        public void Start(float resurrectTime, Action onRevive)
        {
            if (resurrectText == null)
            {
                resurrectText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                resurrectText.enableWordWrapping = false;
            }

            token = new InnerToken(resurrectTime, resurrectText, onRevive, () => token = null);
        }

        public void Update()
        {
            token?.Update();
        }

        public void Reset()
        {
            token?.Reset();
        }
    }
}

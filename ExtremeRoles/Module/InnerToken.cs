using System;
using UnityEngine;
using TMPro;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module
{
    public sealed class InnerToken
    {
        private float resurrectTimer;
        private readonly TextMeshPro resurrectText;
        private readonly Action onRevive;
        private readonly Action onDispose;

        public InnerToken(float resurrectTime, TextMeshPro resurrectText, Action onRevive, Action onDispose)
        {
            this.resurrectTimer = resurrectTime;
            this.resurrectText = resurrectText;
            this.onRevive = onRevive;
            this.onDispose = onDispose;
        }

        public void Update()
        {
            if (resurrectTimer <= 0.0f) return;

            resurrectText.gameObject.SetActive(true);
            resurrectTimer -= Time.deltaTime;
            resurrectText.text = string.Format(
                Tr.GetString("resurrectText"),
                Mathf.CeilToInt(resurrectTimer));

            if (resurrectTimer <= 0.0f)
            {
                onRevive?.Invoke();
                onDispose?.Invoke();
            }
        }

        public void Reset()
        {
            if (resurrectText != null)
            {
                resurrectText.gameObject.SetActive(false);
            }
        }
    }
}

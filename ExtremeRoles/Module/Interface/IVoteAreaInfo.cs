using UnityEngine;
using TMPro;

namespace ExtremeRoles.Module.Interface
{
    public interface IVoteAreaInfo
    {
        public void Init(PlayerVoteArea pva, bool commActive);

        public static void InitializeText(TextMeshPro text)
        {
            text.transform.localPosition += Vector3.down * 0.20f + Vector3.left * 0.30f;
            text.fontSize *= 0.63f;
            text.autoSizeTextContainer = false;
            text.gameObject.name = "VoteAreaInfo";
        }
    }
}

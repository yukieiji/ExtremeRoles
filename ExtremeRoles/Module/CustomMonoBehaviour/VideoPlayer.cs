using System;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class DlayableVideoPlayer : MonoBehaviour
{
    private float delayTimer = 0.0f;
    private bool isPlayed = false;
    
    public DlayableVideoPlayer(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {

    }

    public void SetVideoPath(string path)
    {

    }

    public void SetTimer(float time)
    {
        this.delayTimer = time;
        checkAndPlay();
    }

    public void FixedUpdate()
    {
        if (this.isPlayed ||
            MeetingHud.Instance ||
            ExileController.Instance) { return; }

        this.delayTimer -= Time.fixedTime;

        checkAndPlay();
    }
    private void checkAndPlay()
    {
        if (this.delayTimer > 0.0f) { return; }
        
    }
}

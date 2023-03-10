using System;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class DlayableVideoPlayer : MonoBehaviour
{
    private float delayTimer = 0.0f;
    private bool isPlayed = false;

    private SpriteRenderer thumbnail;
    private VideoPlayer player;

    public DlayableVideoPlayer(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        this.thumbnail = this.gameObject.AddComponent<SpriteRenderer>();
        this.player = this.gameObject.AddComponent<VideoPlayer>();

        this.player.source = VideoSource.Url;
        this.player.renderMode = VideoRenderMode.MaterialOverride;
        this.player.isLooping = true;
    }

    public void SetVideoPath(string path)
    {
        this.player.url = path;
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
        this.player.Play();
    }
}

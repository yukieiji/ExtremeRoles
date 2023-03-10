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
        this.isPlayed = false;
        this.thumbnail = this.gameObject.AddComponent<SpriteRenderer>();
        this.player = this.gameObject.AddComponent<VideoPlayer>();

        this.player.renderMode = VideoRenderMode.MaterialOverride;
        this.player.isLooping = true;
    }

    public void SetThum(Sprite sprite)
    {
        this.thumbnail.sprite = sprite;
        if (!this.isPlayed)
        {
            this.thumbnail.enabled = false;
        }
    }

    public void SetVideo(string path)
    {
        this.player.source = VideoSource.Url;
        this.player.url = path;
    }

    public void SetVideo(VideoClip video)
    {
        this.player.source = VideoSource.VideoClip;
        this.player.clip = video;
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

        this.delayTimer -= Time.fixedDeltaTime;

        checkAndPlay();
    }

    private void checkAndPlay()
    {
        if (this.delayTimer > 0.0f) { return; }

        this.player.Play();
        this.isPlayed = true;
        this.thumbnail.enabled = true;
    }
}

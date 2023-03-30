using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class VentInPlayerPosSyncer : MonoBehaviour
{
    private float timer = 0.0f;

    private Vent vent;
    private VentilationSystem ventilationSystem;
    private PlayerControl localPlayer;

    public VentInPlayerPosSyncer(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        this.vent = base.gameObject.GetComponent<Vent>();
        this.localPlayer = CachedPlayerControl.LocalPlayer;
        setSystem();
    }

    public void FixedUpdate()
    {
        this.timer += Time.fixedDeltaTime;

        if (this.timer < 0.15f ||
            AmongUsClient.Instance.IsGameOver ||
            !this.ventilationSystem.PlayersCleaningVents.TryGetValue(
                this.localPlayer.PlayerId, out byte ventId) ||
            this.vent.Id != ventId) { return; }

        this.timer = 0.0f;

        Vector2 pos = this.vent.transform.position;
        pos -= this.localPlayer.Collider.offset;
        this.localPlayer.transform.position = pos;

        var camera = FastDestroyableSingleton<HudManager>.Instance.PlayerCam;
        camera.transform.position = pos + camera.Offset;

        this.vent.SetButtons(true);
    }

    private void setSystem()
    {
        if (!CachedShipStatus.Instance.Systems.TryGetValue(
                SystemTypes.Ventilation, out ISystemType systemType))
        {
            return;
        }
        this.ventilationSystem = systemType.Cast<VentilationSystem>();
    }
}

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

        if (this.timer < 0.1f ||
            AmongUsClient.Instance.IsGameOver ||
            !this.ventilationSystem.PlayersCleaningVents.TryGetValue(
                this.localPlayer.PlayerId, out byte ventId) ||
            this.vent.Id != ventId) { return; }

        Vector2 vector = this.vent.transform.position;
        vector -= this.localPlayer.Collider.offset;

        this.localPlayer.transform.position = vector;
        FastDestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
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

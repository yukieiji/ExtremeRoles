using UnityEngine;

using System;

namespace ExtremeRoles.Module.CustomMonoBehaviour;


[Il2CppRegister]
public sealed class ArrowControler : MonoBehaviour
{
    private GameObject targetObj;
    private Arrow arrow;

    private float activeTimer = 0.0f;
    private bool isActiveActiveTimer = false;

    private float hideTimer = 0.0f;
    private bool isActiveHideTimer = false;

    public ArrowControler(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        this.arrow = new Arrow(Color.white);
        this.activeTimer = 0.0f;
        this.isActiveActiveTimer = false;

        this.hideTimer = 0.0f;
        this.isActiveHideTimer = false;
    }

    public void OnEnable()
    {
        this.arrow.SetActive(true);
    }

    public void OnDisable()
    {
        this.arrow.SetActive(false);
    }

    public void Hide()
    {
        this.activeTimer = 0.0f;
        this.isActiveActiveTimer = false;

        this.hideTimer = 0.0f;
        this.isActiveHideTimer = false;
        base.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        this.arrow.Clear();
    }

    public void SetColor(Color color)
    {
        this.arrow.SetColor(color);
    }

    public void SetTarget(GameObject obj)
    {
        this.targetObj = obj;
    }
    public void SetTarget(Vector3 pos)
    {
        this.arrow.UpdateTarget(pos);
    }

    public void SetHideTimer(float timer)
    {
        this.hideTimer = timer;
        this.isActiveHideTimer = true;
    }

    public void SetDelayActiveTimer(float timer)
    {
        this.activeTimer = timer;
        this.isActiveActiveTimer = true;
    }

    public void Update()
    {
        if (this.isActiveActiveTimer)
        {
            this.activeTimer -= Time.deltaTime;
            if (this.activeTimer > 0.0f)
            {
                this.arrow.SetActive(false);
                return;
            }
            else
            {
                this.isActiveActiveTimer = false;
                this.arrow.SetActive(true);
            }
        }

        if (this.isActiveHideTimer)
        {
            this.hideTimer -= Time.deltaTime;
            if (this.hideTimer <= 0.0f)
            {
                this.Hide();
                return;
            }
        }

        if (this.targetObj)
        {
            this.arrow.UpdateTarget(this.targetObj.transform.position);
        }
        this.arrow.Update();
    }
}
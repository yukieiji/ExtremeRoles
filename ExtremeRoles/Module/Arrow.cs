using UnityEngine;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class Arrow
{
	public GameObject Main
	{
		get => this.arrowBehaviour.gameObject;
	}

	public Vector3 Target { get; private set; }

	private readonly ArrowBehaviour arrowBehaviour;
	private static readonly Vector3 defaultPos = new Vector3(100.0f, 100.0f, 100.0f);
	private static ArrowBehaviour? arrow = null;

	public Arrow(Color color)
	{
		if (arrow == null)
		{
			arrow = GameSystem.GetArrowTemplate();
		}

		this.arrowBehaviour = Object.Instantiate(arrow);
		this.arrowBehaviour.gameObject.SetActive(true);
		this.arrowBehaviour.image.color = color;
		this.arrowBehaviour.MaxScale = 0.75f;

		this.Target = defaultPos;
	}

	public void Update()
	{
		if (this.Target == defaultPos)
		{
			this.Target = Vector3.zero;
		}
		UpdateTarget();
	}

	public void SetColor(Color? color = null)
	{
		if (color.HasValue)
		{
			this.arrowBehaviour.image.color = color.Value;
		}
	}

	public void UpdateTarget(Vector3? target = null)
	{
		if (this.arrowBehaviour == null) { return; }

		if (target.HasValue)
		{
			this.Target = target.Value;
		}

		this.arrowBehaviour.target = this.Target;
	}

	public void Clear()
	{
		if (this.arrowBehaviour == null) { return; }

		this.arrowBehaviour.SetImageEnabled(false);
		Object.Destroy(
			this.arrowBehaviour.image);
		Object.Destroy(
			this.arrowBehaviour.gameObject);
	}
	public void SetActive(bool active)
	{
		if (this.arrowBehaviour == null) { return; }

		this.arrowBehaviour.gameObject.SetActive(active);
	}
}

using System;
using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.Ability;

#nullable enable

public class ExtremeMultiModalAbilityButton : ExtremeAbilityButton
{
	public int MultiModalAbilityNum => allAbility.Count;
	private readonly SpriteRenderer multiAbilityImg;
	private readonly List<BehaviorBase> allAbility;
	private int curIndex = 0;
	private float blockTimer = 0.0f;

	public ExtremeMultiModalAbilityButton(
		List<BehaviorBase> behaviorors,
		IButtonAutoActivator activator,
		KeyCode hotKey) : base(
			behaviorors[0],
			activator,
			hotKey)
	{
		this.allAbility = behaviorors.ToList();
		if (this.MultiModalAbilityNum > 1)
		{
			foreach (BehaviorBase behavior in this.allAbility)
			{
				if (behavior == this.Behavior)
				{
					continue;
				}
				behavior.Initialize(this.Button);
				_ = behavior.Update(AbilityState.None);
			}
		}

		var obj = new GameObject("MultiAbilityImg");
		obj.transform.SetParent(this.Transform);
		obj.transform.position = this.Transform.position;
		obj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.25f);
		this.multiAbilityImg = obj.AddComponent<SpriteRenderer>();
			this.multiAbilityImg.name = "MultiAbilityImg";
		this.multiAbilityImg.sprite = UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(ObjectPath.CommonImagePathFormat, "MultiAbility"));
		this.multiAbilityImg.enabled = this.MultiModalAbilityNum > 1;
	}

	public ExtremeMultiModalAbilityButton(
		IButtonAutoActivator activator,
		KeyCode hotKey,
		params BehaviorBase[] behaviorors) : this(
			behaviorors.ToList(),
			activator,
			hotKey)
	{ }

	public void Add(BehaviorBase behavior, bool withUpdate = true)
	{
		if (withUpdate)
		{
			_ = behavior.Update(AbilityState.None);
		}

		behavior.Initialize(this.Button);

		this.allAbility.Add(behavior);
		this.reenableImg();
	}
	public void Remove(int index)
	{
		var targetAbility = this.allAbility[index];
		checkForRemove(targetAbility);
		this.allAbility.RemoveAt(index);
		this.reenableImg();
	}
	public void Remove(in BehaviorBase behavior)
	{
		checkForRemove(behavior);
		this.allAbility.Remove(behavior);
		this.reenableImg();
	}

	public void ClearAndAnd(BehaviorBase behavior)
	{
		this.allAbility.RemoveAll(x => x != this.Behavior);
		Add(behavior, false);
		var curAbility = this.Behavior;
		switchAbility(false);
		Remove(curAbility);
		this.OnMeetingEnd();
		this.reenableImg();
	}

	protected override void UpdateImp()
	{
		if (this.blockTimer > 0.0f)
		{
			this.blockTimer -= Time.fixedDeltaTime;
		}

		if (this.MultiModalAbilityNum > 1 &&
			this.State is not AbilityState.Activating or AbilityState.Charging &&
			this.blockTimer <= 0.0f)
		{
			float delta = Input.mouseScrollDelta.y;
			if (delta <= -0.5f || Input.GetKeyDown(KeyCode.Mouse3))
			{
				switchAbility(false);
				this.blockTimer = 0.75f;
				return;
			}
			else if (delta >= 0.5f || Input.GetKeyDown(KeyCode.Mouse4))
			{
				switchAbility(true);
				this.blockTimer = 0.75f;
				return;
			}
		}
		base.UpdateImp();
	}

	private void checkForRemove(in BehaviorBase removeBehaviour)
	{
		if (this.allAbility.Count == 1)
		{
			throw new IndexOutOfRangeException("Can't remove only ability!!");
		}

		if (removeBehaviour != this.Behavior)
		{
			return;
		}
		switchAbility(false);
	}

	private void switchAbility(bool isInvert)
	{
		int curAbilityNum = this.MultiModalAbilityNum;
		int rowIndex = isInvert ? this.curIndex - 1 : this.curIndex + 1;
		int newIndex = (rowIndex + curAbilityNum) % curAbilityNum;

		float curMaxCoolTime = this.Behavior.CoolTime;
		if (this.Behavior is IHideLogic hideLogic)
		{
			hideLogic.Hide();
		}
		this.Behavior = this.allAbility[newIndex];
		if (this.Behavior is IHideLogic hideLogic2)
		{
			hideLogic2.Show();
		}
		this.curIndex = newIndex;

		ExtremeRolesPlugin.Logger.LogInfo($"Switched to {this.Behavior.GetType().Name}, Index:{newIndex}");

		// クールタイム詐称を防ぐ(CT長いの使う => 短いのに切り替え => CTがカットされる => 長いのに切り替えて詐称)
		// 短いのから長いのに切り替えた瞬間にクールタイムが発生するようにする
		float diff = this.Behavior.CoolTime - curMaxCoolTime;

		// 能力使い切ってるとかでクールタイムになるように
		if (this.State is AbilityState.None or AbilityState.Stop)
		{
			this.OnMeetingEnd();
		}

		if (diff > 0.0f)
		{
			this.AddTimerOffset(diff);
		}
	}
	private void reenableImg()
	{
		this.multiAbilityImg.enabled = this.MultiModalAbilityNum > 1;
	}
}

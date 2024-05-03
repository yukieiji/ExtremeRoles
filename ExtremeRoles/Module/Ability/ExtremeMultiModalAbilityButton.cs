using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Helper;
using System;

namespace ExtremeRoles.Module.Ability;

#nullable enable

public class ExtremeMultiModalAbilityButton : ExtremeAbilityButton
{
	public int MultiModalAbilityNum => allAbility.Count;
	private readonly List<BehaviorBase> allAbility;

	public ExtremeMultiModalAbilityButton(
		IButtonAutoActivator activator,
		KeyCode hotKey,
		params BehaviorBase[] behaviorors) : base(
			behaviorors[0],
			activator,
			hotKey)
	{
		this.allAbility = behaviorors.ToList();
	}

	public void Add(BehaviorBase behavior)
	{
		this.allAbility.Add(behavior);
	}
	public void Remove(int index)
	{
		var targetAbility = this.allAbility[index];
		checkForRemove(targetAbility);
		this.allAbility.RemoveAt(index);
	}
	public void Remove(in BehaviorBase behavior)
	{
		checkForRemove(behavior);
		this.allAbility.Remove(behavior);
	}

	protected override void UpdateImp()
	{
		if (this.State is not AbilityState.Activating &&
			Input.GetKeyDown(KeyCode.Tab))
		{
			bool isShiftDown = Key.IsShiftDown();
			switchAbility(isShiftDown);
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
		int rowIndex = isInvert ? curAbilityNum - 1 : curAbilityNum + 1;
		int newIndex = (rowIndex + curAbilityNum) % curAbilityNum;

		this.Behavior = this.allAbility[newIndex];
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrackerButtonPreview : TrackerButton
{
	public SDispatcher<bool> Invalid;
	public SDispatcher<bool> NonInteractable;
	public SDispatcher<bool> SomethingReduced;
	public SDispatcher<string> Name;

	protected override void PreformDelayedUpdate()
	{
		bool active = actions.Count > 0;
		bool interactable = true;

		for (int i = 0; i < actions.Count; i++)
		{
			TrackerAction action = actions[i];
			active &= !action.Dispatcher.Invalid;
			interactable &= !action.Dispatcher.Disabled;
		}

		Invalid.Value = !active;
		NonInteractable.Value = !interactable;

		UnityEngine.Debug.Log("PreformDelayedUpdate: " + actions.Count + " " + active + " " + interactable);

		bool reduced = false;
		if (active)
		{
			reducingList.Clear();
			for (int i = 0; i < actions.Count; i++)
				reducingList.AddLast(new ReducingActionEntry(i, actions[i].NetworkData));

			for (int i = 0; i < actions.Count; i++)
			{
				TrackerAction action = actions[i];

				if (reducingList.Count > 0 && i == reducingList.First.Value.index)
				{
					action.Dispatcher.Reduced.Value = false;
					reducingList.RemoveFirst();
					action.ApplySolo(reducingList);
				}
				else {
					action.Dispatcher.Reduced.Value = true;
					reduced = true;
				}
			}
#if DEBUG
			if (reducingList.Count > 0)
				UnityEngine.Debug.LogError("ReducingList not empty after loop.");
#endif
		}
		SomethingReduced.Value = reduced;
	}
}
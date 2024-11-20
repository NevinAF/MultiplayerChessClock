using System;
using System.Collections.Generic;
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
		bool active = false;
		bool interactable = true;

		for (int i = 0; i < actions.Values.Count; i++)
		{
			TrackerAction action = actions.Values[i];
			active |= !action.Dispatcher.Invalid;
			interactable &= !action.Dispatcher.Disabled;
		}

		Invalid.Value = !active;
		NonInteractable.Value = !interactable;

		UnityEngine.Debug.Log("PreformDelayedUpdate: " + actions.Count + " " + active + " " + interactable);

		bool reduced = false;
		if (active)
		{
			reducingList.Clear();
			reducingList.AddRange(actions.Keys);

			for (int i = 0; i < actions.Count; i++)
			{
				int key = actions.Keys[i];
				TrackerAction action = actions.Values[i];

				if (reducingList.Count > 0 && key == reducingList[0])
				{
					action.Dispatcher.Reduced.Value = false;
					reducingList.RemoveAt(0);
					action.ApplySolo(reducingList);
				}
				else {
					action.Dispatcher.Reduced.Value = true;
					reduced = true;
				}
			}
		}
		SomethingReduced.Value = reduced;
	}
}
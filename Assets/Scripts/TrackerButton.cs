using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrackerButton : MonoBehaviour
{
	protected static readonly List<int> reducingList = new List<int>();

	public readonly SortedList<int, TrackerAction> actions = new SortedList<int, TrackerAction>();

	public Transform ActionsParent;
	public Button Button;

	public TrackerAction CreateAction(int key, TrackerActionNetworkData data)
	{
		var action = TrackerManager.CreateAction(data, ActionsParent);
		actions.Add(key, action);
		action.Dispatcher.Target.Valid.Attach(PreformDelayedUpdate);
		action.Dispatcher.Disabled.Attach(PreformDelayedUpdate);
		return action;
	}

	public void DestroyAction(int key)
	{
		var action = actions[key];
		action.Dispatcher.Target.Valid.Detach(PreformDelayedUpdate);
		action.Dispatcher.Disabled.Detach(PreformDelayedUpdate);
		Destroy(action.gameObject);
		actions.Remove(key);
	}

	public void ClearActions()
	{
		foreach (var action in actions.Values)
		{
			action.Dispatcher.Target.Valid.Detach(PreformDelayedUpdate);
			action.Dispatcher.Disabled.Detach(PreformDelayedUpdate);
			Destroy(action.gameObject);
		}
		actions.Clear();
	}

	public void OnClick(UnityAction action)
	{
		Button.onClick.AddListener(action);
	}

	private void PreformDelayedUpdate(bool _) => PreformDelayedUpdate();

	protected virtual void PreformDelayedUpdate()
	{
		bool active = false;
		bool interactable = true;

		for (int i = 0; i < actions.Values.Count; i++)
		{
			TrackerAction action = actions.Values[i];
			active |= !action.Dispatcher.Invalid;
			interactable &= !action.Dispatcher.Disabled;
		}

		ActionsParent.gameObject.SetActive(active);
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
					action.gameObject.SetActive(true);
					reducingList.RemoveAt(0);
					action.ApplyReduceWith(reducingList);
				}
				else action.gameObject.SetActive(false);
			}

			Button.interactable = interactable;
		}
	}
}
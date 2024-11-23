using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public readonly struct ReducingActionEntry
{
	public readonly int index;
	public readonly TrackerActionNetworkData data;

	public ReducingActionEntry(int index, in TrackerActionNetworkData data)
	{
		this.index = index;
		this.data = data;
	}
}

public class TrackerButton : MonoBehaviour
{
	protected static readonly LinkedList<ReducingActionEntry> reducingList = new LinkedList<ReducingActionEntry>();

	public readonly List<TrackerAction> actions = new List<TrackerAction>();

	public Transform ActionsParent;
	public Button Button;

	public virtual TrackerAction CreateAction(int key, in TrackerActionNetworkData data)
	{
		int siblingIndex = LobbyNetworkManager.GetActionSiblingIndex(key, data);

#if DEBUG
		if (siblingIndex < 0 || siblingIndex > actions.Count)
		{
			Debug.LogError("Sibling index is out of range for the existing actions! It should be between 0 and " + actions.Count + " but was " + siblingIndex);
			return null;
		}
#endif

		var action = TrackerManager.CreateAction(data, ActionsParent);
		action.transform.SetSiblingIndex(siblingIndex + 1);
		action.Dispatcher.Invalid.OnChange.AddListener(PreformDelayedUpdate);
		action.Dispatcher.Disabled.OnChange.AddListener(PreformDelayedUpdate);

		actions.Insert(siblingIndex, action);

		PreformDelayedUpdate();


		// LayoutRebuilder.ForceRebuildLayoutImmediate(ActionsParent.parent as RectTransform);

		return action;
	}

	public void DestroyAction(int key, in TrackerActionNetworkData data)
	{
		int siblingIndex = LobbyNetworkManager.GetActionSiblingIndex(key, data);

#if DEBUG
		if (siblingIndex < 0 || siblingIndex >= actions.Count)
		{
			Debug.LogError("Sibling index did not result in an existing action!");
			return;
		}
#endif

		TrackerAction action = actions[siblingIndex];

#if DEBUG
		if (!data.Equals(action.NetworkData))
			Debug.LogError("NetworkData does not match the action to be destroyed.");
#endif

		Destroy(action.gameObject);

		actions.RemoveAt(siblingIndex);

		PreformDelayedUpdate();

		// LayoutRebuilder.ForceRebuildLayoutImmediate(ActionsParent.parent as RectTransform);
	}

	public void ClearActions()
	{
		for (int i = 0; i < actions.Count; i++)
		{
			TrackerAction action = actions[i];
			Destroy(action.gameObject);
		}
		actions.Clear();
	}

	public void OnClick(UnityAction action)
	{
		Button.onClick.RemoveAllListeners();
		Button.onClick.AddListener(action);
	}

	private void PreformDelayedUpdate(bool _) => PreformDelayedUpdate();

	protected virtual void PreformDelayedUpdate()
	{
		bool active = actions.Count > 0;
		bool interactable = true;

		for (int i = 0; i < actions.Count; i++)
		{
			TrackerAction action = actions[i];
			active &= !action.Dispatcher.Invalid;
			interactable &= !action.Dispatcher.Disabled;
		}

		ActionsParent.gameObject.SetActive(active);
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
					action.gameObject.SetActive(true);
					reducingList.RemoveFirst();
					action.ApplyReduceWith(reducingList);
				}
				else action.gameObject.SetActive(false);
			}
#if DEBUG
			if (reducingList.Count > 0)
				UnityEngine.Debug.LogError("ReducingList not empty after loop.");
#endif
			Button.interactable = interactable;
		}
	}
}
using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TrackerManager : MonoBehaviour
{
	public static TrackerManager m_instance;
	public static TrackerManager Instance
	{
		get
		{
#if UNITY_EDITOR
			if (!Application.isPlaying && m_instance == null)
				m_instance = FindObjectOfType<TrackerManager>();
#endif
			if (m_instance == null)
				Debug.LogError("TrackerManager not available in the current scene.");
			return m_instance;
		}
	}

	public readonly List<TrackerPresenter> trackers = new List<TrackerPresenter>();

	public SDispatcher<int> ValidTrackerCount = new SDispatcher<int>();

	public TrackerPresenter Prefab;
	public List<TrackerAction> ActionPrefabs = new List<TrackerAction>();
	public TrackerButton ButtonPrefab;

	private void Awake()
	{
		m_instance = this;
		LobbyNetworkManager.OnTrackersChange += OnNetworkTrackerChange;
		LobbyNetworkManager.OnActionsChange += OnNetworkActionChange;
	}

	public static TrackerPresenter GetTracker(byte networkId)
	{
		if (networkId > byte.MaxValue)
		{
			Debug.LogError("TrackerNetworkData is outside of the maximum number of trackers: " + networkId + " > " + byte.MaxValue);
			return null;
		}

		var trackers = Instance.trackers;
		while (trackers.Count <= networkId)
		{
			TrackerPresenter tracker = Instantiate(Instance.Prefab, Instance.transform);
			tracker.NetworkID = (byte)trackers.Count;
			tracker.UpdateFromNetwork();
			trackers.Add(tracker);
		}

		return trackers[networkId];
	}

	public static TrackerButton GetActionButton(byte trackerId, byte buttonIndex)
	{
		var tracker = GetTracker(trackerId);

		while (tracker.buttons.Count <= buttonIndex)
		{
			TrackerButton button = Instantiate(Instance.ButtonPrefab, tracker.DefaultButtonGroup);
			button.OnClick(() => LobbyNetworkManager.Cmd_ButtonPressed(trackerId, buttonIndex));
			tracker.buttons.Add(button);
		}
		return tracker.buttons[buttonIndex];
	}

	public static TrackerAction CreateAction(int networkId, Transform parent) =>
		CreateAction(LobbyNetworkManager.GetTrackerActionData(networkId), parent);

	public static TrackerAction CreateAction(TrackerActionNetworkData data, Transform parent)
	{
		if (data.type == 0)
		{
			Debug.LogError("Action type 0 is reserved for invalid and uninitialized action. Cannot create an action with this type.");
			return null;
		}

		if (data.type > Instance.ActionPrefabs.Count)
		{
			Debug.LogError("Action type out of range and does not exist: " + data.type + " > " + Instance.ActionPrefabs.Count);
			return null;
		}

		TrackerAction action = Instantiate(Instance.ActionPrefabs[data.type - 1], parent);
		action.Dispatcher.SetNetworkData(data);
		return action;
	}

	public static void OnNetworkTrackerChange(SyncList<TrackerNetworkData>.Operation op, int index, TrackerNetworkData data)
	{
		if (op == SyncList<TrackerNetworkData>.Operation.OP_CLEAR)
		{
			// For all trackers that exceed the count in the network, make sure they are invalidated.
			for (int i = LobbyNetworkManager.TrackerCount; i < Instance.trackers.Count; i++)
				Instance.trackers[index].UpdateFromNetwork();

			Instance.ValidTrackerCount.Value = 0;
			return;
		}

		if (op == SyncList<TrackerNetworkData>.Operation.OP_SET)
		{
			bool newValid = LobbyNetworkManager.GetTrackerData(index).Valid;
			if (newValid != data.Valid)
				Instance.ValidTrackerCount.Value += newValid ? 1 : -1;
		}
		else if (op == SyncList<TrackerNetworkData>.Operation.OP_ADD)
		{
			if (data.Valid)
				Instance.ValidTrackerCount.Value++;
		}
		else throw new InvalidOperationException("Invalid operation: " + op);

		if (Instance.trackers.Count <= index)
			GetTracker((byte)index); // Creates missing trackers.
		else Instance.trackers[index].UpdateFromNetwork();
	}

	public static void OnNetworkActionChange(SyncList<TrackerActionNetworkData>.Operation op, int index, TrackerActionNetworkData data)
	{
		if (op == SyncList<TrackerActionNetworkData>.Operation.OP_CLEAR)
		{
			for (int t = 0; t < Instance.trackers.Count; t++)
			{
				var tracker = Instance.trackers[t];
				for (int i = 0; i < tracker.buttons.Count; i++)
					tracker.buttons[i].ClearActions();
			}
			return;
		}

		var button = GetActionButton(data.attachedTracker, data.buttonIndex);

		switch (op)
		{
			case SyncList<TrackerActionNetworkData>.Operation.OP_ADD:
			case SyncList<TrackerActionNetworkData>.Operation.OP_INSERT:
			{
				button.CreateAction(index, data);
				break;
			}
			case SyncList<TrackerActionNetworkData>.Operation.OP_REMOVEAT:
			{
				button.DestroyAction(index);
				break;
			}
			case SyncList<TrackerActionNetworkData>.Operation.OP_SET:
			{
				button.DestroyAction(index);
				goto case SyncList<TrackerActionNetworkData>.Operation.OP_ADD;
			}
		}
	}

	public static void Server_PreformAction(byte actionType, byte target, ushort data)
	{
		if (actionType == 0)
		{
			Debug.LogError("Action type 0 is reserved for invalid and uninitialized action. Cannot preform a server action with this type.");
			return;
		}

		if (actionType > Instance.ActionPrefabs.Count)
		{
			Debug.LogError("Action type out of range and does not exist: " + actionType + " > " + Instance.ActionPrefabs.Count);
			return;
		}

		var trackerNetworkData = LobbyNetworkManager.GetTrackerData(target);
		Instance.ActionPrefabs[actionType - 1].Server_PreformActionPrefab(ref trackerNetworkData, data);
		LobbyNetworkManager.Server_SetTracker(target, trackerNetworkData);
	}
}
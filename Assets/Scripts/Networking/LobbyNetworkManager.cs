using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System;
using UnityEngine.Events;

public class LobbyNetworkManager : NetworkBehaviour
{
#region Singleton Template

	protected static LobbyNetworkManager m_instance;
	public static LobbyNetworkManager Instance
	{
		get
		{
			if (m_instance == null)
				m_instance = (NetworkManager.singleton as CustomNetworkManager).lobbyNetworkManager;
			return m_instance;
		}
	}
#endregion


	public static TrackerNetworkData GetTrackerData(int index)
	{
		if (index < 0 || index >= Instance.trackers.Count)
			return default;
		return Instance.trackers[index];
	}

	public static void Server_SetTracker(int index, in TrackerNetworkData data)
	{
		if (index < 0 || index >= Instance.trackers.Count)
		{
			Debug.LogError("TrackerNetworkData index out of range " + Instance.trackers.Count + ": " + index + ". This should never happen.");
			return;
		}
		UnityEngine.Debug.Log("Setting tracker " + index + ":\n\tFrom: " + Instance.trackers[index] + "\n\tTo: " + data);

		Instance.trackers[index] = data;
	}

	public static TrackerActionNetworkData GetTrackerActionData(int index)
	{
		if (index < 0 || index >= Instance.actions.Count)
		{
			Debug.LogError("TrackerActionNetworkData index out of range " + Instance.actions.Count + ": " + index + ". This should never happen.");
			return default;
		}
		return Instance.actions[index];
	}

	public static int GetActionSiblingIndex(int actionIndex, in TrackerActionNetworkData data)
	{
		TrackerActionNetworkData other;

		int index = 0;
		int count = Mathf.Min(Instance.actions.Count, actionIndex);
		for (int i = 0; i < count; i++)
		{
			other = Instance.actions[i];
			if (other.attachedTracker == data.attachedTracker && other.buttonIndex == data.buttonIndex)
				index++;
		}

		return index;
	}

	public static int GetActionIndex(int siblingIndex, in TrackerActionNetworkData data)
	{
		TrackerActionNetworkData other;

		int index = 0;
		for (int i = 0; i < Instance.actions.Count; i++)
		{
			other = Instance.actions[i];
			if (other.attachedTracker == data.attachedTracker && other.buttonIndex == data.buttonIndex)
			{
				if (index == siblingIndex)
				{
#if DEBUG
					if (!other.Equals(data))
						Debug.LogError("NetworkData for sibling index does not match the data that was provided.");
#endif
					return i;
				}
				index++;
			}
		}
		return -1;
	}

	public static Action<SyncList<TrackerNetworkData>.Operation, int, TrackerNetworkData> OnTrackersChange;
	public static Action<SyncList<TrackerActionNetworkData>.Operation, int, TrackerActionNetworkData> OnActionsChange;
	public static Action<string> OnNameChanged;
	public static Action<int> OnIconChanged;

	private static readonly List<TrackerNetworkData> backing_trackers = new List<TrackerNetworkData>();
	private static readonly List<TrackerActionNetworkData> backing_actions = new List<TrackerActionNetworkData>();


	public readonly SyncList<TrackerNetworkData> trackers = new SyncList<TrackerNetworkData>(backing_trackers);
	public readonly SyncList<TrackerActionNetworkData> actions = new SyncList<TrackerActionNetworkData>(backing_actions);

	[SyncVar] public double recoverTime = 0; // Value should be saved to files and passed on initialization. Never updated otherwise.
	[SyncVar(hook = nameof(CallOnNameChanged))] public string Name;
	[SyncVar(hook = nameof(CallOnIconChanged))] public int Icon;

	public static double RunTime => NetworkClient.active ? NetworkTime.time - Instance.recoverTime : Instance.recoverTime;

	public static byte TrackerCount => (byte)Instance.trackers.Count;
	public static int ActionCount => Instance.actions.Count;
	
	protected virtual void Awake()
	{
		if (Instance != this)
		{
			Debug.LogError("SingletonMono<" + typeof(LobbyNetworkManager).Name + "> instance mismatch: " + m_instance + " != " + this);
		}

#if DEBUG
		OnTrackersChange += (op, index, data) => Debug.Log("Tracker " + op + " " + index + ": " + data);
		OnActionsChange += (op, index, data) => Debug.Log("Action " + op + " " + index + ": " + data);
#endif
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		recoverTime = NetworkTime.time - recoverTime;
		if (string.IsNullOrWhiteSpace(Name))
			Name = LocalSettings.Instance.DefaultLobbyName;
		if (Icon < 0)
			Icon = LocalSettings.Instance.DefaultLobbyIconIndex;
	}

	// on any type of disconnect
	public override void OnStopClient()
	{
		trackers.OnChange -= CallOnTrackersChange;
		actions.OnChange -= CallOnActionsChange;

		recoverTime = NetworkTime.time - recoverTime;
		if (trackers.Count != 0)
		{
			LobbySaves.Save();
		}
	}

	public void ResetNetworkData()
	{
		Name = null;
		Icon = -1;
		trackers.Reset();
		actions.Reset();
	}

	public override void OnStartClient()
	{
		trackers.OnChange += CallOnTrackersChange;
		actions.OnChange += CallOnActionsChange;

		// Process initial SyncList payload
		OnReinitialize();
	}

	private void OnReinitialize()
	{
		if (isClient)
		{
			ReinitializeSyncList(trackers, OnTrackersChange);
			ReinitializeSyncList(actions, OnActionsChange);
			CallOnNameChanged(null, Name);
			CallOnIconChanged(-1, Icon);
		}
	}

	public static void ReinitializeSyncList<T>(SyncList<T> list, Action<SyncList<T>.Operation, int, T> callback)
	{
		callback?.Invoke(SyncList<T>.Operation.OP_CLEAR, 0, default);
		for (int i = 0; i < list.Count; i++)
			callback?.Invoke(SyncList<T>.Operation.OP_ADD, i, list[i]);
	}

	public static void ReinitializeActionCallback(Action<SyncList<TrackerActionNetworkData>.Operation, int, TrackerActionNetworkData> callback) =>
		ReinitializeSyncList(Instance.actions, callback);

	private void CallOnTrackersChange(SyncList<TrackerNetworkData>.Operation op, int index, TrackerNetworkData data) =>
		OnTrackersChange?.Invoke(op, index, data);

	private void CallOnActionsChange(SyncList<TrackerActionNetworkData>.Operation op, int index, TrackerActionNetworkData data) =>
		OnActionsChange?.Invoke(op, index, data);

	private void CallOnNameChanged(string oldName, string newName) =>
		OnNameChanged?.Invoke(newName);

	private void CallOnIconChanged(int oldIcon, int newIcon) =>
		OnIconChanged?.Invoke(newIcon);


	public static void Cmd_ButtonPressed(byte tracker, byte buttonIndex) =>
		Instance.Server_ButtonPressed(tracker, buttonIndex);

	[Command(requiresAuthority = false)]
	private void Server_ButtonPressed(byte tracker, byte buttonIndex)
	{
		UnityEngine.Debug.Log("Button pressed: " + tracker + ", " + buttonIndex);
		for (int i = 0; i < actions.Count; i++)
		{
			if (actions[i].attachedTracker != tracker || actions[i].buttonIndex != buttonIndex)
				continue;

			TrackerManager.Server_PreformAction(actions[i].type, actions[i].target, actions[i].data);
		}
	}

	[Command(requiresAuthority = false)]
	private void Server_AddNewTracker()
	{
		int index;
		for (index = 0; index < trackers.Count; index++)
			if (!trackers[index].Valid)
				break;

		var data = new TrackerNetworkData(
			name:      "Tracker " + (index + 1),
			rank:      "RR",
			iconIndex: (ushort)GlobalSpriteList.RandomIconIndex,
			color:     ColorWheelPopup.RandomColor,
			time:      300
		);

		if (index == trackers.Count)
			trackers.Add(data);
		else
			Server_SetTracker(index, data);
	}
	public static void Cmd_AddNewTracker() => Instance.Server_AddNewTracker();

	[Command(requiresAuthority = false)]
	private void Server_DuplicateTracker(int sourceId)
	{
		var data = GetTrackerData(sourceId);
		data.SetName((data.Name.Length > 15 ? data.Name.Substring(0, 15) : data.Name) + " (Copy)");

		int index;
		for (index = 0; index < trackers.Count; index++)
			if (!trackers[index].Valid)
				break;
	
		if (index == trackers.Count)
			trackers.Add(data);
		else
			Server_SetTracker(index, data);
	}
	public static void Cmd_DuplicateTracker(int sourceId) => Instance.Server_DuplicateTracker(sourceId);

	[Command(requiresAuthority = false)]
	private void Server_RemoveTracker(int index)
	{
		var data = GetTrackerData(index);
		data.Invalidate();
		Server_SetTracker(index, data);

		for (int i = 0; i < actions.Count; i++)
		{
			if (actions[i].attachedTracker == index)
				actions.RemoveAt(i--);
		}
	}
	public static void Cmd_RemoveTracker(int index) => Instance.Server_RemoveTracker(index);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker(int index, TrackerNetworkData data) => Server_SetTracker(index, data);
	public static void Cmd_ChangeTracker(int index, TrackerNetworkData data) => Instance.Server_ChangeTracker(index, data);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker_PhysicalTimeRemaining(int index, float time)
	{
		var data = GetTrackerData(index);
		data.SetPhysicalTimeRemaining(time);
		Server_SetTracker(index, data);
	}
	public static void Cmd_ChangeTracker_PhysicalTimeRemaining(int index, float time) => Instance.Server_ChangeTracker_PhysicalTimeRemaining(index, time);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker_TimerActive(int index, bool active)
	{
		var data = GetTrackerData(index);
		if (active) data.StartTimer();
		else data.StopTimer();
		Server_SetTracker(index, data);
	}
	public static void Cmd_ChangeTracker_TimerActive(int index, bool active) => Instance.Server_ChangeTracker_TimerActive(index, active);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker_Name(int index, string name)
	{
		var data = GetTrackerData(index);
		data.SetName(name);
		Server_SetTracker(index, data);
	}
	public static void Cmd_ChangeTracker_Name(int index, string name) => Instance.Server_ChangeTracker_Name(index, name);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker_Color(int index, Color32 color)
	{
		var data = GetTrackerData(index);
		data.SetColor(color);
		Server_SetTracker(index, data);
	}
	public static void Cmd_ChangeTracker_Color(int index, Color32 color) => Instance.Server_ChangeTracker_Color(index, color);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker_Rank(int index, string rank)
	{
		var data = GetTrackerData(index);
		data.SetRank(rank);
		Server_SetTracker(index, data);
	}
	public static void Cmd_ChangeTracker_Rank(int index, string rank) => Instance.Server_ChangeTracker_Rank(index, rank);

	[Command(requiresAuthority = false)]
	private void Server_ChangeTracker_IconIndex(int index, int iconIndex)
	{
		var data = GetTrackerData(index);
		data.SetIconIndex((ushort)iconIndex);
		Server_SetTracker(index, data);
	}
	public static void Cmd_ChangeTracker_IconIndex(int index, int iconIndex) => Instance.Server_ChangeTracker_IconIndex(index, iconIndex);

	[Command(requiresAuthority = false)]
	private void Server_AddAction(TrackerActionNetworkData data) => actions.Add(data);
	public static void Cmd_AddAction(TrackerActionNetworkData data) => Instance.Server_AddAction(data);

	[Command(requiresAuthority = false)]
	private void Server_RemoveAction(int index) => actions.RemoveAt(index);
	public static void Cmd_RemoveAction(int index) => Instance.Server_RemoveAction(index);

	[Command(requiresAuthority = false)]
	private void Server_ChangeAction(int index, TrackerActionNetworkData data)
	{
		if (index < 0 || index >= actions.Count)
		{
			Debug.LogError("TrackerActionNetworkData index out of range " + actions.Count + ": " + index + ". This should never happen.");
			return;
		}
		actions[index] = data;
	}
	public static void Cmd_ChangeAction(int index, TrackerActionNetworkData data) => Instance.Server_ChangeAction(index, data);

	[Command(requiresAuthority = false)]
	private void Server_ChangeLobbyName(string name) => Name = name;
	public static void Cmd_ChangeLobbyName(string name) => Instance.Server_ChangeLobbyName(name);

	[Command(requiresAuthority = false)]
	private void Server_ChangeLobbyIcon(int icon) => Icon = icon;
	public static void Cmd_ChangeLobbyIcon(int icon) => Instance.Server_ChangeLobbyIcon(icon);

	[Command(requiresAuthority = false)]
	private void Server_BroadcastReinitialize(byte[] data)
	{
		try {
			// Deserialize the data
			using (var reader = NetworkReaderPool.Get(data))
				OnDeserialize(reader, true);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}

		RpcReinitialize(data);
		recoverTime = NetworkTime.time - recoverTime;
	}
	[ClientRpc]
	private void RpcReinitialize(byte[] data)
	{
		if (!isServer) // Server (i.e. host) already deserialized the data
		{
			try {
				// Deserialize the data
				using (var reader = NetworkReaderPool.Get(data))
					OnDeserialize(reader, true);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		OnReinitialize();
	}
	public static void Cmd_BroadcastReinitialize(byte[] data) => Instance.Server_BroadcastReinitialize(data);
}

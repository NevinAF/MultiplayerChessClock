using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TrackerPresenter : MonoBehaviour
{
#if DEBUG
	private bool m_hasStarted = false;
#endif

	private byte networkID;
	public byte NetworkID
	{
		get => networkID;
		set
		{
#if DEBUG
			if (m_hasStarted)
				Debug.LogError("NetworkID should be set before Start and should never be changed.");
#endif
			networkID = value;
		}
	}

	public TrackerDispatchers Dispatcher;

	public List<TrackerButton> buttons = new List<TrackerButton>();
	public RectTransform DefaultButtonGroup;

	private void Start()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			byte index = (byte)i;
			buttons[i].OnClick(() => LobbyNetworkManager.Cmd_ButtonPressed(NetworkID, index));
		}
	}

	public void UpdateFromNetwork()
	{
#if DEBUG
		m_hasStarted = true;
#endif
		var data = LobbyNetworkManager.GetTrackerData(networkID);
		Dispatcher.UpdateFromSource(data);
	}

	public void Update()
	{
		Dispatcher.UpdateTimers();
	}
}

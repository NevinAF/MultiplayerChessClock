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

	public SDispatcher<bool> Valid => Dispatcher.Valid;
	public SDispatcher<string> Name => Dispatcher.Name;
	public SDispatcher<Color> Color => Dispatcher.Color;
	public SDispatcher<double> SetTime => Dispatcher.SetTime;
	public SDispatcher<string> Rank => Dispatcher.Rank;
	public SDispatcher<int> IconIndex => Dispatcher.IconIndex;
	public SDispatcher<bool> TimerActive => Dispatcher.TimerActive;
	public SDispatcher<string> HumanReadableTime => Dispatcher.HumanReadableTime;
	public SDispatcher<Sprite> Icon => Dispatcher.Icon;


	public List<TrackerButton> buttons = new List<TrackerButton>();
	public Transform DefaultButtonGroup;

	private void Start()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			byte index = (byte)i;
			buttons[i].OnClick(() => LobbyNetworkManager.Cmd_ButtonPressed(NetworkID, (byte)i));
		}
	}

	public void UpdateFromNetwork()
	{
#if DEBUG
		m_hasStarted = true;
#endif
		Dispatcher.UpdateFromNetwork(NetworkID);
	}

	public void Update()
	{
		// Only recreate the string if time is changing. The string is always updated on SetTime change.
		if (TimerActive.Value)
			HumanReadableTime.Value = Formatting.Time(SetTime - LobbyNetworkManager.CTime);
	}
}

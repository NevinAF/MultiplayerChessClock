using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrackerActionDispatcher : MonoBehaviour
{
	public TrackerActionNetworkData NetworkData { get; set; }

	public TrackerDispatchers Target;
	public SDispatcher<Sprite> ActionIcon;
	public SDispatcher<string> Label;

	public SDispatcher<bool> Invalid;
	public SDispatcher<bool> Disabled;
	public SDispatcher<bool> Reduced;
	public SDispatcher<bool> Editable;

	public Button EditButton;
	public void AddOnEditAction(UnityAction action)
	{
		Editable.Value = true;
		EditButton.onClick.AddListener(action);
	}

	public void SetNetworkData(TrackerActionNetworkData data)
	{
		NetworkData = data;

		if (enabled)
		{
			if (NetworkData.IsValid) {
				Target.PipeFrom(NetworkData.target);
			}
			else
				Target.ClearData();
		}
	}

	public void SetNetworkData(int networkID)
	{
		NetworkData = LobbyNetworkManager.GetTrackerActionData(networkID);
	}

	protected void Start()
	{
		if (NetworkData.type == 0) {
			Debug.LogError("TrackerActionNetworkData not set! After Instantiation, SetNetworkData must be called.", this);
			// Destroy(this);
			return;
		}
	}

	private void OnEnable()
	{
		if (NetworkData.IsValid)
			Target.PipeFrom(NetworkData.target);
	}

	private void OnDisable()
	{
		Target.ClearData();
	}
}
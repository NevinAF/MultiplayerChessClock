using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnOffTimer_Action : TrackerAction
{
	public TrackerDispatchers CopyFrom;

	public string InvertLabel;
	public string OffLabel;
	public string OnLabel;
	public string ToggleLabel;
	public string UseLabel;

	public override void ApplySolo(List<int> actionIds)
	{
		short toggleTarget = (short)NetworkData.data;

		if (toggleTarget < 0) // Invert value of another tracker
		{
			CopyFrom.gameObject.SetActive(true);
			CopyFrom.PipeFrom((byte)(-toggleTarget - 1));
			Dispatcher.Label.Value = InvertLabel;
		}

		else if (toggleTarget == short.MaxValue) // Off
		{
			CopyFrom.ClearData();
			CopyFrom.gameObject.SetActive(false);
			Dispatcher.Label.Value = OffLabel;
		}

		else if (toggleTarget == short.MaxValue - 1) // On
		{
			CopyFrom.ClearData();
			CopyFrom.gameObject.SetActive(false);
			Dispatcher.Label.Value = OnLabel;
		}

		else if (toggleTarget == short.MaxValue - 2) // Toggle
		{
			CopyFrom.ClearData();
			CopyFrom.gameObject.SetActive(false);
			Dispatcher.Label.Value = ToggleLabel;
		}

		else // Use value of another tracker
		{
			CopyFrom.gameObject.SetActive(true);
			CopyFrom.PipeFrom((byte)toggleTarget);
			Dispatcher.Label.Value = UseLabel;
		}
	}

	public override void Server_PreformActionPrefab(ref TrackerNetworkData targetNetworkData, ushort data)
	{
		bool wantsActive;

		short toggleTarget = (short)data;
		if (toggleTarget < 0) // Invert value of another tracker
			wantsActive = !LobbyNetworkManager.GetTrackerData(-toggleTarget - 1).TimerActive;

		else if (toggleTarget == short.MaxValue) // Off
			wantsActive = false;

		else if (toggleTarget == short.MaxValue - 1) // On
			wantsActive = true;

		else if (toggleTarget == short.MaxValue - 2) // Toggle
			wantsActive = !targetNetworkData.TimerActive;

		else // Use value of another tracker
			wantsActive = LobbyNetworkManager.GetTrackerData(toggleTarget).TimerActive;

			UnityEngine.Debug.Log("Server_PreformActionPrefab: " + wantsActive);

		if (wantsActive)
			targetNetworkData.StartTimer();
		else
			targetNetworkData.StopTimer();
	}
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetTurnTimer_Action : TrackerAction
{
	public SDispatcher<string> Time;

	public override void ApplySolo(List<int> actionIds)
	{
		int seconds = TimeShort.ToSeconds(NetworkData.data);

		Dispatcher.Label.Value = "Set Turn Time";
		Time.Value = "+" + Formatting.Time(seconds);
	}

	public override void Server_PreformActionPrefab(ref TrackerNetworkData targetNetworkData, ushort data)
	{
		UnityEngine.Debug.Log("Server_PreformActionPrefab: " + TimeShort.ToSeconds(data));
		targetNetworkData.IncPhysicalTimeRemaining(TimeShort.ToSeconds(data));
	}
}
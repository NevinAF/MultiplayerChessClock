using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetTimer_Action : TrackerAction
{
	public SDispatcher<string> Time;

	public override void ApplySolo(LinkedList<ReducingActionEntry> actionIds = null)
	{
		Dispatcher.Label.Value = "Set Timer";

		int seconds = TimeShort.ToSeconds(NetworkData.data);
		Time.Value = Formatting.Time(seconds);
	}

	public override void Server_PreformActionPrefab(ref TrackerNetworkData targetNetworkData, ushort data)
	{
		UnityEngine.Debug.Log("Server_PreformActionPrefab: " + TimeShort.ToSeconds(data));
		targetNetworkData.SetPhysicalTimeRemaining(TimeShort.ToSeconds(data));
	}
}
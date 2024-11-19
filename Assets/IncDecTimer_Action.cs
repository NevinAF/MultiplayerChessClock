using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IncDecTimer_Action : TrackerAction
{
	public SDispatcher<string> Time;

	public override void ApplySolo(List<int> actionIds)
	{
		int seconds = TimeShort.ToSeconds(NetworkData.data);

		if (seconds < 0)
		{
			Dispatcher.Label.Value = "Dec Timer";
			Time.Value = Formatting.Time(seconds);
		}
		else
		{
			Dispatcher.Label.Value = "Inc Timer";
			Time.Value = "+" + Formatting.Time(seconds);
		}
	}

	public override void Server_PreformActionPrefab(ref TrackerNetworkData targetNetworkData, ushort data)
	{
		UnityEngine.Debug.Log("Server_PreformActionPrefab: " + TimeShort.ToSeconds(data));
		targetNetworkData.TimeRemaining += TimeShort.ToSeconds(data);
	}
}
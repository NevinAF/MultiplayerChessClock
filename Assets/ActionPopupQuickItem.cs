using System;
using UnityEngine;

public class ActionPopupQuickItem : InputPopupQuickItem
{
	private TrackerAction action;
	public Transform ActionParent;

	public void SetData(TrackerActionNetworkData target)
	{
		if (action == null || !action.NetworkData.Equals(target))
		{
			if (action != null)
				Destroy(action);

			action = TrackerManager.CreateAction(target, ActionParent);
			action.ApplySolo(ActionInputPopup.emptyList);
		}
	}
}

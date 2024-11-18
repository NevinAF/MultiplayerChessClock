using System;
using UnityEngine;

public class TrackerPopupQuickItem : InputPopupQuickItem
{
	public TrackerDispatchers Dispatchers;

	public SDispatcher<bool> IsFocused;

	public void SetData(byte target)
	{
		Dispatchers.PipeFrom(target);
	}

	public void ClearData()
	{
		Dispatchers.ClearData();
	}
}

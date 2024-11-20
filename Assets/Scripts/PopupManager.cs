using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : SingletonMono<PopupManager>
{
	private static Stack<Action> clickOffLayers = new Stack<Action>();

	public Button ClickOffButton;

	public static void RegisterClickOffLayer(Action action)
	{
		UnityEngine.Debug.Log("Registering click off layer");
		Instance.ClickOffButton.gameObject.SetActive(true);
		clickOffLayers.Push(action);
	}

	public static void CloseUntil(Action action)
	{
		if (clickOffLayers.Count == 0)
		{
			Debug.LogError("CloseUntil called with no layers to close.");
			return;
		}

		int count = 0;
		foreach (Action layer in clickOffLayers)
		{
			count++;
			if (layer == action)
				goto Close;
		}

		Debug.LogError("CloseUntil could not find the action to close to. Closing all.");

	Close:
		for (int i = 0; i < count; i++)
			Instance.ClickOffTop();
	}

	public void ClickOffTop()
	{
		if (clickOffLayers.Count > 0)
			clickOffLayers.Pop()();

		if (clickOffLayers.Count == 0)
		{
			ClickOffButton.gameObject.SetActive(false);
		}
	}

	public void ClickOffAll()
	{
		while (clickOffLayers.Count > 0)
			ClickOffTop();
	}
}

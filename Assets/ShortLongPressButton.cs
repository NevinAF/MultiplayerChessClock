using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class ShortLongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	public Image graphic;
	public Color normalColor = Color.white;
	public Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1);
	public double longPressTime = 0.6;


	public Action<PointerEventData> PressStarted;
	public Action<PointerEventData> ShortPress;
	public Action<PointerEventData> LongPress;

	private PointerEventData currentEvent;
	private double pointerDownTime;

	public void Update()
	{
		if (currentEvent != null && Time.timeAsDouble - pointerDownTime > longPressTime)
		{
			if (LongPress == null) {
				EventUsed();
				return;
			}

			foreach (var handler in LongPress.GetInvocationList())
			{
				if (handler is Action<PointerEventData> action)
				{
					action.Invoke(currentEvent);
					if (currentEvent.used)
					{
						EventUsed();
						return;
					}
				}
			}
		}
	}

	private void EventUsed()
	{
		currentEvent = null;
		graphic.color = normalColor;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.used) return;

		if (PressStarted != null)
		{
			foreach (var handler in PressStarted.GetInvocationList())
			{
				if (handler is Action<PointerEventData> action)
				{
					action.Invoke(eventData);
					if (eventData.used)
						return;
				}
			}
		}

		currentEvent = eventData;
		graphic.color = pressedColor;
		pointerDownTime = Time.timeAsDouble;
	}

	// If the user releases the button before the long press time, it's a short press
	public void OnPointerUp(PointerEventData eventData)
	{
		if (currentEvent == null)
			return;

		if (ShortPress != null)
		{
			foreach (var handler in ShortPress.GetInvocationList())
			{
				if (handler is Action<PointerEventData> action)
				{
					action.Invoke(eventData);
					if (eventData.used)
						break;
				}
			}
		}

		EventUsed();
	}

	public void OnPointerExit(PointerEventData _)
	{
		if (currentEvent == null)
			return;

		EventUsed();
	}
}

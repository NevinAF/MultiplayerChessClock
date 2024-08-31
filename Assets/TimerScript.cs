using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

[RequireComponent(typeof(DraggableLayout))]
[RequireComponent(typeof(ShortLongPressButton))]
public class TimerScript : MonoBehaviour
{
	public TextMeshProUGUI LabelTMP;
	public TextMeshProUGUI ValueText;

	public DraggableLayout DraggableLayout { get; private set; }
	public ShortLongPressButton ShortLongPressButton { get; private set; }

	[NonSerialized] public double value;

	public double CurrentTime => value < 0 ? -value : value - Configuration.Instance.CTime;
	public bool IsActive => value > 0;

	private void Awake()
	{
		DraggableLayout = GetComponent<DraggableLayout>();
		ShortLongPressButton = GetComponent<ShortLongPressButton>();
	}

	public void SetName(string newName)
	{
		LabelTMP.text = newName;
	}

	public void Update()
	{
		ValueText.text = FormatTime(CurrentTime);
	}

	public static string FormatTime(double time)
	{
		if (time < 0)
			return "∞";
		if (time < 60)
			return time.ToString("F2");
		if (time < 3600)
			return $"{(int)time / 60}:{(int)(time % 60):00}";
		return $"{(int)time / 3600}:{(int)(time / 60) % 60}:{time % 60:00}";
	}
}

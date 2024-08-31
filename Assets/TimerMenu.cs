using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class TimerMenu : MonoBehaviour
{
	public static TimerMenu Instance;

	public void Awake()
	{
		Instance = this;
		gameObject.SetActive(false);
	}

	public TMP_InputField LabelTMP;
	public TMP_InputField TimeTMP;
	[NonSerialized] public int _index = -1;

	public void Open(int index, TimerData data)
	{
		_index = index;
		LabelTMP.text = data.label;
		TimeTMP.text = (data.value < 0 ? -data.value : data.value - Configuration.Instance.CTime).ToString();
		gameObject.SetActive(true);
	}

	public void SaveAndClose()
	{
		gameObject.SetActive(false);
		Configuration.Instance.SetTimer(_index, LabelTMP.text, double.Parse(TimeTMP.text));
	}

	public void Remove()
	{
		gameObject.SetActive(false);
		Configuration.Instance.RemoveTimer(_index);
	}
}

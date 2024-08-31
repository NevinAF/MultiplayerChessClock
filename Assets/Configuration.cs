using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class Configuration : NetworkBehaviour
{
	public readonly List<TimerScript> timers = new List<TimerScript>();
	public readonly SyncList<TimerData> timerDatas = new SyncList<TimerData>();

	public static Configuration Instance;

	private double disconnectTime = 0;
	public double CTime => double.IsNaN(disconnectTime) ? NetworkTime.time : Time.timeAsDouble - disconnectTime;

	public DraggableLayoutManager timersLayout;
	public TimerScript prefab;

	internal bool LayoutMode = false;

	public void Awake()
	{
		Instance = this;
	}

	// on any type of disconnect
	public override void OnStopClient()
	{
		disconnectTime = Time.timeAsDouble - NetworkTime.time;
		timerDatas.OnAdd -= OnOPAdd;
		timerDatas.OnInsert -= OnOPInsert;
		timerDatas.OnRemove -= OnOPRemoveAt;
		timerDatas.OnSet -= OnOPSet;
		timerDatas.OnClear -= OnOPClear;
	}

	public override void OnStartClient()
	{
		disconnectTime = double.NaN;
		timerDatas.OnAdd += OnOPAdd;
		timerDatas.OnInsert += OnOPInsert;
		timerDatas.OnRemove += OnOPRemoveAt;
		timerDatas.OnSet += OnOPSet;
		timerDatas.OnClear += OnOPClear;
		
		// Process initial SyncList payload
		if (!isServer)
		{
			OnOPClear();

			for (int index = 0; index < timerDatas.Count; index++)
				OnOPAdd(index);
		}
	}

	private TimerScript InstantiateTimer(int index)
	{
		TimerScript timer = Instantiate(prefab, transform);
		timer.ShortLongPressButton.LongPress += e => {
			TimerMenu.Instance.Open(index, timerDatas[index]);
			e.Use();
		};
		timer.ShortLongPressButton.ShortPress += e => {
			ToggleTimer(index);
			e.Use();
		};
		timer.gameObject.SetActive(false);
		return timer;
	}

	private void OnOPAdd(int index) {
		while (timers.Count <= index)
			timers.Add(InstantiateTimer(timers.Count));

		OnOPSet(index, default);
	}

	private void OnOPInsert(int index) {
		throw new System.InvalidOperationException("This should never be called");
	}

	private void OnOPRemoveAt(int index, TimerData oldItem) {
		throw new System.InvalidOperationException("This should never be called");
	}

	private void OnOPSet(int index, TimerData oldItem)
	{
		if (timerDatas[index].active) {
			EnableTimer(index);
			timers[index].SetName(timerDatas[index].label);
			timers[index].value = timerDatas[index].value;
		}
		else
			DisableTimer(index);
	}

	private void OnOPClear()
	{
		for (int i = 0; i < timers.Count; i++)
			DisableTimer(i);

		TimerMenu.Instance.gameObject.SetActive(false);
	}

	private void DisableTimer(int index)
	{
		if (!timers[index].gameObject.activeSelf)
			return;

		timersLayout.RemoveFromLayout(timers[index].DraggableLayout, transform);
		timers[index].gameObject.SetActive(false);
	}

	public void EnableTimer(int index)
	{
		if (timers[index].gameObject.activeSelf)
			return;

		timers[index].gameObject.SetActive(true);
		timersLayout.InsertNew(timers[index].DraggableLayout);
	}

	[Command(requiresAuthority = false)]
	public void ToggleTimer(int index)
	{
		UnityEngine.Debug.Log("ToggleTimer " + index);
		if (index < 0 || index >= timerDatas.Count)
			return;
		var old = timerDatas[index];
		timerDatas[index] = new TimerData
		{
			active = old.active,
			label = old.label,
			value = CTime - old.value
		};
	}

	[Command(requiresAuthority = false)]
	public void CmdCreateTimer()
	{
		TimerData timer = new TimerData
		{
			active = true,
			label = "Timer " + timerDatas.Count.ToString("X"),
			value = -60 * 5
		};
		timerDatas.Add(timer);
	}

	[Command(requiresAuthority = false)]
	public void RemoveTimer(int index)
	{
		if (index < 0 || index >= timerDatas.Count)
			return;
		timerDatas[index] = default;
	}

	[Command(requiresAuthority = false)]
	public void SetTimer(int index, string label, double value)
	{
		if (index < 0 || index >= timerDatas.Count)
			return;
		var old = timerDatas[index];
		timerDatas[index] = new TimerData
		{
			active = old.active,
			label = label,
			value = old.value < 0 ? -value : CTime + value
		};
	}
}

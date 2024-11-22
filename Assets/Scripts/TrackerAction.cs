using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class TrackerAction : MonoBehaviour
{
	public TrackerActionDispatcher Dispatcher;
	public TrackerActionNetworkData NetworkData => Dispatcher.NetworkData;
	public bool Initialized => Dispatcher.NetworkData.type != 0;


	public virtual void Start()
	{
		Dispatcher.Target.Valid.Attach(UpdateValid);
	}

	public abstract void ApplySolo(LinkedList<ReducingActionEntry> actionIds = null);
	public virtual void ApplyReduceWith(LinkedList<ReducingActionEntry> actionIds) => ApplySolo();
	public abstract void Server_PreformActionPrefab(ref TrackerNetworkData targetNetworkData, ushort data);

	public virtual void UpdateValid()
	{
		Dispatcher.Invalid.Value = !Dispatcher.Target.Valid;
	}

	protected virtual void UpdateValid(bool _) => UpdateValid();

	public virtual void UpdateDisabled()
	{
		Dispatcher.Disabled.Value = false;
	}
}
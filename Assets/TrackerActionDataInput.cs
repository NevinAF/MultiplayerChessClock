using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class TrackerActionDataInput : MonoBehaviour
{
	public TrackerAction action;
	public string Name;

	public Graphic DropdownGraphic;

	public Action<ushort> SetData;

	protected virtual void OnEnable()
	{
		DropdownGraphic.color = ActionInputPopup.Instance.DropdownFocusColor;
	}

	public abstract void UpdatePreview();
	public abstract ushort DefaultData { get; }
	public virtual bool IsDataValid() => true;
	protected virtual void OnDisable()
	{
		DropdownGraphic.color = ActionInputPopup.Instance.DropdownNormalColor;
	}
}
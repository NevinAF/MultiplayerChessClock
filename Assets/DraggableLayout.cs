using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;

public class DraggableLayout : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
	public Image graphic;
	public Color normalColor = Color.white;
	public Color draggingColor = new Color(0.5f, 0.5f, 0.5f, 1);

	[NonSerialized] public DraggableLayoutManager manager;

	public void OnDrag(PointerEventData eventData)
	{
		if (manager.enabled == false)
			return;

		graphic.color = draggingColor;
		manager.OnElementDragged(eventData, this);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		graphic.color = normalColor;
		manager.OnElementDropped(eventData);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (manager.enabled == false)
			return;

		eventData.Use();
	}
}

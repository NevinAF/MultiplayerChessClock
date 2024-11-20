using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class DraggableLayoutManager : MonoBehaviour
{
	private static List<Vector4> anchors = new List<Vector4>(); // x = left, y = top, z = right, w = bottom
	private static List<int> _matches = new List<int>();
	private static int activeIndex;
	private static DraggableLayout activeComponent;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Vector4 GetAnchor(RectTransform target)
	{
		return new Vector4(target.anchorMin.x, target.anchorMin.y, target.anchorMax.x, target.anchorMax.y);
	}

	public void ResetLayout()
	{
		// Create a grid layout based on the number of children. Each child should be organized so it is as square as possible.
		int count = transform.childCount;
		int rows = Mathf.CeilToInt(Mathf.Sqrt(count));
		int cols = Mathf.CeilToInt((float)count / rows);

		int i = 0;
		for (; i < count; i++)
		{
			RectTransform target = transform.GetChild(i) as RectTransform;
			Vector4 anchor = new Vector4((i % cols) / (float)cols, (i / cols) / (float)rows, (i % cols + 1) / (float)cols, (i / cols + 1) / (float)rows);
			target.anchorMin = new Vector2(anchor.x, anchor.y);
			target.anchorMax = new Vector2(anchor.z, anchor.w);
		}

		if (cols * rows != i)
		{
			// Fill the remaining area...
			Vector4 anchorVoid = new Vector4((i % cols) / (float)cols, (i / cols) / (float)rows, 1, 1);
			PrepareAnchors();
			FillVoid(1, 3, 0, 2, anchorVoid);
		}

	}

	private void PrepareAnchors(DraggableLayout active = null)
	{
		anchors.Clear();

#if DEBUG
		activeIndex = -1;
		if (active != null && active.transform.parent != transform)
			throw new Exception("Active transform is not a child of the layout");
#endif
		activeComponent = active;

		for (int i = 0; i < transform.childCount; i++)
		{
			RectTransform target = transform.GetChild(i) as RectTransform;
			anchors.Add(GetAnchor(target));

			if (active != null && active.transform == target)
				activeIndex = i;
		}

#if DEBUG
		if (active == null)
			activeIndex = -2;
		else if (activeIndex == -1)
			throw new Exception("Active transform not found in the layout");
#endif
	}

	public void RemoveFromLayout(DraggableLayout target, Transform newParent)
	{
		if (activeComponent != null)
			activeComponent.OnEndDrag(null);

		Vector4 anchorVoid = GetAnchor(target.transform as RectTransform);
		target.transform.SetParent(newParent, false);
		target.manager = null;

		if (transform.childCount == 0)
			return;

		PrepareAnchors(null);
		FillVoid(0, 1, 3, 2, anchorVoid);
	}

	public void InsertNew(DraggableLayout target)
	{
		if (activeComponent != null)
			activeComponent.OnEndDrag(null);

		Vector4 selfAnchor;
		if (transform.childCount == 0)
			selfAnchor = new Vector4(0, 0, 1, 1);
		else
		{
			RectTransform splitTarget = transform.GetChild(0) as RectTransform;
			Vector4 anchor = GetAnchor(splitTarget);

			// Split left if the width is greater than the height, otherwise split top
			SplitAnchor(ref anchor, out selfAnchor, splitTarget.rect.width > splitTarget.rect.height ? 0 : 1);
			SetTransformAnchors(0, anchor);
		}

		target.transform.SetParent(transform, false);
		SetTransformAnchors(transform.childCount - 1, selfAnchor);
		target.manager = this;
	}

	public static int GetVectorDirection(Vector2 direction)
	{
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
			return direction.x > 0 ? 0 : 2;
		else return direction.y > 0 ? 1 : 3;
	}

	public int GetMouseOver(Vector2 mousePosition)
	{
#if DEBUG
		if (activeIndex < 0 || anchors.Count == 0)
			throw new Exception("Anchors not prepared!");
#endif

		for (int i = 0; i < anchors.Count; i++)
		{
			if (mousePosition.x > anchors[i].x && mousePosition.x < anchors[i].z &&
				mousePosition.y > anchors[i].y && mousePosition.y < anchors[i].w)
				return i;
		}

		return activeIndex;
	}

	public void SetTransformAnchors(int index, Vector4 anchors)
	{
		RectTransform target = transform.GetChild(index) as RectTransform;
		target.anchorMin = new Vector2(anchors.x, anchors.y);
		target.anchorMax = new Vector2(anchors.z, anchors.w);
	}

	public static void SplitAnchor(ref Vector4 anchor, out Vector4 other, int dir)
	{
		other = anchor;
		int ud = dir % 2;
		float mid = ((anchor[2 + ud] - anchor[0 + ud]) / 2f) + anchor[0 + ud];
		anchor[(2 + dir) % 4] = mid;
		other[dir] = mid;
	}

	public bool GenerateSplitAnchors(PointerEventData eventData, out int mouseOver, out Vector4 mousedAnchor, out Vector4 selfAnchor)
	{
#if DEBUG
		if (activeIndex < 0 || anchors.Count == 0)
			throw new Exception("Anchors not prepared!");
#endif

		// Convert the mouse point into a normalized position on the canvas:
		Vector2 mousePosition = eventData.position - Vector2.up * 200;
		Vector2 canvasSize = (transform as RectTransform).rect.size;
		mousePosition.x /= canvasSize.x;
		mousePosition.y /= canvasSize.y;

		mouseOver = GetMouseOver(mousePosition);
		mousedAnchor = anchors[mouseOver];

		if (mouseOver == activeIndex) {
			selfAnchor = mousedAnchor;
			return false;
		}

		Vector2 positionInRect = (mousePosition - (Vector2)mousedAnchor) / (new Vector2(mousedAnchor.z, mousedAnchor.w) - (Vector2)mousedAnchor) - new Vector2(0.5f, 0.5f);
		int dir = GetVectorDirection(positionInRect);

		SplitAnchor(ref mousedAnchor, out selfAnchor, dir);

		return true;
	}


	public void FillVoid(int primary, int secondary, int tertiary, int quaternary, Vector4 anchorVoid)
	{
		if (!TryFill(primary, anchorVoid) &&
			!TryFill(secondary, anchorVoid) &&
			!TryFill(tertiary, anchorVoid) &&
			!TryFill(quaternary, anchorVoid))
			throw new Exception("Failed to fill in the original position of the timer being dragged");

		for (int i = 0; i < anchors.Count; i++)
			SetTransformAnchors(i, anchors[i]);
	}

	public bool TryFill(int dir, Vector4 anchorVoid)
	{
#if DEBUG
		if (activeIndex == -1 || anchors.Count == 0)
			throw new Exception("Anchors not prepared!");
#endif

		_matches.Clear();
		
		// First check right to fill left:
		for (int i = 0; i < anchors.Count; i++)
		{
			if (anchors[i][0 + dir] == anchorVoid[(2 + dir) % 4])
			{
				int lr = dir % 2;
				bool over = anchors[i][1 - lr] >= anchorVoid[1 - lr];
				bool under = anchors[i][3 - lr] <= anchorVoid[3 - lr];

				if (over && under) {
					_matches.Add(i);
				}

				if ((anchors[i][1 - lr] < anchorVoid[3 - lr] && !under) || (anchors[i][3 - lr] > anchorVoid[1 - lr] && !over)) {
					return false;
				}
			}
		}

		if (_matches.Count == 0) {
			return false;
		}

		// Fill left:
		for (int i = 0; i < _matches.Count; i++) {
			Vector4 anchor = anchors[_matches[i]];
			anchor[dir] = anchorVoid[dir];
			anchors[_matches[i]] = anchor;
		}

		return true;
	}

	public void OnElementDragged(PointerEventData eventData, DraggableLayout target)
	{
		if (!enabled)
			return;

		if (activeComponent != target) {
			activeIndex = -1;
			PrepareAnchors(target);
		}

		GenerateSplitAnchors(eventData, out int mouseOver, out Vector4 mousedAnchor, out Vector4 selfAnchor);

		for (int i = 0; i < anchors.Count; i++)
		{
			if (i == activeIndex)
				SetTransformAnchors(i, selfAnchor);
			else if (i == mouseOver)
				SetTransformAnchors(i, mousedAnchor);
			else
				SetTransformAnchors(i, anchors[i]);
		}
	}

	public void OnElementDropped(PointerEventData eventData)
	{
		if (activeIndex == -1 || anchors.Count == 0 || !enabled)
			return;

		if (eventData == null) {
			activeIndex = -1;
			activeComponent = null;

			for (int i = 0; i < anchors.Count; i++)
				SetTransformAnchors(i, anchors[i]);

			return;
		}

		bool change = GenerateSplitAnchors(eventData, out int mouseOver, out Vector4 mousedAnchor, out Vector4 selfAnchor);

		if (change)
		{
			Vector4 anchorVoid = anchors[activeIndex];

			anchors[mouseOver] = mousedAnchor; // apply the new moused anchor
			anchors[activeIndex] = selfAnchor; // apply the new self anchor

			Vector2 diff = (Vector2)anchors[activeIndex] - (Vector2)anchorVoid;
			switch (GetVectorDirection(diff))
			{
				case 0: FillVoid(2, 1, 3, 0, anchorVoid); break;
				case 1: FillVoid(1, 2, 0, 3, anchorVoid); break;
				case 2: FillVoid(0, 1, 3, 2, anchorVoid); break;
				case 3: FillVoid(3, 2, 0, 1, anchorVoid); break;
			}
		}
		else for (int i = 0; i < anchors.Count; i++)
			SetTransformAnchors(i, anchors[i]);
	}

	private void OnDisable()
	{
		if (activeComponent != null)
			activeComponent.OnEndDrag(null);
	}
}

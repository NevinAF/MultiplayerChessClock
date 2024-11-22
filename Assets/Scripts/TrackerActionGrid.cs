using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Layout class to arrange child elements in a grid format.
/// </summary>
/// <remarks>
/// The GridLayoutGroup component is used to layout child layout elements in a uniform grid where all cells have the same size. The size and the spacing between cells is controlled by the GridLayoutGroup itself. The children have no influence on their sizes.
/// </remarks>
public class TrackerActionGrid : LayoutGroup
{
	/// <summary>
	/// Which corner is the starting corner for the grid.
	/// </summary>
	public enum Corner
	{
		/// <summary>
		/// Upper Left corner.
		/// </summary>
		UpperLeft = 0,
		/// <summary>
		/// Upper Right corner.
		/// </summary>
		UpperRight = 1,
		/// <summary>
		/// Lower Left corner.
		/// </summary>
		LowerLeft = 2,
		/// <summary>
		/// Lower Right corner.
		/// </summary>
		LowerRight = 3
	}

	[SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;

	public RectTransform Background;

	/// <summary>
	/// Which corner should the first cell be placed in?
	/// </summary>
	public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }

	[SerializeField] protected Vector2 m_CellSize = new Vector2(100, 100);

	/// <summary>
	/// The size to use for each cell in the grid.
	/// </summary>
	public Vector2 cellSize { get { return m_CellSize; } set { SetProperty(ref m_CellSize, value); } }

	[SerializeField] protected Vector2 m_Spacing = Vector2.zero;

	/// <summary>
	/// The spacing to use between layout elements in the grid on both axises.
	/// </summary>
	public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

	[SerializeField] protected int m_ConstraintCount = 2;

	/// <summary>
	/// How many cells there should be along the constrained axis.
	/// </summary>
	public int constraintCount { get { return m_ConstraintCount; } set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); } }


	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		constraintCount = constraintCount;
	}

	#endif

	/// <summary>
	/// Called by the layout system to calculate the horizontal layout size.
	/// Also see ILayoutElement
	/// </summary>
	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();

		int preferredColumns = Mathf.Min(rectChildren.Count, m_ConstraintCount);
		float minSpace = padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x;
		SetLayoutInputForAxis(minSpace, minSpace, -1, 0);

		Background.sizeDelta = new Vector2(
			padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
			Background.sizeDelta.y
		);
	}

	/// <summary>
	/// Called by the layout system to calculate the vertical layout size.
	/// Also see ILayoutElement
	/// </summary>
	public override void CalculateLayoutInputVertical()
	{
		int minRows = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
		float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
		SetLayoutInputForAxis(minSpace, minSpace, -1, 1);

		rectTransform.sizeDelta = new Vector2(
			rectTransform.sizeDelta.x,
			minSpace
		);

		Background.sizeDelta = new Vector2(
			Background.sizeDelta.x,
			minSpace
		);
	}

	/// <summary>
	/// Called by the layout system
	/// Also see ILayoutElement
	/// </summary>
	public override void SetLayoutHorizontal()
	{
		SetCellsAlongAxis(0);
	}

	/// <summary>
	/// Called by the layout system
	/// Also see ILayoutElement
	/// </summary>
	public override void SetLayoutVertical()
	{
		SetCellsAlongAxis(1);
	}

	private void SetCellsAlongAxis(int axis)
	{
		// Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
		// and only vertical values when invoked for the vertical axis.
		// However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
		// Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
		// and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
		var rectChildrenCount = rectChildren.Count;
		if (axis == 0)
		{
			// Only set the sizes when invoked for horizontal axis, not the positions.

			for (int i = 0; i < rectChildrenCount; i++)
			{
				RectTransform rect = rectChildren[i];

				m_Tracker.Add(this, rect,
					DrivenTransformProperties.Anchors |
					DrivenTransformProperties.AnchoredPosition |
					DrivenTransformProperties.SizeDelta);

				rect.anchorMin = Vector2.up;
				rect.anchorMax = Vector2.up;
				rect.sizeDelta = cellSize;
			}
			return;
		}

		float width = rectTransform.rect.size.x;
		float height = rectTransform.rect.size.y;

		int cellCountX = Mathf.Min(rectChildrenCount, m_ConstraintCount);
		int cellCountY = 1;

		if (rectChildrenCount > cellCountX)
			cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);

		int cornerX = (int)startCorner % 2;
		int cornerY = (int)startCorner / 2;

		int cellsPerMainAxis, actualCellCountX, actualCellCountY;
		cellsPerMainAxis = cellCountX;
		actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);
		actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));

		Vector2 requiredSpace = new Vector2(
			actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
			actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
		);
		Vector2 startOffset = new Vector2(
			GetStartOffset(0, requiredSpace.x),
			GetStartOffset(1, requiredSpace.y)
		);

		for (int i = 0; i < rectChildrenCount; i++)
		{
			int positionX;
			int positionY;
			positionX = i % cellsPerMainAxis;
			positionY = i / cellsPerMainAxis;

			if (cornerX == 1)
				positionX = actualCellCountX - 1 - positionX;
			if (cornerY == 1)
				positionY = actualCellCountY - 1 - positionY;

			SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
			SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
		}
	}
}
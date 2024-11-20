using System;
using UnityEngine.Events;

public class SpriteInputPopup : InputPopup<SpriteInputPopup, int, SpriteInputPopup.InputOptions, SpritePopupQuickItem>
{
	public class InputOptions : InputPopupShowOptions<int>
	{
		public int currentValue;
	}

	private bool valid = false;
	private int currentValue;

	protected override void OnShow(InputOptions options)
	{
		currentValue = options.currentValue;
	}

	protected override bool ResultValid => valid;

	protected override void Start()
	{
		var sprites = GlobalSpriteList.Instance.sprites;
		for (int i = 0; i < sprites.Length; i++)
			AddQuickFillEntry(i, InputQuickFillIconType.Other);

		base.Start();
	}

	protected override int ValueSortScore(int value) => throw new Exception("Results should never be valid and this should never be called.");

	protected override void QuickFillClicked(int value)
	{
		Result.Value = value;
		valid = true;
		OnSubmit();
		valid = false;
	}

	protected override void SetQuickFillEntry(SpritePopupQuickItem item, int value)
	{
		item.SpriteRenderer.sprite = GlobalSpriteList.Instance.sprites[value];
		item.IsFocused.Value = value == currentValue;
	}
}

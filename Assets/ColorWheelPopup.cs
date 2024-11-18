using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorWheelPopup : InputPopup<ColorWheelPopup, Color32, ColorWheelPopup.InputOptions, ColorWheelQuickItem>
{
	public class InputOptions : InputPopupShowOptions<Color32>
	{
		public Color InitialColor;
	}

	protected override void OnShow(InputOptions options)
	{
		SetToColor(options.InitialColor);
	}

	public SDispatcher<float> Hue;
	public SDispatcher<float> Saturation;
	public SDispatcher<float> Value;

	public SDispatcher<Color> PreviewHueColor;
	public SDispatcher<Color> PreviewSatColor;
	public SDispatcher<Color> PreviewValColor;
	public SDispatcher<string> ColorString;

	public Image HSVGradient;
	public Image SaturationGradient;
	public RectTransform Nob;

	public Slider HueSlider;
	public Slider SaturationSlider;
	public Slider ValueSlider;

	private void SetToColor(Color color)
	{
		Color.RGBToHSV(color, out float h, out float s, out float v);
		Hue.Value = h;
		Saturation.Value = s;
		Value.Value = v;

		HueSlider.value = h;
		SaturationSlider.value = s;
		ValueSlider.value = v;
	}

	public void OnHueChanged(float value)
	{
		Hue.Value = value;
		UpdatePreview();
	}
	
	public void OnSaturationChanged(float value)
	{
		Saturation.Value = value;
		UpdatePreview();
	}

	public void OnValueChanged(float value)
	{
		Value.Value = value;
		UpdatePreview();
	}

	public void OnTouchAreaMoved(UnityEngine.EventSystems.BaseEventData eventData)
	{
		if (eventData is UnityEngine.EventSystems.PointerEventData mouseData &&
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Nob, mouseData.position, mouseData.pressEventCamera, out Vector2 localPos))
		{
			Saturation.Value = Mathf.Clamp01((localPos.x + Nob.sizeDelta.x / 2) / Nob.sizeDelta.x);
			Value.Value = Mathf.Clamp01((localPos.y + Nob.sizeDelta.y / 2) / Nob.sizeDelta.y);
			UpdatePreview();
		}
	}

	protected override void UpdatePreview()
	{
		Color32 color = Color.HSVToRGB(Hue, Saturation, Value);
		Result.Value = color;
		PreviewHueColor.Value = Color.HSVToRGB(Hue, 1, 1);
		PreviewSatColor.Value = Color.HSVToRGB(Hue, Saturation, 1);
		PreviewValColor.Value = Color.HSVToRGB(0, 0, Value);

		ColorString.Value = $"RBG( {color.r}, {color.b}, {color.g} ) #{ColorUtility.ToHtmlStringRGB(color)}";

		Nob.anchorMin = new Vector2(Saturation, Value);
		Nob.anchorMax = new Vector2(Saturation, Value);

		HSVGradient.materialForRendering.SetFloat("_Hue", Hue);
		SaturationGradient.materialForRendering.SetFloat("_Hue", Hue);

		base.UpdatePreview();
	}

	protected override int ValueSortScore(Color32 value)
	{
		Vector3 v_as_vec3 = new Vector3(value.r, value.g, value.b) / 255f;
		Vector3 result_as_vec3 = new Vector3(Result.Value.r, Result.Value.g, Result.Value.b) / 255f;
		float normalizedDistance = Vector3.Distance(v_as_vec3, result_as_vec3) / Mathf.Sqrt(3);
		return (int)(normalizedDistance * int.MaxValue);
	}

	protected override void QuickFillClicked(Color32 value)
	{
		SetToColor(value);
	}

	protected override void SetQuickFillEntry(ColorWheelQuickItem item, Color32 value)
	{
		item.Preview.text = $"#{ColorUtility.ToHtmlStringRGB(value)}";
		item.Color.Value = value;
	}

	public static Color RandomColor => UnityEngine.Random.ColorHSV(0, 1, 0.8f, 1, 0.8f, 1);
}

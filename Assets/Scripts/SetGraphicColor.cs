using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class SetGraphicColor : MonoBehaviour
{
	private Graphic m_graphic;
	public Graphic Graphic => m_graphic != null ? m_graphic : m_graphic = GetComponent<Graphic>();

	[Min(0)] public float Saturation = 1;
	[Min(0)] public float Value = 1;

	public void SetColor(Color color)
	{
		Color.RGBToHSV(color, out float h, out float s, out float v);
		color = Color.HSVToRGB(h, Saturation * s, Value * v);
		Graphic.color = color;
	}
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ToggleGraphicColor : MonoBehaviour
{
	private Graphic m_graphic;
	public Graphic Graphic => m_graphic != null ? m_graphic : m_graphic = GetComponent<Graphic>();

	public Color On;
	public Color Off;

	public void SetColor(bool on)
	{
		Graphic.color = on ? On : Off;
	}

	public void Toggle()
	{
		SetColor(!Graphic.color.Equals(On));
	}
}
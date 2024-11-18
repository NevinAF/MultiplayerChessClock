using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ColorToggle : MonoBehaviour
{
	private Graphic m_graphic;
	public Graphic Graphic => m_graphic != null ? m_graphic : m_graphic = GetComponent<Image>();

	public Color On;
	public Color Off;

	private bool m_isOn;
	public bool isOn
	{
		get => m_isOn;
		set
		{
			m_isOn = value;
			Graphic.color = m_isOn ? On : Off;
		}
	}

	public void Toggle()
	{
		isOn = !isOn;
	}

	public void SetOnColor(Color color)
	{
		On = color;
		if (isOn)
			Graphic.color = On;
	}

	public void SetOffColor(Color color)
	{
		Off = color;
		if (!isOn)
			Graphic.color = Off;
	}

	private void OnValidate()
	{
		if (Graphic == null)
			return;

		if (isOn)
			Graphic.color = On;
		else
			Graphic.color = Off;
	}
}
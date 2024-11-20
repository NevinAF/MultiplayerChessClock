using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class TimerColorController : MonoBehaviour
{
	private Graphic m_graphic;
	public Graphic Graphic => m_graphic != null ? m_graphic : m_graphic = GetComponent<Graphic>();

	public Color On_Positive;
	public Color On_Negative;
	public Color Off_Positive;
	public Color Off_Negative;

	private bool isNegative;
	private bool lastValue;

	public void SetOn(bool no)
	{
		lastValue = no;
		SetColor();
	}

	public void SetText(string text)
	{
		bool negative = text != null && text.StartsWith("-");
		if (negative != isNegative)
		{
			isNegative = negative;
			SetColor();
		}
	}

	private void SetColor()
	{
		if (isNegative)
		{
			Graphic.color = lastValue ? On_Negative : Off_Negative;
		}
		else
		{
			Graphic.color = lastValue ? On_Positive : Off_Positive;
		}
	}
}
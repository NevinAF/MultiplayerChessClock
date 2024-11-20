using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteToggle : MonoBehaviour
{
	private Image m_image;
	public Image Image => m_image != null ? m_image : m_image = GetComponent<Image>();

	public Sprite On;
	public Sprite Off;

	private bool m_isOn;
	public bool isOn
	{
		get => m_isOn;
		set
		{
			m_isOn = value;
			Image.sprite = m_isOn ? On : Off;
		}
	}

	public void Toggle()
	{
		isOn = !isOn;
	}

	public void SetOnSprite(Sprite sprite)
	{
		On = sprite;
		if (isOn)
			Image.sprite = On;
	}

	public void SetOffSprite(Sprite sprite)
	{
		Off = sprite;
		if (!isOn)
			Image.sprite = Off;
	}

	private void OnValidate()
	{
		if (Image == null)
			return;

		if (isOn)
			Image.sprite = On;
		else
			Image.sprite = Off;
	}
}
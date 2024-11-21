using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteToggle : MonoBehaviour
{
	private Image m_image;
	public Image Image => m_image != null ? m_image : m_image = GetComponent<Image>();

	public Sprite On;
	public Sprite Off;

	[SerializeField] private bool m_isOn;

	public void SetOnSprite(Sprite sprite)
	{
		On = sprite;
		if (m_isOn)
			Image.sprite = On;
	}

	public void SetOffSprite(Sprite sprite)
	{
		Off = sprite;
		if (!m_isOn)
			Image.sprite = Off;
	}

	public void SetIsOn(bool isOn)
	{
		m_isOn = isOn;
		if (Image == null)
			return;

		if (m_isOn)
			Image.sprite = On;
		else
			Image.sprite = Off;
	}

	private void OnValidate()
	{
		if (Image == null)
			return;

		if (m_isOn)
			Image.sprite = On;
		else
			Image.sprite = Off;
	}
}
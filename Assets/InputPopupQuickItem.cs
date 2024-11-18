using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class InputPopupQuickItem : MonoBehaviour
{
	// [HideInInspector] public int Index;
	private Button m_button;
	public Button Button => m_button != null ? m_button : m_button = GetComponent<Button>();
	public Image Icon;

	public virtual void SetIcon(InputQuickFillIconType type)
	{
		Icon.sprite = GlobalSpriteList.GetIcon(type);
	}
}
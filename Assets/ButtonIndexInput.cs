using UnityEngine;
using UnityEngine.UI;

public class ButtonIndexInput : MonoBehaviour
{
	public IntToBoolEvent IntToBoolEvent;
	public TMPro.TMP_Text Text;
	public Button Button;

	public void SetIndex(int index)
	{
		Text.text = $"#{index}";
		IntToBoolEvent.value = index;
	}
}
using UnityEngine;

public class SliderIncrementButtons : MonoBehaviour
{
	public UnityEngine.UI.Slider Slider;
	public float Increment = 1f;

	public void IncrementSlider()
	{
		Slider.value += Increment;
	}

	public void DecrementSlider()
	{
		Slider.value -= Increment;
	}
}
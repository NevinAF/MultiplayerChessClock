using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
	public SDispatcher<string> Name;
	public SDispatcher<string> Desc;
	public SDispatcher<int> IconIndex;
	public SDispatcher<int> PlayerCount;
	[NonSerialized] public double lastUpdateTime;

	public SDispatcher<Sprite> Icon;
	public SDispatcher<string> HumanReadableLastUpdateTime;

	public Button m_button;
	public Button Button => m_button != null ? m_button : m_button = GetComponent<Button>();

	private void OnEnable()
	{
		IconIndex.PipeTo(Icon, GlobalSpriteList.GetSprite);
	}

	private void Update()
	{
		HumanReadableLastUpdateTime.Value = Formatting.Time(Time.timeAsDouble - lastUpdateTime) + " ago";
	}
}

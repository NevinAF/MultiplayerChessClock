using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
	public TMPro.TMP_Text IP;

	public Button m_button;
	public Button Button => m_button != null ? m_button : m_button = GetComponent<Button>();

	public double TimeoutAt;
}
